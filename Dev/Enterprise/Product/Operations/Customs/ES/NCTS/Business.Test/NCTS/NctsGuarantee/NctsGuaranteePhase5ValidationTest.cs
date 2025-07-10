using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

sealed class NctsGuaranteePhase5ValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckPW_BondNumber_C0085()
	{
		using var deciderTestContext = new EU.NCTS.Business.Testing.NctsGuaranteeValidationDeciderTestContext<EU.NCTS.Business.INctsGuaranteeDeparturePhase5ValidationDecider>(Factory);

		deciderTestContext.EnableRule(c => c.IsRuleC0085Active);
		var guaranteeTypeWithReference = new[]
		{
				ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaiver,
				ESNCTS5GuaranteeTypeList.Codes.ComprehensiveGuarantee,
				ESNCTS5GuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor,
				ESNCTS5GuaranteeTypeList.Codes.CashDepositGuarantee,
				ESNCTS5GuaranteeTypeList.Codes.FlatRateVoucher
			};

		var errorMessage = "[C0085] You have not entered a Guarantee Reference Number (GRN).";
		var allCodes = new ESNCTS5GuaranteeTypeList().GetAllCodes();
		var guaranteeTypeNotWithReference = allCodes.Except(guaranteeTypeWithReference.Cast<string>());

		CombineAssertions(() =>
		{
			guaranteeTypeWithReference.ForEach(code => AssertGuaranteeTypeWithReference(guarantee, code));
			guaranteeTypeNotWithReference.ForEach(code => AssertGuaranteeTypeNotWithReference(guarantee, code));
		});

		void AssertGuaranteeTypeWithReference(NctsGuarantee guarantee, ZString bondType)
		{
			guarantee.PW_BondType = bondType;
			guarantee.PW_BondNumber = ZString.Empty;
			AssertHasMessageError($"PW_BondType is {bondType} and PW_BondNumber is empty", guarantee.PW_BondNumberInfo, errorMessage);

			guarantee.PW_BondNumber = "abc";
			AssertNoMessageError($"PW_BondType is {bondType} and PW_BondNumber is not empty", guarantee.PW_BondNumberInfo, errorMessage);
		}

		void AssertGuaranteeTypeNotWithReference(NctsGuarantee guarantee, ZString bondType)
		{
			guarantee.PW_BondType = bondType;
			guarantee.PW_BondNumber = ZString.Empty;
			AssertNoMessageError($"PW_BondType is {bondType} and PW_BondNumber is empty", guarantee.PW_BondNumberInfo, errorMessage);

			guarantee.PW_BondNumber = "abc";
			AssertNoMessageError($"PW_BondType is {bondType} and PW_BondNumber is not empty", guarantee.PW_BondNumberInfo, errorMessage);
		}
	}

	public void TestCheckPW_BondType_Empty() => CombineAssertions(() =>
	{
		guarantee.PW_BondType = ZString.Empty;
		AssertHasMessageErrorContaining($"When PW_BondType is empty and Departure", guarantee.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		guarantee = (NctsGuarantee)header.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondType = ZString.Empty;

		AssertNoMessageErrorContaining($"When PW_BondType is empty but Arrival", guarantee.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
	});

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		guarantee = header.MovementHeader.Guarantees.AddNew();
	}

	NctsGuarantee guarantee;
}
