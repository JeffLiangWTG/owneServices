using System;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class ConsignmentInfoProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentInfoProvider>
{
	public void TestConstructor()
	{
		NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new ConsignmentInfoProvider(null), "Bill is null");
		AssertNoExceptionThrown("Bill is valid", () => new ConsignmentInfoProvider(bill));
	}

	[ExpectNoExceptions]
	public void TestTotalHouseBills()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().TotalHouseBills, Is.EqualTo(1), "Single HouseBill");

			bill.ABL_BolType = "STD";
			bill.Header.Bills.AddNew().ABL_BolType = "STD";
			bill.Header.Bills.AddNew().ABL_BolType = "CLD";
			NUnit.Framework.Assert.That(GetProvider().TotalHouseBills, Is.EqualTo(2), "Multiple HouseBills");
		});
	}

	[ExpectNoExceptions]
	public void TestBillDetails()
	{
		NUnit.Framework.Assert.That(GetProvider().BillDetails, Is.TypeOf<ConsignmentDetailsProvider>());
	}

	protected override ConsignmentInfoProvider GetProvider() => new ConsignmentInfoProvider(bill);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
	}
	AsycudaBill bill;
}
