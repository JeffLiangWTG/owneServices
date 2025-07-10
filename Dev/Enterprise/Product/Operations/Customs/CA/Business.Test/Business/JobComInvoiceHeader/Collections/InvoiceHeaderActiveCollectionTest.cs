using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceHeaderActiveCollectionTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestAllowNew()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			var collection = new InvoiceHeaderActiveCollectionForTest(declaration);
			Assert("Allow New", collection.AllowNew);
			declaration.CA_LVSCloseDate = CargoWise.Types.ZDateTime.Now;
			Assert("Not Allow New", !collection.AllowNew);
			declaration.CA_LVSCloseDate = CargoWise.Types.ZDateTime.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			Assert("Allow New", collection.AllowNew);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Assert("Not Allow New", !collection.AllowNew);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Allow New", collection.AllowNew);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			Assert("Not Allow New", !collection.AllowNew);
		}
	}
}
