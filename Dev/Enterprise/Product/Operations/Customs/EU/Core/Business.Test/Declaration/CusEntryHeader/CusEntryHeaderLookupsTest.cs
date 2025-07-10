using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeader()
		{
			CusEntryHeader parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Lookups.EntryHeader, parent);
		}

		public void TestMessageStatusList()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			AssertEquals(typeof(Common.EU.MessageStatusList), entry.Lookups.MessageStatusList.GetType());
		}

		public void TestUQList()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			CombineAssertions(() =>
			{
				AssertEquals("Necessary to use ZCalcDropEdit, we only use KG as Unit of Measure, Code", "KG", entry.Lookups.UQList.CodesAsString);
				AssertEquals("Necessary to use ZCalcDropEdit, we only use KG as Unit of Measure, Description", "Kilograms", entry.Lookups.UQList.GetDescriptionFromCode("KG"));
			});
		}
	}
}
