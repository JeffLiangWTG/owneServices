using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace CargoWise.ProductRegistration.Service
{
	/// <summary>
	/// Signs Product Registration XML documents.
	/// Contains a private key so should not be distributed outside our company.
	/// The distributable public key is in Enterprise.ProductRegistration.Common.SignedMessage.
	/// </summary>
	/// <seealso cref="http://msdn.microsoft.com/en-us/library/ms229745(v=vs.85).aspx"/>
	public static class MessageSigner
	{
		static RSACryptoServiceProvider InitRSACryptoServiceProvider()
		{
			var result = new RSACryptoServiceProvider();
			result.FromXmlString(PrivateKeyXml);
			return result;
		}

		static readonly RSACryptoServiceProvider rsaKey = InitRSACryptoServiceProvider();

		public static string SignXml(string unsignedXml)
		{
			var doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.LoadXml(unsignedXml);
			var signedXml = new SignedXml(doc);
			signedXml.SigningKey = rsaKey;

			Reference reference = new Reference();
			reference.Uri = "";
			var env = new XmlDsigEnvelopedSignatureTransform();
			reference.AddTransform(env);
			signedXml.AddReference(reference);
			signedXml.ComputeSignature();
			var xmlDigitalSignature = signedXml.GetXml();
			var importedNode = doc.ImportNode(xmlDigitalSignature, true);

			if (importedNode != null && doc.DocumentElement != null)
			{
				doc.DocumentElement.AppendChild(importedNode);
			}

			return doc.OuterXml;
		}

		public const string PrivateKeyXml = @"<RSAKeyValue><Modulus>t9jeFIoBnd8kKMhpOCGyvzKdKPAzB+ruNmAr3v5aSyKUQPvxM4LQTb8Twt4FjY0OQPvaQdXJEIRTRjYVzq/qNR1hCzC3QsawaYD1L9IXAH/98MFPTFJFv1tg0CGlfGsNUU/Zl8in35v4TNqm9tQ+mc0hDLbHZAAbrYgrGTLoP00=</Modulus><Exponent>AQAB</Exponent><P>t9pRGLoQ38C+6bJPYRBgSbhLWZf5A6wW5rvDbV5bASDSUNeLErCXR5f9tMFB0lRcfWM3LXT++WzJ7u/iebdXRw==</P><Q>//37Y9mfGQr1Kl3bMUfhAY71T9/mP0RIGIZNdMenyNWHyGHYbW+PeXSxsQ3wh8Yil/pDIOEpYIMnZ0Z1Z6Kmyw==</Q><DP>aG2oDJx7QyyvA/zVG7P6jUUR/5TTy2MvKuXRzkh+9ngXHfYgN4B4nSDW6Zmv8nEai9oUGEzRGwnQ9VfqUupxtw==</DP><DQ>KBeG/6RdnnZw3ynD4nv3aV8SXGgcj1wKToz6JIgAZxvKID/yvXDzb3ovOiOMwDbvxA2V5dhdupfP3ATU7l4y8Q==</DQ><InverseQ>Rox+3y7QKzdRhxSozc+ebergI/Zk0qwLD9fTJeeV3w/YUbbCnqPX3pOGBY339YNVz5cQXABUfPoKAojYZUhxHA==</InverseQ><D>AAta+kxWqOjUVglIkInN92W4syT5bV+ZTdQ1P6/a+y093dNSac5r+X9NvyRJdI8Gnz5RVBTL0GB31re+dXiAJBQz9P9K/eC6slILNmCITbkL0O+rPtHWX5pHhsM5B6HjNYXld6eqrURAZ0fDjDgpeRQf1945QUHReu5B9uTzHoc=</D></RSAKeyValue>";
	}
}
