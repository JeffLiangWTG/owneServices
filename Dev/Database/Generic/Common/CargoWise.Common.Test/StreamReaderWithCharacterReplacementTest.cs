using System.IO;
using System.Text;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class StreamReaderWithCharacterReplacementTest : TestCase
	{
		public void TestCharacterReplacementsInStream_ReadToEnd()
		{
			using (var reader = GetReader("“‘Bow ties’ are cool”"))
			{
				var result = reader.ReadToEnd();
				AssertEquals("\"'Bow ties' are cool\"", result);
			}
		}

		public void TestCharacterReplacementsInStream_ReadLine()
		{
			using (var reader = GetReader("“‘Bow ties’ are cool”\n“““Come along Pond”””"))
			{
				var line = reader.ReadLine();
				AssertEquals("\"'Bow ties' are cool\"", line);
				line = reader.ReadLine();
				AssertEquals("\"\"\"Come along Pond\"\"\"", line);
			}
		}

		public void TestCharacterReplacementsInStream_Read()
		{
			using (var reader = GetReader("“‘Bow ties’ are cool”"))
			{
				var expected = "\"'Bow ties' are cool\"";
				for (int i = 0; i < expected.Length; i++)
				{
					var character = reader.Read();
					AssertEquals(expected[i], (char)character);
				}
			}
		}

		public void TestCharacterReplacementsInStream_ReadBlock()
		{
			using (var reader = GetReader("“‘Bow ties’ are cool”"))
			{
				var chars = new char[11];
				reader.Read(chars, 0, 11);
				var result = string.Join("", chars);
				AssertEquals("\"'Bow ties'", result);
			}
		}

		public void TestCharacterReplacementsInStream_AfterBaseStreamAccess()
		{
			using (var reader = GetReader("“‘Bow ties’ are cool”"))
			{
				AssertNotNull("Access BaseStream property", reader.BaseStream);
				AssertEquals("\"'Bow ties' are cool\"", reader.ReadToEnd());
			}
		}

		public void TestReadStreamWithNullLine_ShouldHandleNullLines()
		{
			var bytes = new byte[] { 80, 79, 72, 44, 49, 46, 48, 44, 44, 50, 52, 53, 51, 57, 52, 44, 44, 44, 65, 73, 82, 44, 76, 83, 69, 44, 50, 48, 49, 50, 48, 57, 49, 48, 44, 50, 48, 49, 50, 48, 57, 49, 48, 44, 50, 48, 49, 50, 48, 57, 49, 48, 44, 44, 85, 83, 68, 44, 48, 44, 48, 44, 44, 67, 73, 70, 44, 44, 52, 53, 48, 48, 49, 48, 57, 55, 56, 50, 32, 84, 69, 83, 84, 44, 48, 44, 67, 65, 83, 44, 44, 44, 44, 44, 44, 44, 44, 50, 48, 49, 50, 48, 57, 48, 52, 44, 44, 44, 44, 44, 70, 49, 44, 75, 75, 65, 66, 69, 82, 76, 73, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 53, 50, 51, 55, 50, 48, 48, 48, 48, 49, 44, 44, 77, 99, 67, 111, 114, 109, 105, 99, 107, 32, 40, 71, 117, 97, 110, 103, 122, 104, 111, 117, 41, 32, 70, 68, 44, 68, 111, 110, 103, 32, 74, 105, 32, 73, 110, 100, 32, 68, 105, 115, 116, 114, 105, 99, 116, 44, 69, 99, 111, 110, 111, 109, 105, 99, 32, 38, 32, 84, 101, 99, 104, 32, 68, 101, 118, 32, 90, 111, 110, 101, 44, 71, 117, 97, 110, 103, 122, 104, 111, 117, 32, 67, 104, 105, 110, 97, 32, 53, 49, 48, 55, 51, 48, 44, 46, 44, 46, 44, 44, 44, 44, 48, 49, 49, 32, 56, 54, 32, 50, 48, 32, 56, 50, 32, 50, 50, 48, 32, 56, 51, 50, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 68, 101, 108, 105, 118, 101, 114, 121, 32, 68, 97, 116, 101, 32, 48, 57, 47, 49, 48, 47, 50, 48, 49, 50, 44, 10, 80, 79, 76, 44, 49, 44, 48, 44, 53, 50, 53, 50, 49, 55, 48, 50, 48, 48, 51, 49, 44, 77, 67, 68, 79, 78, 32, 81, 83, 87, 32, 50, 53, 70, 32, 83, 65, 85, 67, 32, 80, 76, 65, 73, 78, 32, 32, 49, 46, 53, 44, 44, 44, 44, 44, 44, 44, 53, 48, 48, 44, 67, 65, 83, 44, 44, 44, 48, 44, 44, 44, 44, 48, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 78, 44, 44, 78, 44, 78, 44, 78, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 48, 44, 48, 44, 48, 44, 51, 56, 50, 44, 67, 70, 44, 49, 53, 50, 53, 48, 44, 76, 66, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 10 };
			using (var stream = new MemoryStream(bytes))
			using (var reader = new StreamReaderWithCharacterReplacement(stream))
			{
				AssertEquals("POH,1.0,,245394,,,AIR,LSE,20120910,20120910,20120910,,USD,0,0,,CIF,,4500109782 TEST,0,CAS,,,,,,,,20120904,,,,,F1,KKABERLI,,,,,,,,,,,,,,5237200001,,McCormick (Guangzhou) FD,Dong Ji Ind District,Economic & Tech Dev Zone,Guangzhou China 510730,.,.,,,,011 86 20 82 220 832,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,Delivery Date 09/10/2012,", reader.ReadLine());
				AssertEquals("POL,1,0,525217020031,MCDON QSW 25F SAUC PLAIN  1.5,,,,,,,500,CAS,,,0,,,,0,,,,,,,,,,,,,N,,N,N,N,,,,,,,,,,,,,,,,,,,,0,0,0,382,CF,15250,LB,,,,,,,,,,", reader.ReadLine());
				AssertNull("Should return null at EOF.", reader.ReadLine());
			}
		}

		StreamReaderWithCharacterReplacement GetReader(string source)
		{
			var bytes = Encoding.UTF8.GetBytes(source);
			var reader = new StreamReaderWithCharacterReplacement(new MemoryStream(bytes));
			reader.AddReplacement('“', '"');
			reader.AddReplacement('”', '"');
			reader.AddReplacement('‘', '\'');
			reader.AddReplacement('’', '\'');
			return reader;
		}
	}
}