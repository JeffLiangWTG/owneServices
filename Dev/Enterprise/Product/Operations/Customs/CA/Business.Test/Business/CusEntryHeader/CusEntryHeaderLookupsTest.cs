using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			var parent = Factory.New<CusEntryHeader>();
			parent.CH_MessageType = MessageTypeList.Codes.G7Export;
			AssertEquals("Accepted G7 Export Message Original", parent.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ClearOriginal));

			parent.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			AssertEquals("Accepted Data Loading Module Original", parent.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ClearOriginal));

			parent.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			AssertEquals("Accepted Across/IID EDI Release Original", parent.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ClearOriginal));
		}

		public void TestCH_EntryStatusList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var parent = declaration.CustomsEntryHeaders.AddNew();
			parent.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			AssertEquals("Goods Released", parent.Lookups.CH_EntryStatusList.GetDescriptionFromCode("CLR"));

			parent.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals("Entry Accepted", parent.Lookups.CH_EntryStatusList.GetDescriptionFromCode("CLR"));
		}
	}
}
