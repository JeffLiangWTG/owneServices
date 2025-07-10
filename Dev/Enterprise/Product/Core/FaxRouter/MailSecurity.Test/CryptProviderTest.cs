using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.FaxRouter.MailSecurity.Test
{
	public class CryptProviderTest : TestCase
	{
		#region GenerateKey

		public void TestGenerateKey_ValidInput_ReturnsExpectedHex()
		{
			// Arrange
			CryptProvider cryptProvider = new();
			var hashFileElement = Encoding.UTF8.GetBytes("example");
			var dateTime = "20210101";
			var cryptKeyType = CryptKeyType.FAX;

			// Act
			var result = cryptProvider.GenerateKey(hashFileElement, dateTime, cryptKeyType);

			// Assert
			AssertEquals("3E-65-05-B5-47-FA-7B-B7-E5-D3-8C-C2-5F-21-52-38", result);
		}

		public void TestGenerateKey_InvalidHashFileElement_ThrowsArgumentNullException()
		{
			// Arrange
			CryptProvider cryptProvider = new();
			byte[] hashFileElement = null;
			var dateTime = "20210101";
			var cryptKeyType = CryptKeyType.FAX;

			// Act & Assert
			_ = AssertExceptionThrown<ArgumentNullException>(() => cryptProvider.GenerateKey(hashFileElement, dateTime, cryptKeyType));
		}

		public void TestGenerateKey_InvalidDateTime_ThrowsArgumentException()
		{
			// Arrange
			CryptProvider cryptProvider = new();
			var hashFileElement = Encoding.UTF8.GetBytes("example");
			var dateTime = "";
			var cryptKeyType = CryptKeyType.FAX;

			// Act & Assert
			_ = AssertExceptionThrown<ArgumentException>(() => cryptProvider.GenerateKey(hashFileElement, dateTime, cryptKeyType));
		}

		public void TestGenerateKey_DifferentCryptKeyTypes_ProducesDifferentOutputs()
		{
			// Arrange
			CryptProvider cryptProvider = new();
			var hashFileElement = Encoding.UTF8.GetBytes("example");
			var dateTime = "20210101";

			// Act
			var resultFax = cryptProvider.GenerateKey(hashFileElement, dateTime, CryptKeyType.FAX);
			var resultAck = cryptProvider.GenerateKey(hashFileElement, dateTime, CryptKeyType.ACK);

			// Assert
			AssertNotNullOrEmpty(resultFax);
			AssertNotNullOrEmpty(resultAck);
			AssertNotSame(resultFax, resultAck);
		}
		#endregion

		#region GenerateKeyFromFile

		public void TestGenerateKeyFromFile_ValidInput_ReturnsExpectedFAXHex()
		{
			// Arrange
			CryptProvider cryptProvider = new();
			using var tempDirectory = new TempDirectory();
			var filePath = Path.Combine(tempDirectory.DirectoryName, "GenerateKeyFromFile_ValidInput_ReturnsExpectedFAXHex.txt");
			File.WriteAllText(filePath, "example");
			var dateTime = new DateTime(2021, 01, 01);
			var cryptKeyType = CryptKeyType.FAX;
			string result;

			// Act
			try
			{
				result = cryptProvider.GenerateKeyFromFile(filePath, dateTime, cryptKeyType);
			}
			finally
			{
				File.Delete(filePath);
			}

			// Assert
			AssertEquals("42-0D-07-EA-25-9E-11-F6-75-F8-36-59-DC-56-84-E8", result);
		}

		public void TestGenerateKeyFromFile_ValidInput_ReturnsExpectedACKHex()
		{
			// Arrange
			CryptProvider cryptProvider = new();
			using var tempDirectory = new TempDirectory();
			var filePath = Path.Combine(tempDirectory.DirectoryName, "GenerateKeyFromFile_ValidInput_ReturnsExpectedACKHex.txt");
			File.WriteAllText(filePath, "example");
			var dateTime = new DateTime(2021, 01, 01);
			var cryptKeyType = CryptKeyType.ACK;
			string result;

			// Act
			try
			{
				result = cryptProvider.GenerateKeyFromFile(filePath, dateTime, cryptKeyType);
			}
			finally
			{
				File.Delete(filePath);
			}

			// Assert
			AssertEquals("57-94-0A-15-4C-C9-F9-9C-C7-FF-CA-46-73-39-49-6C", result);
		}

		public void TestGenerateKeyFromFile_DifferentCryptKeyTypes_ProducesDifferentOutputs()
		{
			// Arrange
			CryptProvider cryptProvider = new();
			using var tempDirectory = new TempDirectory();
			var faxFilePath = Path.Combine(tempDirectory.DirectoryName, "GenerateKeyFromFile_DifferentCryptKeyTypes_ProducesDifferentOutputsFAX.txt");
			var ackFilePath = Path.Combine(tempDirectory.DirectoryName, "GenerateKeyFromFile_DifferentCryptKeyTypes_ProducesDifferentOutputsACK.txt");
			File.WriteAllText(faxFilePath, "exampleFAX");
			File.WriteAllText(ackFilePath, "exampleACK");
			var dateTime = new DateTime(2021, 01, 01);

			// Act
			var resultFax = cryptProvider.GenerateKeyFromFile(faxFilePath, dateTime, CryptKeyType.FAX);
			var resultAck = cryptProvider.GenerateKeyFromFile(ackFilePath, dateTime, CryptKeyType.ACK);

			// Assert
			AssertNotNullOrEmpty(resultFax);
			AssertNotNullOrEmpty(resultAck);
			AssertNotSame(resultFax, resultAck);
		}

		public void TestGenerateKeyFromFile_NullFilePath_ThrowsArgumentNullException()
		{
			// Arrange
			CryptProvider cryptProvider = new();
			string filePath = null;
			var dateTime = new DateTime(2021, 01, 01);
			var cryptKeyType = CryptKeyType.FAX;

			// Act & Assert
			_ = AssertExceptionThrown<ArgumentNullException>(() =>
				cryptProvider.GenerateKeyFromFile(filePath, dateTime, cryptKeyType));
		}

		public void TestGenerateKeyFromFile_EmptyFilePath_ThrowsArgumentNullException()
		{
			// Arrange
			CryptProvider cryptProvider = new();
			var filePath = string.Empty;
			var dateTime = new DateTime(2021, 01, 01);
			var cryptKeyType = CryptKeyType.FAX;

			// Act & Assert
			_ = AssertExceptionThrown<ArgumentException>(() =>
				cryptProvider.GenerateKeyFromFile(filePath, dateTime, cryptKeyType));
		}
		#endregion

		#region Append

		public void TestAppend_ValidInput_CombinesArrays()
		{
			// Arrange
			var sourceArray = Encoding.UTF8.GetBytes("source");
			var destinationArray = Encoding.UTF8.GetBytes("destination");
			var expectedByteArray = sourceArray.Concat(destinationArray).ToArray();

			// Act
			var result = CryptProvider.Append(sourceArray, destinationArray);

			// Assert
			AssertEquals(sourceArray.Length + destinationArray.Length, result.Length);
			AssertEquals(expectedByteArray, result);
		}

		public void TestAppend_SourceArrayEmpty_ThrowsArgumentOutOfRangeException()
		{
			// Arrange
			var sourceArray = Array.Empty<byte>();
			var destinationArray = Encoding.UTF8.GetBytes("destination");

			// Act & Assert
			_ = AssertExceptionThrown<ArgumentOutOfRangeException>(() => CryptProvider.Append(sourceArray, destinationArray));
		}

		public void TestAppend_DestinationArrayEmpty_ThrowsArgumentOutOfRangeException()
		{
			// Arrange
			var sourceArray = Encoding.UTF8.GetBytes("source");
			var destinationArray = Array.Empty<byte>();

			// Act & Assert
			_ = AssertExceptionThrown<ArgumentOutOfRangeException>(() => CryptProvider.Append(sourceArray, destinationArray));
		}
		#endregion
	}
}
