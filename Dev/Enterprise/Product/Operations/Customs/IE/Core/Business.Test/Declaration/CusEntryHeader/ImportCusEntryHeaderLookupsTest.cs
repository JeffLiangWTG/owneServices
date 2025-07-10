using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportCusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeader()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertSame(new ImportCusEntryHeaderLookups(parent).EntryHeader, parent);
		}

		public void TestMessageStatusList()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertSame(Factory.GetCachedValue<IELogicalStatusList>(), new ImportCusEntryHeaderLookups(parent).MessageStatusList);
		}

		public void TestCH_MessageTypeList()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertSame(Factory.GetCachedValue<ImportDeclarationTypeList>(), new ImportCusEntryHeaderLookups(parent).CH_MessageTypeList);
		}

		public void TestCH_EntryStatusList()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertSame(Factory.GetCachedValue<AISEntryStatusList>(), new ImportCusEntryHeaderLookups(parent).CH_EntryStatusList);
		}
	}
}
