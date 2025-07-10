using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Client.UPE.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class Level1FileReaderTest : TestCase
	{
		public void TestReadLevel1File()
		{
			string testResourcePath = UPETestHelper.TestResource.DataImport.Path;
			string fileName = UPETestHelper.TestResource.ExtractToFile(testResourcePath, TempDir.DirectoryName, "AU9639Level1Sample.txt");
			Level1FileReader reader = new Level1FileReader(fileName);
			Level1RecordList level1RecordList = reader.Level1RecordList;
			AssertEquals(79, level1RecordList.RecordLines.Count);

			for (int i = 0; i < level1RecordList.RecordLines.Count; i++)
			{
				AssertEquals(Level1FileReader.Constants.FileLineLength, level1RecordList.RecordLines[i].Length);
			}

			ZString lineWithBinaryChars = level1RecordList.RecordLines[62];
			AssertMultilineASCIIEquals("", "US2795AU9639040422              D4A14T9J3YYD5100001   EA GET READY TO CRUISE THE URBAN JUNGLE IN OUR VINTAGE TRUCKER HAT.   <UL><LI>FOAM FRONT</LI>  <LI>PLAST   1099      USD7906969             US                                   AU7783497                                                                                                                                          ", lineWithBinaryChars);

			ZString lastLine = level1RecordList.RecordLines[level1RecordList.RecordLines.Count - 1];
			AssertMultilineASCIIEquals("", "CA1417AU9639040422              D40E372BZZBY5000001   EA CLEET TOOL                                                                                              1000      CADCLEET TOOL          CA                                     2134                                                                                                                                             ", lastLine);
		}

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		protected override void TearDown()
		{
			base.TearDown();
			tempDir?.Dispose();
		}
	}
}
