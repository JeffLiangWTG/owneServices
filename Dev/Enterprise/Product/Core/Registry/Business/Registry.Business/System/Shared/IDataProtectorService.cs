namespace Enterprise.Registry.Business
{
	public interface IDataProtectorService
	{
		string Encrypt(string textToEncrypt);
		string Decrypt(string encryptedString);
	}
}
