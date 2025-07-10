using System.Security.Cryptography;
using System.Text;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.RSACryptography.Testing
{
	public class RSASecurityProviderTest : TestCaseWithFactory
	{
		public void TestEncryptorAndDecryptor()
		{
			var text = "Some random text for testing only";
			var encrypted = provider.Encryptor.Encrypt(text);
			var decrypted = provider.Decryptor.Decrypt(encrypted);
			AssertEquals(text, decrypted);
		}

		public void TestSignatureSignAndVerifyHash()
		{
			var text = "Some random text for testing only, Some random text for testing only, Some random text for testing only";
			var signature = provider.SignatureSigner.SignHashAsBase64String(text); //Base64 string
			Assert(provider.SignatureVerifier.VerifyHash(text, signature));
		}

		public void TestSignatureSignAndVerifyData()
		{
			var text = "Some random text for testing only";
			var signature = provider.SignatureSigner.SignData(text); //Base64 string
			Assert(provider.SignatureVerifier.VerifyData(text, signature));
		}

		public void TestSignAndVerifyWithASCII()
		{
			var privateKey = "<RSAKeyValue><Modulus>zFp32wGZy5AStfmdHddNrafBm0kkmUj/LosWb0/2GtCDXs++Y0tmaS/t7PicP1Uozon6k9FXTdjncc/Rb7Zl9hJJjpjoWyesoXEH/raurd3BMMsByfS2tkv5uu02fcABVkwPiImBkJXpIWKxubxbiZMq9qDwwCZaPHuWbBesPUU=</Modulus><Exponent>AQAB</Exponent><P>0tF3WCmxHYSzul/DbI99tol8X5HxQylMeskkb9Y+X+JWc+ou8MbI5i/ujMAuWT7xhunta/2b/gsLQXr57JFFSw==</P><Q>+CZPHDmKli+gkLZLvNORA/BgXqOG9q7AvrjIG9HuuyGSQf4h+iXk+WWpHE67HgScFZQ6fkODz6wehHCVTrE9rw==</Q><DP>oAMrEXi7nUsO26Q6AVk8MuNRynxMYgyjpwKqrFQyDbcOpXaXYFuROt4gsyZfR4/15NADTBc3YnBhf7bmoX911w==</DP><DQ>1lVQc1KID1yn1RZ/qGMcmEhhFJ0uD5e1R4aW4OCc0OkYSQPWGRfuBDP4s0iVekIFBuZ292QdT5yc50vOyp9wZQ==</DQ><InverseQ>Ge27b4riL8GtcNMYgSBEGTYi+n0vEJLvjZNKGJqSAz/XSqfkA4R6NKkw4267Bb6JLmPkOmtPS7MoJJeQx4uTqg==</InverseQ><D>jCajuuUfKFg4LOvz0KqAENBT3P9OBX7l3HLxwQfTHtLQtm6+AXWN2ChSAksDRgBOy1AgNc7GFJLlMM45smcjB2T+dshtMIQl0CttAUEjkxHr8n4QhyElYklTN4z6wN5XHRh07xVowRAQvQ0+1537dAfavq7WRp9jwAouE+lq7aU=</D></RSAKeyValue>";
			var publicKey = "<RSAKeyValue><Modulus>zFp32wGZy5AStfmdHddNrafBm0kkmUj/LosWb0/2GtCDXs++Y0tmaS/t7PicP1Uozon6k9FXTdjncc/Rb7Zl9hJJjpjoWyesoXEH/raurd3BMMsByfS2tkv5uu02fcABVkwPiImBkJXpIWKxubxbiZMq9qDwwCZaPHuWbBesPUU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
			ASCIIEncoding byteConverter = new ASCIIEncoding();
			string dataString = "Data to Sign";

			// Create byte arrays to hold original, encrypted, and decrypted data.
			byte[] originalData = byteConverter.GetBytes(dataString);

			RSACryptoServiceProvider rSAalgSign = new RSACryptoServiceProvider();
			rSAalgSign.FromXmlString(privateKey);

			// Hash and sign the data. Pass an instance of the default implementation of SHA1 which is SHA1CryptoServiceProvider
			// to specify the use of SHA1 for hashing.
			var signedData = rSAalgSign.SignData(originalData, SHA1.Create());

			// Create a new instance of RSACryptoServiceProvider using the 
			// key from RSAParameters.
			RSACryptoServiceProvider rSAalgVerify = new RSACryptoServiceProvider();

			rSAalgVerify.FromXmlString(publicKey);
			// Verify the data using the signature.  Pass an instance of the default implementation of SHA1 which is SHA1CryptoServiceProvider
			// to specify the use of SHA1 for hashing.
			Assert(rSAalgVerify.VerifyData(originalData, SHA1.Create(), signedData));
		}

		public void TestSignAndVerifyWithUTF8()
		{
			var privateKey = "<RSAKeyValue><Modulus>zFp32wGZy5AStfmdHddNrafBm0kkmUj/LosWb0/2GtCDXs++Y0tmaS/t7PicP1Uozon6k9FXTdjncc/Rb7Zl9hJJjpjoWyesoXEH/raurd3BMMsByfS2tkv5uu02fcABVkwPiImBkJXpIWKxubxbiZMq9qDwwCZaPHuWbBesPUU=</Modulus><Exponent>AQAB</Exponent><P>0tF3WCmxHYSzul/DbI99tol8X5HxQylMeskkb9Y+X+JWc+ou8MbI5i/ujMAuWT7xhunta/2b/gsLQXr57JFFSw==</P><Q>+CZPHDmKli+gkLZLvNORA/BgXqOG9q7AvrjIG9HuuyGSQf4h+iXk+WWpHE67HgScFZQ6fkODz6wehHCVTrE9rw==</Q><DP>oAMrEXi7nUsO26Q6AVk8MuNRynxMYgyjpwKqrFQyDbcOpXaXYFuROt4gsyZfR4/15NADTBc3YnBhf7bmoX911w==</DP><DQ>1lVQc1KID1yn1RZ/qGMcmEhhFJ0uD5e1R4aW4OCc0OkYSQPWGRfuBDP4s0iVekIFBuZ292QdT5yc50vOyp9wZQ==</DQ><InverseQ>Ge27b4riL8GtcNMYgSBEGTYi+n0vEJLvjZNKGJqSAz/XSqfkA4R6NKkw4267Bb6JLmPkOmtPS7MoJJeQx4uTqg==</InverseQ><D>jCajuuUfKFg4LOvz0KqAENBT3P9OBX7l3HLxwQfTHtLQtm6+AXWN2ChSAksDRgBOy1AgNc7GFJLlMM45smcjB2T+dshtMIQl0CttAUEjkxHr8n4QhyElYklTN4z6wN5XHRh07xVowRAQvQ0+1537dAfavq7WRp9jwAouE+lq7aU=</D></RSAKeyValue>";
			var publicKey = "<RSAKeyValue><Modulus>zFp32wGZy5AStfmdHddNrafBm0kkmUj/LosWb0/2GtCDXs++Y0tmaS/t7PicP1Uozon6k9FXTdjncc/Rb7Zl9hJJjpjoWyesoXEH/raurd3BMMsByfS2tkv5uu02fcABVkwPiImBkJXpIWKxubxbiZMq9qDwwCZaPHuWbBesPUU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
			UTF8Encoding byteConverter = new UTF8Encoding();
			string dataString = "Data to Sign";

			// Create byte arrays to hold original, encrypted, and decrypted data.
			byte[] originalData = byteConverter.GetBytes(dataString);

			RSACryptoServiceProvider rSAalgSign = new RSACryptoServiceProvider();
			rSAalgSign.FromXmlString(privateKey);

			// Hash and sign the data. Pass an instance of the default implementation of SHA1 which is SHA1CryptoServiceProvider
			// to specify the use of SHA1 for hashing.
			var signedData = rSAalgSign.SignData(originalData, SHA1.Create());

			// Create a new instance of RSACryptoServiceProvider using the 
			// key from RSAParameters.
			RSACryptoServiceProvider rSAalgVerify = new RSACryptoServiceProvider();
			rSAalgVerify.FromXmlString(publicKey);
			// Verify the data using the signature.  Pass an instance of the default implementation of SHA1 which is SHA1CryptoServiceProvider
			// to specify the use of SHA1 for hashing.
			Assert(rSAalgVerify.VerifyData(originalData, SHA1.Create(), signedData));
		}

		readonly RSASecurityProvider provider = new RSASecurityProvider();
	}
}
