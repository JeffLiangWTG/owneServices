using System.IO;
using System.Text;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	internal class ZCompressorTest : TestCase
	{
		public void TestGetUncompressedVersionWithDataShouldNotUncompress()
		{
			//Mock a text file with content start with PZ
			AssertGetUncompressedVersion(Encoding.ASCII.GetBytes("PZabcdefg"));
			AssertGetUncompressedVersion(Encoding.ASCII.GetBytes("PZabc"));
		}

		void AssertGetUncompressedVersion(byte[] data)
		{
			// The binary is not suitable for compression as it is less than 50 bytes
			AssertEquals("IsDataSuitableForCompression", false, ZCompressor.IsDataSuitableForCompression(data));

			// But the Compressor treats it as a compressed binary because it starts with 0x505A (PZ)
			AssertEquals("IsCompressed", true, Compressor.IsCompressed(data));
			AssertEquals("IsUserCompressed", false, ZCompressor.IsUserCompressed("SC_ImageData"));

			var uncompressedData = ZCompressor.GetUncompressedVersion(data, "SC_ImageData");
			AssertEquals("Should return original data", data, uncompressedData);
		}

		public void TestIsDataSuitableForCompression()
		{
			byte[] testData1 = System.Array.Empty<byte>();
			byte[] testData2 = new byte[1000];
			testData2[0] = 255;
			testData2[1] = 216;
			testData2[2] = 255;
			testData2[3] = 224;

			byte[] testData3 = new byte[1000];
			testData3[0] = 1;
			testData3[1] = 2;
			testData3[2] = 3;
			testData3[3] = 4;

			var bigText = GenerateBigText();
			byte[] testData4 = Encoding.ASCII.GetBytes(bigText);

			AssertEquals("Test with null data", false, ZCompressor.IsDataSuitableForCompression(testData1));
			AssertEquals("Test with signature data", false, ZCompressor.IsDataSuitableForCompression(testData2));
			AssertEquals("Test with random data", true, ZCompressor.IsDataSuitableForCompression(testData3));
			AssertEquals("Test with big data 'A's", true, ZCompressor.IsDataSuitableForCompression(testData4));

			using (Stream stream = new MemoryStream(testData1))
			{
				AssertEquals("Test with null data", false, ZCompressor.IsDataSuitableForCompression(stream));
			}
			using (Stream stream = new MemoryStream(testData2))
			{
				AssertEquals("Test with signature data", false, ZCompressor.IsDataSuitableForCompression(stream));
			}
			using (Stream stream = new MemoryStream(testData3))
			{
				AssertEquals("Test with random data", true, ZCompressor.IsDataSuitableForCompression(stream));
			}
			using (Stream stream = new MemoryStream(testData4))
			{
				AssertEquals("Test with big data 'A's", true, ZCompressor.IsDataSuitableForCompression(stream));
			}
		}

		public void TestCompressOnlyCompressesBlobs()
		{
			object obj = new object();
			const string str = "blah";
			const int integ = 455;

			var bytes = new byte[1000];

			AssertEquals("Shouldn't be changed", obj, ZCompressor.GetCompressedVersion(obj, "TestColumn"));
			AssertEquals("Shouldn't be changed", str, ZCompressor.GetCompressedVersion(str, "TestColumn"));
			AssertEquals("Shouldn't be changed", integ, ZCompressor.GetCompressedVersion(integ, "TestColumn"));
			Assert("Should be changed", bytes.Length != ((byte[])ZCompressor.GetCompressedVersion(bytes, "TestColumn")).Length);
		}

		public void TestUnCompressDoesNotCorruptThings()
		{
			const string str = "blah";
			const int integ = 455;
			const decimal dec = 4;

			var bytes = new byte[1000];

			AssertEquals("Shouldn't be changed", str, (string)ZCompressor.GetUncompressedVersion(str, "TestColumn"));
			AssertEquals("Shouldn't be changed", integ, (int)ZCompressor.GetUncompressedVersion(integ, "TestColumn"));
			AssertEquals("Shouldn't be changed", dec, (decimal)ZCompressor.GetUncompressedVersion(dec, "TestColumn"));
			AssertEquals("Shouldn't be changed", bytes, (byte[])ZCompressor.GetUncompressedVersion(bytes, "TestColumn"));
		}

		public void TestCompressOnlyCompressesBigBlobs()
		{
			var bigBytes = new byte[1000];
			var smallBytes = new byte[1];

			Assert("Should be compressed", bigBytes.Length > ((byte[])ZCompressor.GetCompressedVersion(bigBytes, "TestColumn")).Length);
			AssertEquals("Should not be changed", smallBytes, (byte[])ZCompressor.GetCompressedVersion(smallBytes, "TestColumn"));
		}

		public void TestGetSameValueBackAfterUncompress()
		{
			var bigBytes = new byte[1000];
			AssertEquals(bigBytes, (byte[])ZCompressor.GetUncompressedVersion(ZCompressor.GetCompressedVersion(bigBytes, "TestColumn"), "TestColumn"));
		}
		public void TestGetCompressedVersionWillNotCompressColumnsThatAreMarkedAsUserCompressed()
		{
			var bigText = GenerateBigText();

			byte[] bigBytes = Encoding.ASCII.GetBytes(bigText);
			byte[] resultBytes = (byte[])ZCompressor.GetCompressedVersion(bigBytes, "Column_COMPRESSED");
			AssertEquals("Result", bigText, Encoding.ASCII.GetString(resultBytes));
		}

		public void TestGetCompressedVersionStreamWillNotCompressColumnsThatAreMarkedAsUserCompressed()
		{
			string bigText = GenerateBigText();

			byte[] bigBytes = Encoding.ASCII.GetBytes(bigText);

			Stream srcStream = new MemoryStream(bigBytes);
			Stream destStream = new MemoryStream();

			destStream = ZCompressor.GetCompressedVersion(srcStream, destStream, "Column_COMPRESSED");

			Assert(destStream is MemoryStream);
		}

		public void TestGetUncompressedVersionWillNotUncompressColumnsThatAreMarkedAsUserCompressed()
		{
			byte[] bigBytes = Encoding.ASCII.GetBytes(GenerateBigText());

			byte[] compressedBytes = (byte[])ZCompressor.GetCompressedVersion(bigBytes, "ColumnToCompress");

			byte[] resultBytes = (byte[])ZCompressor.GetUncompressedVersion(compressedBytes, "Column_COMPRESSED");
			AssertEquals("Result", compressedBytes, resultBytes);
		}

		public void TestGetUncompressedVersionStreamWillNotUncompressColumnsThatAreMarkedAsUserCompressed()
		{
			byte[] bigBytes = Encoding.ASCII.GetBytes(GenerateBigText());

			byte[] compressedBytes = (byte[])ZCompressor.GetCompressedVersion(bigBytes, "ColumnToCompress");

			Stream srcStream = ZCompressor.GetUncompressedVersion(new MemoryStream(compressedBytes), "Column_COMPRESSED");

			AssertEquals("Result", compressedBytes, StreamToByteArray(srcStream));
		}

		public void TestGetUncompressedVersionExcludesPredefinedColumns()
		{
			// This is a real password hash binary that could crash in Compressor.Uncompress(): 0x505A0E5073E1A0572556C19E85E26CD8B6E22C65
			var rawBinary = new byte[] { 0x50, 0x5A, 0x0E, 0x50, 0x73, 0xE1, 0xA0, 0x57, 0x25, 0x56, 0xC1, 0x9E, 0x85, 0xE2, 0x6C, 0xD8, 0xB6, 0xE2, 0x2C, 0x65 };

			// The binary is not suitable for compression as it is less than 50 bytes
			AssertEquals("ZCompressor.IsDataSuitableForCompression", false, ZCompressor.IsDataSuitableForCompression(rawBinary));

			// But the Compressor treats it as a compressed binary because it starts with 0x505A (PZ)
			AssertEquals("Compressor.IsCompressed", true, Compressor.IsCompressed(rawBinary));

			// The Uncompress will crash when it tries to uncompress it, unless it is stored in these columns that exclude from uncompression
			AssertEquals("OC_PasswordHash", rawBinary, ZCompressor.GetUncompressedVersion(rawBinary, "OC_PasswordHash"));
			AssertEquals("OC_PasswordSalt", rawBinary, ZCompressor.GetUncompressedVersion(rawBinary, "OC_PasswordSalt"));
			AssertEquals("GS_PasswordHash", rawBinary, ZCompressor.GetUncompressedVersion(rawBinary, "GS_PasswordHash"));
			AssertEquals("GS_PasswordSalt", rawBinary, ZCompressor.GetUncompressedVersion(rawBinary, "GS_PasswordSalt"));
			AssertEquals("PWH_Hash", rawBinary, ZCompressor.GetUncompressedVersion(rawBinary, "PWH_Hash"));
			AssertEquals("PWH_Salt", rawBinary, ZCompressor.GetUncompressedVersion(rawBinary, "PWH_Salt"));
			AssertEquals("PWH_TruncatedHash", rawBinary, ZCompressor.GetUncompressedVersion(rawBinary, "PWH_TruncatedHash"));
			AssertEquals("PER_PasswordHash", rawBinary, ZCompressor.GetUncompressedVersion(rawBinary, "PER_PasswordHash"));
			AssertEquals("PER_PasswordSalt", rawBinary, ZCompressor.GetUncompressedVersion(rawBinary, "PER_PasswordSalt"));

			// or _COMPRESSED column
			AssertEquals("Something_COMPRESSED", rawBinary, ZCompressor.GetUncompressedVersion(rawBinary, "Something_COMPRESSED"));
		}

		static byte[] StreamToByteArray(Stream resultStream)
		{
			byte[] resultBytes = new byte[resultStream.Length];
			for (int i = 0; i < resultStream.Length; i++)
			{
				resultBytes[i] = (byte)resultStream.ReadByte();
			}
			return resultBytes;
		}

		static string GenerateBigText()
		{
			StringBuilder textBuilder = new StringBuilder(1000000);
			for (int index = 0; index < 1000000; index++)
			{
				textBuilder.Append('A');
			}
			return textBuilder.ToString();
		}
	}
}
