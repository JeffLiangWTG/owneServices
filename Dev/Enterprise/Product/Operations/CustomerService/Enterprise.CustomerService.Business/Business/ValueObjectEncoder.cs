using System.IO;
using CargoWise.IO;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.CustomerService.Business
{
	public static class ValueObjectEncoder
	{
		public static string Encrypt<T>(T valueObject) where T : IValueObject
		{
			string xmlString = Serialize(valueObject);
			return EncryptText(xmlString);
		}

		public static T Decrypt<T>(string encryptedText) where T : IValueObject
		{
			string xmlData = DecryptText(encryptedText);
			return Deserialize<T>(xmlData);
		}

		public static string Serialize<T>(T valueObject) where T : IValueObject
		{
			using (MemoryStream xmlStream = new MemoryStream())
			{
				ZXmlSerializer serializer = ZXmlSerializer.New(typeof(T));
				serializer.Serialize(xmlStream, valueObject);
				return StreamConverter.StreamToString(xmlStream);
			}
		}

		public static T Deserialize<T>(string xmlData) where T : IValueObject
		{
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(T));
			return (T)serializer.Deserialize(new StringReader(xmlData));
		}

		public static string EncryptText(string text)
		{
			var decryptor = TwoWayEncoder.NewWithStandardInitialisationVector();
			return decryptor.Encrypt(text);
		}

		public static string DecryptText(string encryptedText)
		{
			var decryptor = TwoWayEncoder.NewWithStandardInitialisationVector();
			return decryptor.Decrypt(encryptedText);
		}
	}
}
