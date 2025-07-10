using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.Wow.Testing
{
	public class CsvImportFileFromMITest : TestCaseWithFactory
	{
		public void TestUpdateTimeStampForEdiTrack()
		{
			Db.Connection.BeginTransaction();
			try
			{
				string expectedHeaderLine = "\"0\",\"MANAGING IMPORTS\",\"2003-10-28\",\"0001\"";
				using (Stream stream = new MemoryStream())
				{
					StreamWriter writer = new StreamWriter(stream);
					writer.WriteLine(HeaderLine);
					writer.Flush();
					CsvImportFileFromMIForTest importer = new CsvImportFileFromMIForTest(new StreamReader(stream));
					stream.Position = 0;
					StreamReader reader = importer.GetUpdatedMessageReader("FileID");
					AssertEquals("Header Line should have been updated with the fileID", expectedHeaderLine, reader.ReadLine());
					reader.Close();
				}
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		class CsvImportFileFromMIForTest : CsvImportFileFromMI
		{
			public CsvImportFileFromMIForTest(StreamReader reader) : base(new BusinessObjectFactoryProvider(), reader)
			{
			}
		}

		const string HeaderLine = "\"0\",\"MANAGING IMPORTS\",\"2003-10-28\",";
	}
}
