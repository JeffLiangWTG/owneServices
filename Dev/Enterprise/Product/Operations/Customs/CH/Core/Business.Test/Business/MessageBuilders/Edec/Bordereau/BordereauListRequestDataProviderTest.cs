using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class BordereauListRequestDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new BordereauListRequestDataProvider(null));
	}

	public void TestProvider() => CombineAssertions(() =>
	{
		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "CRN123";
		GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.CAD, "CAD123");

		var sendingObject = new BordereauListRequestSendingObject() { StartDate = new ZDate(2024, 12, 1), EndDate = new ZDate(2024, 12, 31) };
		var dataProvider = new BordereauListRequestDataProvider(sendingObject);

		AssertEquals("RequestorTraderIdentificationNumber", "CRN123", dataProvider.RequestorTraderIdentificationNumber);
		AssertEquals("AccountNumber", "CAD123", dataProvider.AccountNumber);
		AssertEquals("StartDate", new DateTime(2024, 12, 1), dataProvider.StartDate);
		AssertEquals("EndDate", new DateTime(2024, 12, 31), dataProvider.EndDate);
	});
}
