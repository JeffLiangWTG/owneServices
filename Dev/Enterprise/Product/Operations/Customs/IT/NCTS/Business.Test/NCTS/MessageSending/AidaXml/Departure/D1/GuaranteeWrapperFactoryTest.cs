using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class GuaranteeWrapperFactoryTest : TestCaseWithFactory
{
	public void TestGetNewGuaranteeWrapper_GuardClause()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When guarantee is null",
			() => guaranteeWrapperFactory.GetNewGuaranteeWrapper(guarantee: null));
	}

	public void TestGetNewGuaranteeWrapper_ReturnGuaranteeWrapperWithDetails()
	{
		CombineAssertions(() =>
		{
			SetBondTypeAndAssertIsGuaranteeWrapperWithDetails(bondType: "0");
			SetBondTypeAndAssertIsGuaranteeWrapperWithDetails(bondType: "1");
			SetBondTypeAndAssertIsGuaranteeWrapperWithDetails(bondType: "2");
			SetBondTypeAndAssertIsGuaranteeWrapperWithDetails(bondType: "3");
			SetBondTypeAndAssertIsGuaranteeWrapperWithDetails(bondType: "4");
			SetBondTypeAndAssertIsGuaranteeWrapperWithDetails(bondType: "5");
			SetBondTypeAndAssertIsGuaranteeWrapperWithDetails(bondType: "9");
		});
	}

	public void TestGetNewGuaranteeWrapper_ReturnGuaranteeWrapper()
	{
		nctsGuarantee.PW_BondType = "6";

		AssertType<GuaranteeWrapper>(
			"Guarantee Wrapper Type",
			guaranteeWrapperFactory.GetNewGuaranteeWrapper(nctsGuarantee));
	}

	void SetBondTypeAndAssertIsGuaranteeWrapperWithDetails(string bondType)
	{
		nctsGuarantee.PW_BondType = bondType;

		AssertType<GuaranteeWrapperWithDetails>(
			$"When PW_BondType='{bondType}', Guarantee Wrapper Type",
			guaranteeWrapperFactory.GetNewGuaranteeWrapper(nctsGuarantee));
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();

		guaranteeWrapperFactory = new GuaranteeWrapperFactory();
	}

	NctsGuarantee nctsGuarantee;
	GuaranteeWrapperFactory guaranteeWrapperFactory;
}
