using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Threading;
using CargoWise.IO;

namespace Enterprise.MailManager.FileDownload
{
	public class HttpTestHelper : IDisposable
	{
		public void Start()
		{
			LocalDirectory = Temp.GetNewTempSubdirectory();
			Port = GetPort();
			httpListener.Prefixes.Add(ServerAddress.ToString());
			httpListener.Start();
			listenerThread = new Thread(HandleRequests);
			listenerThread.Start();
		}

		int GetPort()
		{
			var port = DefaultPort;
			while (IsPortInUse(port))
			{
				port = new Random().Next(49152, 65535);
			}
			return port;
		}

		bool IsPortInUse(int port)
		{
			return IPGlobalProperties.GetIPGlobalProperties()?.GetActiveTcpListeners() is { } endPoints &&
				endPoints.Any(ep => ep.Port == port);
		}

		void HandleRequests()
		{
			while (httpListener.IsListening)
			{
				try
				{
					var context = httpListener.GetContext();

					if (!(context.Request.HttpMethod == HttpMethod.Get.Method || context.Request.HttpMethod == HttpMethod.Head.Method))
					{
						context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
						context.Response.Close();
						continue;
					}

					var file = Path.Combine(LocalDirectory, context.Request.Url.AbsolutePath.TrimStart('/'));
					if (File.Exists(file))
					{
						try
						{
							var fileBytes = File.ReadAllBytes(file);
							byte[] responseBytes;

							if (context.Request.Headers["Range"] is null)
							{
								responseBytes = fileBytes;
								context.Response.AddHeader("Accept-Ranges", $"bytes");
								context.Response.StatusCode = (int)HttpStatusCode.OK;
							}
							else
							{
								var range = RangeHeaderValue.Parse(context.Request.Headers["Range"]).Ranges.First();
								var startByte = range.From ?? 0;
								var endByte = range.To ?? fileBytes.Length - 1;
								var length = endByte - startByte + 1;

								responseBytes = new byte[length];
								Array.Copy(fileBytes, startByte, responseBytes, 0, length);
								context.Response.AddHeader("Content-Range", $"bytes {startByte}-{endByte}/{fileBytes.Length}");
								context.Response.StatusCode = (int)HttpStatusCode.PartialContent;
							}

							context.Response.ContentType = MediaTypeNames.Application.Octet;
							context.Response.ContentLength64 = responseBytes.Length;
							context.Response.Headers.Add("Last-Modified", File.GetLastWriteTimeUtc(file).ToString("r"));

							if (context.Request.HttpMethod == HttpMethod.Get.ToString())
							{
								var buffer = new byte[1024];
								var bytesRead = 0;
								using var output = new MemoryStream(responseBytes);
								while ((bytesRead = output.Read(buffer, 0, buffer.Length)) > 0)
								{
									context.Response.OutputStream.Write(buffer, 0, bytesRead);
								}
							}

							context.Response.OutputStream.Flush();
						}
						catch
						{
							context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
						}
						finally
						{
							context.Response.Close();
							RequestComplete();
						}
					}
					else
					{
						context.Response.StatusCode = (int)HttpStatusCode.NotFound;
						context.Response.Close();
						RequestComplete();
					}
				}
				catch (HttpListenerException)
				{
					break;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public void Dispose()
		{
			httpListener.Close();
			listenerThread?.Join();
			if (Directory.Exists(LocalDirectory))
			{
				try
				{
					Directory.Delete(LocalDirectory, true);
				}
				catch (IOException)
				{
					Thread.Sleep(1000);
					Directory.Delete(LocalDirectory, true);
				}
				DateTime deleteTime = DateTime.UtcNow;
				while (Directory.Exists(LocalDirectory) && DateTime.UtcNow.Subtract(deleteTime) < TimeSpan.FromMinutes(2))
				{
					Thread.Sleep(10);
				}
			}
		}

		public string LocalDirectory
		{
			get;
			private set;
		}

		public int Port
		{
			get;
			private set;
		}

		public Uri ServerAddress
		{
			get { return new Uri("http://localhost:" + Port); }
		}

		public bool IsListening => httpListener.IsListening;

		const int DefaultPort = 55555;

		void RequestComplete()
		{
			RequestCount++;
			RequestCompleted?.Invoke(this, EventArgs.Empty);
		}

		public int RequestCount { get; private set; }
		public event EventHandler RequestCompleted;
		readonly HttpListener httpListener = new HttpListener();
		Thread listenerThread;
	}
}
