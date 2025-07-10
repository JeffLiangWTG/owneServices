using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	static class LicenceDatabaseExtensions
	{
		public static bool ValidateClientSecret(this LicenceDatabase database, string clientSecret)
		{
			// The password value in the client's Product Registration document is the actual password (client secret).
			// LD_Password is SHA512(LD_DatabaseNumber + password), to hex with dashes.
			var hashInputString = string.Format(CultureInfo.InvariantCulture, "{0}{1}", database.LD_DatabaseNumber, clientSecret);
			var hashInputBytes = Encoding.UTF8.GetBytes(hashInputString);

			byte[] hashedSecretData;
			using var sha512 = SHA512.Create();
			hashedSecretData = sha512.ComputeHash(hashInputBytes);

			var expectedLDPasswordValue = BitConverter.ToString(hashedSecretData);

			var valuesMatch = string.Equals(database.LD_Password, expectedLDPasswordValue, StringComparison.Ordinal);
			return valuesMatch;
		}
	}
}
