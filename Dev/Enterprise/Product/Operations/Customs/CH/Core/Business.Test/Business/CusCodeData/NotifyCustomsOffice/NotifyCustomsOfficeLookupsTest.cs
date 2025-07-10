using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CH;

namespace Enterprise.Customs.CH.Business.Testing;

class NotifyCustomsOfficeLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCY_DataList()
	{
		RefCusCodeTestHelper.CreateNotifyCustomsOfficeList(Factory);

		var list = lookups.NotifyCustomsOfficeList;
		list.Load();

		AssertEquals("Number of elements", 1, list.Count);
		CombineAssertions(() =>
		{
			Assert("Expected valid code", list.Find(c => c.ZZD_Code == RefCusCodeTestHelper.ValidNotifyCustomsOfficeCode).Any());
			Assert("Unexpected invalid code", !list.Find(c => c.ZZD_Code == RefCusCodeTestHelper.InvalidNotifyCustomsOfficeCode).Any());
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		lookups = declaration.Invoices.AddNew().InvoiceLines.AddNew().NotifyCustomsOffices.AddNew().Lookups;
	}
	NotifyCustomsOfficeLookups lookups;
}
