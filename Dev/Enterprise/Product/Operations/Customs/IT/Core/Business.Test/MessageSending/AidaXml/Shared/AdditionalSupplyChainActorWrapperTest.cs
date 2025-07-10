using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class AdditionalSupplyChainActorWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new AdditionalSupplyChainActorWrapper(null));
	}

	public void TestRole()
	{
		var supplyChainActor = GetNewAdditionalSupplyChainActor();
		AssertEquals(nameof(IAdditionalSupplyChainActor.Role), "", supplyChainActor.Role);

		cusSupplyChainActorReference.CFR_Code = "ABC";
		supplyChainActor = GetNewAdditionalSupplyChainActor();
		AssertEquals(nameof(IAdditionalSupplyChainActor.Role), "ABC", supplyChainActor.Role);
	}

	public void TestReference()
	{
		var supplyChainActor = GetNewAdditionalSupplyChainActor();
		AssertEquals(nameof(IAdditionalSupplyChainActor.Reference), "", supplyChainActor.Reference);

		cusSupplyChainActorReference.CFR_Reference = "XYZ";
		supplyChainActor = GetNewAdditionalSupplyChainActor();
		AssertEquals(nameof(IAdditionalSupplyChainActor.Reference), "XYZ", supplyChainActor.Reference);
	}

	protected override void SetUp()
	{
		base.SetUp();

		cusSupplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
	}

	CusSupplyChainActorReference cusSupplyChainActorReference;

	IAdditionalSupplyChainActor GetNewAdditionalSupplyChainActor() => new AdditionalSupplyChainActorWrapper(cusSupplyChainActorReference);
}
