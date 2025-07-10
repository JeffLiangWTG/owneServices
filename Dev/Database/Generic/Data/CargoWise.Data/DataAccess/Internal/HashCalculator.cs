using System.Text;

namespace CargoWise.EntityFramework
{
	public static class HashCalculator
	{
		public static string CalculateMD5Hash(byte[] input)
		{
			if (input == null)
			{
				return string.Empty;
			}

			using (var md5 = System.Security.Cryptography.MD5.Create())
			{
				var hash = md5.ComputeHash(input);

				var builder = new StringBuilder(hash.Length * 2);

				for (int i = 0; i < hash.Length; i++)
				{
					builder.Append(hash[i].ToString("x2"));
				}

				return builder.ToString();
			}
		}
	}
}
