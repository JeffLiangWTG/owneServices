using System;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class TransportServiceRequirementsProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportServiceRequirementsProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new TransportServiceRequirementsProvider(null), "Bill is null");
			AssertNoExceptionThrown("Valid bill", () => new TransportServiceRequirementsProvider(bill));
		});
	}

	[ExpectNoExceptions]
	public void TestServiceRequirementCode()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().ServiceRequirementCode, Is.Null.Or.Empty, "Service Requirement not set - should be [null] or [empty]");

			bill.ABL_SpecialCargoCode = "ABC";
			NUnit.Framework.Assert.That(GetProvider().ServiceRequirementCode, Is.EqualTo("ABC"), "Service Requirement set");
		});
	}

	[ExpectNoExceptions]
	public void TestCargoType()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().CargoType, Is.Null.Or.Empty, "Cargo Type not set - should be [null] or [empty]");

			bill.ABL_CargoType = "12";
			NUnit.Framework.Assert.That(GetProvider().CargoType, Is.EqualTo("12"), "Cargo Type set");
		});
	}

	protected override TransportServiceRequirementsProvider GetProvider() => new(bill);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
	}
	AsycudaBill bill;
}
