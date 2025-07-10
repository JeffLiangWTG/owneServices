using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class TransportEquipmentInfoProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentInfoProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new TransportEquipmentInfoProvider(null, bill), "Container is null");
			NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new TransportEquipmentInfoProvider(container, null), "Bill is null");
			AssertNoExceptionThrown("Valid arguments", () => new TransportEquipmentInfoProvider(container, bill));
		});
	}

	[ExpectNoExceptions]
	public void TestContainerDetails() => NUnit.Framework.Assert.That(GetProvider().ContainerDetails, Is.TypeOf<TransportEquipmentDetailsProvider>());

	[ExpectNoExceptions]
	public void TestServiceRequirements()
	{
		bill.ABL_SpecialCargoCode = "10";
		CombineAssertions(() =>
		{
			var provider = GetProvider().ServiceRequirements;
			NUnit.Framework.Assert.That(GetProvider().ServiceRequirements, Is.Not.EqualTo(default(Enterprise.Customs.AE.Manifest.Business.ITransportServiceRequirementsProvider)), "Service Requirement set - should not be [null]");
			NUnit.Framework.Assert.That(provider, Is.TypeOf<TransportServiceRequirementsProvider>(), "Service Requirements Type");

			bill.ABL_SpecialCargoCode = ZString.Empty;
			bill.ABL_CargoType = "12";
			NUnit.Framework.Assert.That(GetProvider().ServiceRequirements, Is.Not.EqualTo(default(Enterprise.Customs.AE.Manifest.Business.ITransportServiceRequirementsProvider)), "Cargo Type is set - should not be [null]");

			bill.ABL_CargoType = ZString.Empty;
			NUnit.Framework.Assert.That(GetProvider().ServiceRequirements, Is.Null, "Service Requirements not needed");
		});
	}

	[ExpectNoExceptions]
	public void TestGoodsWeightInKgs()
	{
		CombineAssertions(() =>
		{
			container.ACN_GoodsWeight = 100m;
			container.ACN_GoodsWeightUQ = Weight.Kilograms;
			NUnit.Framework.Assert.That(GetProvider().GoodsWeightInKgs, Is.EqualTo(100m), "Goods Weight");

			container.ACN_GoodsWeightUQ = Weight.Pounds;
			NUnit.Framework.Assert.That(GetProvider().GoodsWeightInKgs, Is.EqualTo(45.359237m), "Weight converted to KG");
		});
	}

	[ExpectNoExceptions]
	public void TestTemperature()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().Temperature, Is.Null, "Temperature not needed");

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ContainerType = ContainerTypes.Refrigerated;
			container.ACN_RC_ContainerType = refContainer.PK;
			NUnit.Framework.Assert.That(GetProvider().Temperature, Is.TypeOf<TemperatureDetailsProvider>(), "Refrigerated Container");
		});
	}

	[ExpectNoExceptions]
	public void TestSealNumber() => CombineAssertions(() =>
	{
		NUnit.Framework.Assert.That(GetProvider().SealNumber, Is.Null.Or.Empty, "Default - should be [null] or [empty]");

		container.ACN_Seal1 = "SEL";
		NUnit.Framework.Assert.That(GetProvider().SealNumber, Is.EqualTo("SEL"), "From ACN_Seal1");
	});

	protected override TransportEquipmentInfoProvider GetProvider() => new(container, bill);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
		container = header.Containers.AddNew();
	}
	AsycudaContainer container;
	AsycudaBill bill;
}
