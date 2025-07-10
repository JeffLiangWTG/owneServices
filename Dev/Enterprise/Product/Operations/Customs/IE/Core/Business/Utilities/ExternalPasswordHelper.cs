using System;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business
{
	public static class ExternalPasswordHelper
	{
		public static ZString GetHashedPassword(ZString password)
		{
			var result = ZString.Empty;
			if (!password.IsEmpty)
			{
				using (var md5 = MD5.Create())
				{
					var passwordBytes = Encoding.ASCII.GetBytes(password);
					var passwordMD5Bytes = md5.ComputeHash(passwordBytes);
					result = Convert.ToBase64String(passwordMD5Bytes);
				}
			}
			return result;
		}
	}
}
