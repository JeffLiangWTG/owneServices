using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExportCusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeader()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertSame(new ExportCusEntryHeaderLookups(parent).EntryHeader, parent);
		}

		public void TestMessageStatusList()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertSame(Factory.GetCachedValue<IELogicalStatusList>(), new ExportCusEntryHeaderLookups(parent).MessageStatusList);
		}

		public void TestCH_MessageTypeList()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertSame(Factory.GetCachedValue<ExportDeclarationTypeList>(), new ExportCusEntryHeaderLookups(parent).CH_MessageTypeList);
		}

		public void TestCH_EntryStatusList()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertSame(Factory.GetCachedValue<AESEntryStatusList>(), new ExportCusEntryHeaderLookups(parent).CH_EntryStatusList);
		}
	}
}
