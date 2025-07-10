using System;

namespace Enterprise.FaxRouter.MailSecurity
{
	public interface ICryptographicProvider
	{
		/// <summary>
		/// Generates a cryptographic key based on the specified parameters.
		/// </summary>
		/// <param name="inputData">The input data used for generating the cryptographic key.</param>
		/// <param name="dateTime">The date and time information, formatted as a string, to be included in the key generation process.</param>
		/// <param name="keyType">The type of cryptographic key to generate.</param>
		/// <returns>A string representation of the cryptographic key.</returns>
		string GenerateKey(byte[] inputData, string dateTime, CryptKeyType keyType);

		/// <summary>
		/// Generates a cryptographic key based on the contents of a file and a specific date and time.
		/// </summary>
		/// <param name="filePath">The name of the file whose contents are used in the key generation process.</param>
		/// <param name="dateTime">The date and time used in the key generation process.</param>
		/// <param name="keyType">The type of cryptographic key to generate; set to <see cref="CryptKeyType.FAX"/> by default.</param>
		/// <returns>A string representation of the cryptographic key.</returns>
		string GenerateKeyFromFile(string filePath, DateTime dateTime, CryptKeyType keyType = CryptKeyType.FAX);

		/// <summary>
		/// Retrieves the date and time format string used for cryptographic operations.
		/// </summary>
		/// <returns>The date and time format string.</returns>
		string GetDateTimeFormat();

		/// <summary>
		/// Retrieves the constant cryptographic key based on the specified type.
		/// </summary>
		/// <param name="type">The type of key to retrieve.</param>
		/// <returns>A string representation of the cryptographic key.</returns>
		string GetCryptKey(CryptKeyType type);
	}
}
