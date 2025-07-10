using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using GuaranteeTypeList = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsGuaranteeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckPW_BondType_ValidateGuaranteeTypeA_CombinedRecords()
	{
		const string message = "All Guarantee Types must be A, or none must be A.";
		var mainGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		var secondGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		mainGuarantee.PW_BondType = GuaranteeTypeList.Codes.GuaranteeWaiverByAgreement;
		secondGuarantee.PW_BondType = GuaranteeTypeList.Codes.GuaranteeWaiverByAgreement;
		mainGuarantee.Validation.ValidatePW_BondType();
		CombineAssertions(() =>
		{
			AssertNoError("Main Guarantee is of type type A", mainGuarantee.PW_BondTypeInfo, message);

			secondGuarantee.PW_BondType = GuaranteeTypeList.Codes.GuaranteeWaiver;
			mainGuarantee.Validation.ValidatePW_BondType();
			AssertHasError("Main Guarantee is of type A and second Guarantee is not", mainGuarantee.PW_BondTypeInfo, message);
		});
	}

	public void TestCheckPW_BondType_ValidateGuaranteeTypes01249_CombinedRecords()
	{
		var mainGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		var secondGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		foreach (var type in ValidationExtendMethods.GuaranteeTypesApplicable)
		{
			var message = $"Guarantee Type {type} can only be combined with type 3.";

			mainGuarantee.PW_BondType = type;
			secondGuarantee.PW_BondType = type;
			mainGuarantee.Validation.ValidatePW_BondType();

			CombineAssertions(() =>
			{
				AssertNoError($"Main Guarantee is of type {type} and second Guarantee of type {type}", mainGuarantee.PW_BondTypeInfo, message);

				secondGuarantee.PW_BondType = GuaranteeTypeList.Codes.CashDepositGuarantee;
				mainGuarantee.Validation.ValidatePW_BondType();
				AssertNoError($"Main Guarantee is of type {type} and second Guarantee of type 3", mainGuarantee.PW_BondTypeInfo, message);

				var thirdGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
				thirdGuarantee.PW_BondType = type;
				mainGuarantee.Validation.ValidatePW_BondType();
				AssertNoError($"Two Guarantees of type {type} and one Guarantee of type 3", mainGuarantee.PW_BondTypeInfo, message);
				nctsHeader.MovementHeader.Guarantees.RemoveAndDelete(thirdGuarantee);

				secondGuarantee.PW_BondType = GuaranteeTypeList.Codes.GuaranteeWaived;
				mainGuarantee.Validation.ValidatePW_BondType();
				AssertHasError($"Main Guarantee is of type {type} and second Guarantee of type 0", mainGuarantee.PW_BondTypeInfo, message);
			});
		}
	}

	public void TestCheckPW_BondType_ValidateGuaranteeTypes35B_CombinedRecords()
	{
		var mainGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		var secondGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		foreach (var type in new ZString[] { GuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention, GuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur })
		{
			string message = $"Guarantee Type {type} can only be combined with types 3, 5 or B.";

			mainGuarantee.PW_BondType = type;

			CombineAssertions(() =>
			{
				secondGuarantee.PW_BondType = GuaranteeTypeList.Codes.CashDepositGuarantee;
				mainGuarantee.Validation.ValidatePW_BondType();
				AssertNoError($"Main Guarantee is of type {type} and second Guarantee of type 3", mainGuarantee.PW_BondTypeInfo, message);

				secondGuarantee.PW_BondType = GuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention;
				mainGuarantee.Validation.ValidatePW_BondType();
				AssertNoError($"Main Guarantee is of type {type} and second Guarantee of type B", mainGuarantee.PW_BondTypeInfo, message);

				secondGuarantee.PW_BondType = GuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur;
				mainGuarantee.Validation.ValidatePW_BondType();
				AssertNoError($"Main Guarantee is of type {type} and second Guarantee of type 5", mainGuarantee.PW_BondTypeInfo, message);

				secondGuarantee.PW_BondType = GuaranteeTypeList.Codes.GuaranteeWaiver;
				mainGuarantee.Validation.ValidatePW_BondType();
				AssertHasError($"Main Guarantee is of type {type} and second Guarantee of type 0", mainGuarantee.PW_BondTypeInfo, message);
			});
		}
	}

	public void TestValidateGuaranteesRuleB0054()
	{
		const string message = "You cannot combine a national guarantee with an international guarantee for type 0, 1, 2, 4 or 9.";
		var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee1.PW_BondNumber = "22BE1234567890123";
		var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();

		CombineAssertions(() =>
		{
			foreach (var type in new[] { "0", "1", "2", "4", "9" })
			{
				guarantee1.PW_BondType = type;
				guarantee2.PW_BondType = "3";
				guarantee2.PW_BondNumber = "22NL1234567890123";
				AssertNoError("Nationality check is only done for PW_BondType 0, 1, 2, 4 and 9", guarantee2.PW_BondNumberInfo, message);

				guarantee2.PW_BondType = type;
				guarantee2.Validation.ValidatePW_BondNumber();
				AssertHasError("International and national guarantees cannot be mixed.", guarantee2.PW_BondNumberInfo, message);

				guarantee2.PW_BondNumber = ZString.Empty;
				AssertNoError("No error when no guarantee number is filled in", guarantee2.PW_BondNumberInfo, message);

				guarantee2.PW_BondNumber = "22BE43210987654321";
				AssertNoError("Only national guarantees are used.", guarantee2.PW_BondNumberInfo, message);
			}
		});
	}

	public void TestValidateGuaranteesRuleB0039()
	{
		nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
		var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		var message = "Type of guarantee must be 0 or 1. Please correct type.";

		CombineAssertions(() =>
		{
			foreach (var type in new[] { GuaranteeTypeList.Codes.GuaranteeWaiver, GuaranteeTypeList.Codes.ComprehensiveGuarantee, GuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor, GuaranteeTypeList.Codes.CashDepositGuarantee,
										GuaranteeTypeList.Codes.FlatRateVoucher, GuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur, GuaranteeTypeList.Codes.GuaranteeWaived,
										GuaranteeTypeList.Codes.GuaranteeNotRequiredForTheJourneyBetweenOodepAndOotra, GuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies, GuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage,
										GuaranteeTypeList.Codes.GuaranteeWaiverByAgreement, GuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention })
			{
				guarantee.PW_BondType = type;
				if (type == GuaranteeTypeList.Codes.GuaranteeWaiver || type == GuaranteeTypeList.Codes.ComprehensiveGuarantee)
				{
					AssertNoMessageError($"Type {type} is allowed in simplified procedure", guarantee.PW_BondTypeInfo, message);
				}
				else
				{
					AssertHasMessageError($"Type {type} is not allowed in simplified procedure", guarantee.PW_BondTypeInfo, message);
				}
			}
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
	}

	NctsHeader nctsHeader;
}
