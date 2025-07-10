using System;

namespace Enterprise.Registry.Business;

internal class NoProtectionDataProtectorService : IDataProtectorService
{
	public string Encrypt(string textToEncrypt) => null;
	public string Decrypt(string encryptedString) => throw new NotImplementedException("Decrypt is not implemented");
}
