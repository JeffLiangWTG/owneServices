using System;
using System.Linq;
using System.Security.Cryptography;
using Enterprise.Accounting.ElectronicMessaging.Common.Sensitive;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class SensitiveDataEncryptionTest : TestCase
	{
		[TestDate(2025, 5, 27, 12, 12, 12)]
		public void TestEncryptStringAndSerialize_Success()
		{
			var sensitiveEncryptor = CreateSensitive();
			var encrypted = sensitiveEncryptor.EncryptStringAndSerialize(Message, Secret);

			var expected = "SENSITIVE$$1$$AES256_HMACSHA256$$$$20250527T121212Z$$AAECAwQFBgcICQoLDA0ODw==$$4sjf/Pgz2qjb/TzgbTcH3FqRzuavUhX6ft/54xen9ja9OGtdPQPyKQmC6uaHu3tgRsV2KjYflNOTnYqnfyCsY3aijcVPuh/53BoTv6hfxvG56Do2nNKSt6wT2UeQMySk+Z29E6Rvh0v6erJJagUOy/3r3LT9DArcblujBhaAtY+Dxf9QqpF3F/kcA5vZW2g2VG6SqeoG5VhQbuwbNi8UrUU6WuQ/8MV1Qi6+zZCpM2mzMOzbAOK16GJHvgrv2g4w8Grev+Kp8FUKyh5NONLKyFsrlPy6jougyZtrpEnxzhtuZ+f9TYn7UNz448X3M56a305O2kyDRPN2t+hO6eXDhLF89ZlyGn6ve+E67YdEhD0UWjY50dtRlQnuYzEhZRbH$$319E8C53EA201A981BE826178873011DF0F94EFADA92C104185994C614053D43";
			AssertEquals(expected, encrypted);
		}

		public void TestEncryptAndSerialize_Failure_SecretTooShort()
		{
			var sensitiveEncryptor = CreateSensitive();
			AssertExceptionThrown<CryptographicException>(() => sensitiveEncryptor.EncryptStringAndSerialize("test", FourBytes));
		}

		SensitiveDataEncryption CreateSensitive()
		{
			return new SensitiveDataEncryption(new TestRandomNumberGenerator());
		}

		readonly string Message = "Gj5nS9wyYHRadaVffz5VKB4v4wlVWyPhcJvrTD4NHtOsmn4WEhkrnJW7zWOmQD7qUDLVWZqNROnJk8ElUkfT38xh/EJq5G6ISXrLBHLNsIPftpZnzpinZWhURwJ/iZVCxvxWI89aqV5fvPThtItXW5iF58GCqaaJWv4ZKTNnk0cF+0vWfrJqS/6yBvnz6CrBNnRxo2AMgRzrvLgx9buDOtlT6wCwzq7VIK/Q8YlxyQPUAr121WPErq7TCHJNikF/ylAfcU1/ac9K9GQyj7AfFQ==";
		readonly ReadOnlyMemory<byte> Secret = Enumerable.Range(0, 64).Select(x => (byte)x).ToArray();
		readonly ReadOnlyMemory<byte> FourBytes = Enumerable.Range(0, 4).Select(x => (byte)x).ToArray();
	}

	class TestRandomNumberGenerator : RandomNumberGenerator
	{
		byte Current;

		public override void GetBytes(byte[] data)
		{
			for (var i = 0; i < data.Length; i++)
			{
				data[i] = Current;
				unchecked
				{
					++Current;
				}
			}
		}
	}
}
