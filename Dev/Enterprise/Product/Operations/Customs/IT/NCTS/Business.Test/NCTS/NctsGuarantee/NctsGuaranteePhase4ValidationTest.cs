using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsGuaranteePhase4ValidationTest : TestCaseWithFactory
{
	public void TestConditionC125C130()
	{
		var guarantee = nctsHeader.Guarantees.AddNew();
		CombineAssertions(() =>
		{
			{
				AssertConditionC125C130(guarantee, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver, true);
				AssertConditionC125C130(guarantee, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, true);
				AssertConditionC125C130(guarantee, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor, true);
				AssertConditionC125C130(guarantee, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher, true);
				AssertConditionC125C130(guarantee, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage, true);
				AssertConditionC125C130(guarantee, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies, false);
				AssertConditionC125C130(guarantee, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeNotRequiredForTheJourneyBetweenOodepAndOotra, false);
				AssertConditionC125C130(guarantee, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverByAgreement, false);
				AssertConditionC125C130(guarantee, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur, false);
				AssertConditionC125C130(guarantee, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention, false);
			}
		});
	}

	void AssertConditionC125C130(NctsGuarantee guarantee, string guaranteeType, ZBool isApplicableType)
	{
		guarantee.PW_BondType = guaranteeType;
		guarantee.PW_BondAmount = ZDecimal.Zero;
		guarantee.PW_BondNumber = "123456";
		guarantee.PW_BondNumber2 = "123456";
		guarantee.PW_Password = "1234";

		if (isApplicableType)
		{
			AssertConditionC125C130ForApplicableType(guarantee, guaranteeType);
		}
		else
		{
			AssertConditionC125C130ForNonApplicableType(guarantee, guaranteeType);
		}
	}

	void AssertConditionC125C130ForApplicableType(NctsGuarantee guarantee, string guaranteeType)
	{
		AssertHasMessageErrorContaining($"Check condition C125/C130 for Type {guaranteeType}, PW_BondNumber2 should be empty", guarantee.PW_BondNumber2Info, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForApplicableType);
		guarantee.PW_BondNumber2 = "";
		AssertNoMessageErrorContaining($"Check condition C125/C130 for Type {guaranteeType}, PW_BondNumber2 is empty", guarantee.PW_BondNumber2Info, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForApplicableType);
	}

	void AssertConditionC125C130ForNonApplicableType(NctsGuarantee guarantee, string guaranteeType)
	{
		AssertHasMessageErrorContaining($"Check condition C125/C130 for Type {guaranteeType}, PW_BondAmout is required", guarantee.PW_BondAmountInfo, ValidationCaptions.NctsGuarantee.IsRequiredForSelectedType);
		guarantee.PW_BondAmount = 1234.56m;
		AssertNoMessageErrorContaining($"Check condition C125/C130 for Type {guaranteeType}, PW_BondAmout is not empty", guarantee.PW_BondAmountInfo, ValidationCaptions.NctsGuarantee.IsRequiredForSelectedType);

		AssertNoMessageErrorContaining($"Check condition C125/C130 for Type {guaranteeType}, PW_BondNumber2 is not null", guarantee.PW_BondNumber2Info, ValidationCaptions.NctsGuarantee.IsRequiredForSelectedType);
		guarantee.PW_BondNumber2 = "";
		AssertHasMessageErrorContaining($"Check condition C125/C130 for Type {guaranteeType}, PW_BondNumber2 is required", guarantee.PW_BondNumber2Info, ValidationCaptions.NctsGuarantee.IsRequiredForSelectedType);

		AssertHasMessageErrorContaining($"Check condition C125/C130 for Type {guaranteeType}, PW_BondNumber should be empty", guarantee.PW_BondNumberInfo, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForNonApplicableType);
		guarantee.PW_BondNumber = "";
		AssertNoMessageErrorContaining($"Check condition C125/C130 for Type {guaranteeType}, PW_BondNumber is not empty", guarantee.PW_BondNumberInfo, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForNonApplicableType);

		AssertHasMessageErrorContaining($"Check condition C125/C130 for Type {guaranteeType}, PW_Password should be empty", guarantee.PW_PasswordInfo, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForNonApplicableType);
		guarantee.PW_Password = "";
		AssertNoMessageErrorContaining($"Check condition C125/C130 for Type {guaranteeType}, PW_Password is not empty", guarantee.PW_PasswordInfo, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForNonApplicableType);
	}

	public void TestConditionC125C130ForTir()
	{
		var guarantee = nctsHeader.Guarantees.AddNew();
		guarantee.PW_BondType = "1";
		guarantee.PW_BondAmount = 1234.56m;
		guarantee.PW_BondNumber = "123456";
		guarantee.PW_BondNumber2 = "123456";
		guarantee.PW_Password = "1234";

		AssertHasMessageErrorContaining($"Check condition C125/C130 for Type 1, PW_BondNumber2 should be empty", guarantee.PW_BondNumber2Info, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForApplicableType);
		nctsHeader.MovementHeader.BM_InBondEntryType = "TIR";
		guarantee.Validation.ValidatePW_BondNumber2();
		AssertNoMessageErrorContaining($"Check condition C125/C130 for Type 1 and InBondEntryType 'TIR', PW_BondNumber2 is optional", guarantee.PW_BondNumber2Info, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForApplicableType);
		nctsHeader.MovementHeader.BM_InBondEntryType = "";

		guarantee.PW_BondAmount = ZDecimal.Zero;
		guarantee.PW_BondNumber2 = "";
		guarantee.PW_BondType = "A";
		AssertHasMessageErrorContaining($"Check condition C125/C130 for Type A, PW_BondAmout is required", guarantee.PW_BondAmountInfo, ValidationCaptions.NctsGuarantee.IsRequiredForSelectedType);
		nctsHeader.MovementHeader.BM_InBondEntryType = "TIR";
		guarantee.Validation.ValidatePW_BondAmount();
		AssertNoMessageErrorContaining($"Check condition C125/C130 for Type A and InBondEntryType 'TIR', PW_BondAmout is optional", guarantee.PW_BondAmountInfo, ValidationCaptions.NctsGuarantee.IsRequiredForSelectedType);
		nctsHeader.MovementHeader.BM_InBondEntryType = "";

		AssertHasMessageErrorContaining($"Check condition C125/C130 for Type A, PW_BondNumber2 is required", guarantee.PW_BondNumber2Info, ValidationCaptions.NctsGuarantee.IsRequiredForSelectedType);
		nctsHeader.MovementHeader.BM_InBondEntryType = "TIR";
		guarantee.Validation.ValidatePW_BondNumber2();
		AssertNoMessageErrorContaining($"Check condition C125/C130 for Type A and InBondEntryType 'TIR', PW_BondNumber2 is optional", guarantee.PW_BondNumber2Info, ValidationCaptions.NctsGuarantee.IsRequiredForSelectedType);
		nctsHeader.MovementHeader.BM_InBondEntryType = "";

		AssertHasMessageErrorContaining($"Check condition C125/C130 for Type A, PW_BondNumber should be empty", guarantee.PW_BondNumberInfo, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForNonApplicableType);
		nctsHeader.MovementHeader.BM_InBondEntryType = "TIR";
		guarantee.Validation.ValidatePW_BondNumber();
		AssertNoMessageErrorContaining($"Check condition C125/C130 for Type A and InBondEntryType 'TIR', PW_BondNumber is optional", guarantee.PW_BondNumberInfo, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForNonApplicableType);
		nctsHeader.MovementHeader.BM_InBondEntryType = "";

		guarantee.Validation.ValidatePW_Password();
		AssertHasMessageErrorContaining($"Check condition C125/C130 for Type A, PW_Password should be empty", guarantee.PW_PasswordInfo, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForNonApplicableType);
		nctsHeader.MovementHeader.BM_InBondEntryType = "TIR";
		guarantee.Validation.ValidatePW_Password();
		AssertNoMessageErrorContaining($"Check condition C125/C130 for Type A and InBondEntryType 'TIR', PW_Password is optional", guarantee.PW_PasswordInfo, ValidationCaptions.NctsGuarantee.ShouldBeEmptyForNonApplicableType);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
	}

	NctsHeader nctsHeader;
}
