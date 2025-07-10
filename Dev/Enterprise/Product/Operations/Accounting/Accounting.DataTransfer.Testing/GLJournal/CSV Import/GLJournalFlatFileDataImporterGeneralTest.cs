using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	public class GLJournalFlatFileDataImporterGeneralTest : TestCaseWithFactory
	{
		public void TestCreateConverter()
		{
			AssertEquals(typeof(GLJournalFlatFileConverter), fImporter.CreateConverter_ForTestOnly(null).GetType());
		}

		public void TestCreateXsd()
		{
			AssertEquals(typeof(Xsd.GLJournal), fImporter.CreateXsd_ForTestOnly().GetType());
		}

		public void TestFlatFileFormat()
		{
			AssertEquals(typeof(CsvFlatFileFormat), fImporter.FlatFileFormat_ForTestOnly.GetType());
		}

		#region Implementation

		GLJournalFlatFileDataImporter fImporter;

		protected override void SetUp()
		{
			fImporter = new GLJournalFlatFileDataImporter();
			base.SetUp();
		}

		#endregion
	}
}
