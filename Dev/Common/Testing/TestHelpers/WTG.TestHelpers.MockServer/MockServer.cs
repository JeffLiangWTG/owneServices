using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WTG.TestHelpers.MockServer
{
	public abstract class MockServer : IDisposable
	{
		string UnitTestCertificateSubjectIdentifier => $"CW Unit Test {ServiceName}";

		int port;
		bool isDisposed;

		readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
		readonly ManualResetEvent threadStopped = new ManualResetEvent(false);

		public string BaseUrl => $"https://{HostName}:{Port}";

		public int Port => port;
		public string ClientIdentifier { get; set; } = Guid.NewGuid().ToString();
		public string Sub { get; set; } = Guid.NewGuid().ToString();

		public static string HostName => Dns.GetHostEntry(Environment.MachineName).HostName.ToLowerInvariant();
		public string ServiceName { get; private set; }

		readonly ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception>();
		public Exception[] Exceptions => exceptions.ToArray();
		public bool TokensAreExpired { get; set; }

		Task processRequestsTask;

		protected MockServer(string serviceName)
		{
			ServiceName = serviceName;
			var httpListener = CreateAndStartListener();
			RemoveCerts();
			UnbindCertFromPort(Port);
			CreateAndTrustCert(Port);
			ProcessRequests(httpListener);
		}

		public void Dispose()
		{
			if (!isDisposed)
			{
				UnbindCertFromPort(Port);
				RemoveCerts();
				tokenSource.Cancel();
				tokenSource.Dispose();
				isDisposed = true;
				threadStopped.WaitOne();
				threadStopped.Dispose();
				processRequestsTask.Wait();
			}
		}

		public string GetFullUrl(string path)
		{
			var baseUri = new Uri(BaseUrl);
			var fullUri = new Uri(baseUri, path);
			return fullUri.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5403:Do not hard-code certificate", Justification = "need to trust the certifciate")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5380", Justification = "need to trust the certifciate")]
		void CreateAndTrustCert(int port)
		{
			using (var parent = RSA.Create(4096))
			using (var rsa = RSA.Create(2048))
			{
				var req = new CertificateRequest($"cn=Unit Test Root CA,OU={UnitTestCertificateSubjectIdentifier}", parent, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				req.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
				req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

				using (var rootCA = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5)))
				using (var rootCAWithPersistedKey = new X509Certificate2(rootCA.Export(X509ContentType.Pkcs12, ""), "", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet))
				{
					using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
					{
						store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
						store.Add(rootCAWithPersistedKey);
					}

					var req2 = new CertificateRequest($"cn={HostName},OU={UnitTestCertificateSubjectIdentifier}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
					req2.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
					req2.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
					req2.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, true));
					req2.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req2.PublicKey, false));

					SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
					sanBuilder.AddDnsName(HostName);
					req2.CertificateExtensions.Add(sanBuilder.Build());

					using (var serverCert = req2.Create(rootCAWithPersistedKey, DateTimeOffset.Now, DateTimeOffset.UtcNow.AddDays(90), new byte[] { 1, 2, 3, 4 }))
					using (var serverCertWithPrivateKey = serverCert.CopyWithPrivateKey(rsa))
					using (var serverCertWithPersistedKey = new X509Certificate2(serverCertWithPrivateKey.Export(X509ContentType.Pkcs12, ""), "", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet))
					{
						using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
						{
							store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
							store.Add(serverCertWithPersistedKey);
						}

						ExecuteShellCommand("netsh", $"http add sslcert ipport=0.0.0.0:{port} certhash={serverCertWithPersistedKey.GetCertHashString()} appid={{{Guid.NewGuid()}}}");
					}
				}
			}
		}

		void UnbindCertFromPort(int port)
		{
			ExecuteShellCommand("netsh", $"http delete sslcert ipport=0.0.0.0:{port}");
		}

		void RemoveCerts()
		{
			using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
			{
				store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
				var unitTestCerts = store.Certificates.Find(X509FindType.FindBySubjectName, UnitTestCertificateSubjectIdentifier, false);
				foreach (var cert in unitTestCerts)
				{
					store.Remove(cert);
				}
			}

			using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
			{
				store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
				var unitTestCerts = store.Certificates.Find(X509FindType.FindBySubjectName, UnitTestCertificateSubjectIdentifier, false);
				foreach (var cert in unitTestCerts)
				{
					store.Remove(cert);
				}
			}
		}

		int ExecuteShellCommand(string cmd, string args)
		{
			using (var process = new Process())
			{
				process.StartInfo.FileName = cmd;
				process.StartInfo.Arguments = args;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;

				process.Start();
				process.WaitForExit();
				return process.Id;
			}
		}

		HttpListener CreateAndStartListener()
		{
			const int minPort = 49215;
			const int maxPort = 65535;
			var hostName = HostName;
			HttpListener httpListener = null;

			for (port = minPort; port < maxPort; port++)
			{
				httpListener = new HttpListener();
				httpListener.Prefixes.Add($"https://{hostName}:{port}/");
				try
				{
					httpListener.Start();
					break;
				}
				catch
				{
					// ignored
				}
			}

			if (!httpListener.IsListening)
			{
				throw new Exception("Couldn't start listener");
			}

			return httpListener;
		}

		protected string GetRequestBody(HttpListenerRequest request)
		{
			using (var body = request.InputStream)
			{
				var encoding = request.ContentEncoding;

				using (var reader = new StreamReader(body, encoding))
				{
					return reader.ReadToEnd();
				}
			}
		}

		void ProcessRequests(HttpListener httpListener)
		{
			processRequestsTask = Task.Run(() =>
			{
				var cancelToken = tokenSource.Token;
				try
				{
					while (true)
					{
						var task = httpListener.GetContextAsync();

						try
						{
							Task.WaitAny(new Task[] { task }, cancelToken);
						}
						catch (OperationCanceledException)
						{
							httpListener.Stop();
							try
							{
								task.GetAwaiter().GetResult();
							}
							catch
							{
								// ignored
							}

							return;
						}

						var context = task.GetAwaiter().GetResult();
						try
						{
							var response = CreateMockResponse(context.Request);
							context.Response.SendChunked = false;
							context.Response.StatusCode = response.StatusCode;
							context.Response.ContentType = response.ContentType;

							var bytes = Encoding.UTF8.GetBytes(response.Content);
							context.Response.ContentLength64 = bytes.Length;
							context.Response.OutputStream.Write(bytes, 0, bytes.Length);
							context.Response.Close();
						}
						catch (WebException)
						{
							context.Response.Abort();
						}
					}
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
				finally
				{
					threadStopped.Set();
				}
			});
		}

		protected abstract IMockResponse CreateMockResponse(HttpListenerRequest request);
	}
}
