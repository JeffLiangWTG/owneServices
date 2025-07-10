using System;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Client
{
	public static class ProtectedDataHelper
	{
		public static string Protect(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			try
			{
				return Convert.ToBase64String(ProtectedData.Protect(Encoding.Unicode.GetBytes(value), null, DataProtectionScope.LocalMachine));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return value;
			}
		}

		public static string Unprotect(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			try
			{
				return Encoding.Unicode.GetString(ProtectedData.Unprotect(Convert.FromBase64String(value), null, DataProtectionScope.LocalMachine));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return value;
			}
		}
	}
}
