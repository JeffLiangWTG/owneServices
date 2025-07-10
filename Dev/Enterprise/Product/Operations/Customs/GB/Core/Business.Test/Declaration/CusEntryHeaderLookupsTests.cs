using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class CusEntryHeaderLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryHeader = (CusEntryHeader)dec.ActiveEntryHeaders.AddNew();
			AssertType<Customs.Common.EU.MessageStatusList>(entryHeader.Lookups.MessageStatusList);
		}
	}
}
