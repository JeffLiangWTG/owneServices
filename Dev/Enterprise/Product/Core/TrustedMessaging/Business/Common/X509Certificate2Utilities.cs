using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TrustedMessaging.Business
{
	public static class X509Certificate2Utilities
	{
		public static X509Certificate2 TryMakeCertificate(byte[] bytes, string password, bool includePrivateKey)
		{
			X509Certificate2 result = null;
			var file = TempFile.New();
			try
			{
				File.WriteAllBytes(file.Filename, bytes);
				if (includePrivateKey)
				{
					result = new X509Certificate2(file.Filename, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);
				}
				else
				{
					result = new X509Certificate2(file.Filename);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce($"Trusted Messaging Error - {e.Message}", e.Message, e);
			}
			finally
			{
				TempFile.TryDeleteHandleAllExceptions(file.Filename, out _);
				if (!file.IsDisposed)
				{
					file.Dispose();
				}
			}
			return result;
		}
	}
}
