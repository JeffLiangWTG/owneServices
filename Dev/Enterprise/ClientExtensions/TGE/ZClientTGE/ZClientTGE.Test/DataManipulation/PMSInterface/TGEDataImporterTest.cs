using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TGE.PMS.Testing
{
	public class TGEDataImporterTest : FlatFileDataImporterTestCase
	{
		public void TestCusEntryIsSystemCreatedFieldIsSetToFalse()
		{
			TGEDataImporter dataImporter = new TGEDataImporter();
			NotificationBuffer notify = new NotificationBuffer();
			dataImporter.ImportData(PathToTestFile, notify, SourceInfo.EmptySourceInfo);
			CommonConsol[] consols = Factory.Load<CommonConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "8112349399"));
			AssertEquals("Consol is created", true, consols.Length > 0);
			CommonConsol consol = consols[0];
			AssertEquals("There should be shipments attached to consol", 1, consol.Shipments.Count);
			AssertEquals("Shipment has cusEntryNumber", true, consol.Shipments[0].CusEntryNumbers.Count > 0);
			AssertEquals("CusEntrynum 's CE_EntryIsSystemCreated", false, consol.Shipments[0].CusEntryNumbers[0].CE_EntryIsSystemGenerated);
		}

		protected override FlatFileDataImporter GetDataImporter()
		{
			return new TGEDataImporter();
		}

		protected override string PathToTestFile
		{
			get
			{
				return resourceRetriever.SaveResourceToFile("expconWithValidTestData.tsv");
			}
		}

		EmbeddedResourceRetriever resourceRetriever;
		protected override void SetUp()
		{
			base.SetUp();
			TGEDataRegistry.Instance.CodeMapPMSOrganisation = GlbCompany.CurrentCompany.OrgProxy.PK;
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}
	}
}
