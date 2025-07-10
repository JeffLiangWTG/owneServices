using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BordereauRequestDataProvider))]
sealed class BordereauRequestDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new BordereauRequestDataProvider(null));
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		GlbCompany.GetCurrentCompany(Factory).GC_CustomsRegistrationNo = "R100";
		var sendingObject = new BordereauRequestSendingObject
		{
			BordereauNumber = "B100",
			CreationDate = new ZDate(2020, 1, 2),
			ProcessingCenterNumber = "C100"
		};
		Factory.Save();

		var dataProvider = new BordereauRequestDataProvider(sendingObject);
		AssertEquals("RequestorTraderIdentificationNumber", "R100", dataProvider.RequestorTraderIdentificationNumber);
		AssertEquals("BordereauNumber", "B100", dataProvider.BordereauNumber);
		AssertEquals("ProcessingCenterNumber", "C100", dataProvider.ProcessingCenterNumber);
		AssertEquals("CreationDate", new DateTime(2020, 1, 2), dataProvider.CreationDate);
	});
}
