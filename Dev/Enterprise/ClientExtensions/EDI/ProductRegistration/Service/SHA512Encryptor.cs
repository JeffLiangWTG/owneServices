using System;
using System.Security.Cryptography;
using System.Text;

namespace CargoWise.ProductRegistration.Service
{
	public static class SHA512Encryptor
	{
		public static string Encrypt(string data)
		{
			using (var provider = SHA512.Create())
			{
				return BitConverter.ToString(provider.ComputeHash(UTF8Encoding.Default.GetBytes(data)));
			}
		}
	}
}
