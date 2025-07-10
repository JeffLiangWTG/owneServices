using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRDKJDataProvidersTest : XMLMessageTestHelper<GOVCBRDKJDataProvidersTest>
	{
		public void TestDKJHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();

			var supplier2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READY1", "레디1");
			var supplier2Codes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리2403221" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier2, supplier2Codes);
			Factory.Save();
			entry.RandomHeader.JZ_OA_SupplierAddress = supplier2.MainAddress.PK;

			var currentDKJHeader = new ExportCancellationHeaderCreator().Create(entry);

			AssertEquals("6N00221000025X", currentDKJHeader.ExportDeclarationNumber);
			AssertEquals("130", currentDKJHeader.DeclarationCustomsOffice);
			AssertEquals("10", currentDKJHeader.DeclarationCustomsDivision);
			AssertEquals("6N002", currentDKJHeader.UnipassDeclarantID);

			var exporter = currentDKJHeader.Exporter;
			AssertEquals("레디코리아2", exporter.CompanyName);
			AssertEquals("레디코리1971018", exporter.UnipassIDForOrganization);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing";
	}
}
