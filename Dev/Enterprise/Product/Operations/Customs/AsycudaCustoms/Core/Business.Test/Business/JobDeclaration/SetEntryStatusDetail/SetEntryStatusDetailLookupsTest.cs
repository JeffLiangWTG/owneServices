using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	public class SetEntryStatusDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsEntryHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();
			declaration.ActiveEntryHeaders.AddNew();
			SetEntryStatusDetail setEntryStatusDetail = new SetEntryStatusDetail(declaration);
			var enties = setEntryStatusDetail.Lookups.ActiveEntryHeaders;
			AssertEquals(2, enties.Count);
		}

		public void TestEntryStatusList()
		{
			Factory.SetupEntryStatusList();
			var declaration = Factory.New<JobDeclaration>();
			SetEntryStatusDetail setEntryStatusDetail = new SetEntryStatusDetail(declaration);
			var list = setEntryStatusDetail.Lookups.EntryStatusList;
			AssertEquals(3, list.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "ST1", "ST2", "ST3" }, list.GetAllCodes());
		}
	}
}
