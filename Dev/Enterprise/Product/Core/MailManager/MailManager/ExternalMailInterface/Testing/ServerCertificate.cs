#if DEBUG
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Enterprise.MailManager.ExternalMailInterface
{
	static class ServerCertificate
	{
		public static X509Certificate GetSSLCertificate()
		{
			X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
			store.Open(OpenFlags.ReadOnly);
			X509CertificateCollection cert;
			try
			{
				cert = store.Certificates.Find(X509FindType.FindBySubjectName, CertificateSubject, false);
			}
			finally
			{
				store.Close();
			}
			if (cert.Count == 0)
			{
				InstallSSLCertificate();
				store.Open(OpenFlags.ReadOnly);
				try
				{
					cert = store.Certificates.Find(X509FindType.FindBySubjectName, CertificateSubject, false);
				}
				finally
				{
					store.Close();
				}
			}
			return cert[0];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		static void InstallSSLCertificate()
		{
			var script = @"New-SelfSignedCertificate -KeyExportPolicy Exportable -Subject " + CertificateSubject + @" -NotBefore 01/01/2000 -NotAfter 01/01/2039 -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.1') -KeyAlgorithm RSA -KeyLength 2048 -CertStoreLocation 'Cert:\LocalMachine\My'";

			using (Process process = new Process())
			{
				process.StartInfo.FileName = "powershell.exe";
				process.StartInfo.Arguments = script;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.Verb = "runas";

				StringBuilder output = new StringBuilder();
				StringBuilder error = new StringBuilder();

				using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
				using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
				{
					process.OutputDataReceived += (sender, e) =>
					{
						if (e.Data == null)
						{
							outputWaitHandle.Set();
						}
						else
						{
							output.AppendLine(e.Data);
						}
					};
					process.ErrorDataReceived += (sender, e) =>
					{
						if (e.Data == null)
						{
							errorWaitHandle.Set();
						}
						else
						{
							error.AppendLine(e.Data);
						}
					};

					process.Start();

					process.BeginOutputReadLine();
					process.BeginErrorReadLine();

					if (process.WaitForExit(60 * 1000) && outputWaitHandle.WaitOne(5000) && errorWaitHandle.WaitOne(5000))
					{
						if (process.ExitCode != 0)
						{
							throw new Exception(string.Format("Process exited with code {0}. Stdout: {1}. Stderr: {2}", process.ExitCode, output.ToString(), error.ToString()));
						}
					}
					else
					{
						throw new Exception("makecert process did not exit within 1 minute");
					}
				}
			}
		}

		const string CertificateSubject = "MailManagerTesting";
	}
}
#endif
