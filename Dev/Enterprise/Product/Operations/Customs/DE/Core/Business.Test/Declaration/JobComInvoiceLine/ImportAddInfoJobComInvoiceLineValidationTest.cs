using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.Business.Testing.ValidationTestHelper;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_EconomicConditions_Mandatory()
		{
			CombineAssertions(() =>
			{
				invoiceLine.AddInfoValidation.ValidateZG_EconomicConditions();
				AssertNoMessageErrorContaining("No Entry Instruction", invoiceLine.ZG_EconomicConditionsInfo, MandatoryValidation.YouHaveNotEntered);
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.AddInfoValidation.ValidateZG_EconomicConditions();
				AssertNoMessageErrorContaining("Not Simplified Grant", invoiceLine.ZG_EconomicConditionsInfo, MandatoryValidation.YouHaveNotEntered);
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
				invoiceLine.AddInfoValidation.ValidateZG_EconomicConditions();
				AssertHasMessageErrorContaining("Is Simplified Grant", invoiceLine.ZG_EconomicConditionsInfo, MandatoryValidation.YouHaveNotEntered);
				invoiceLine.ZG_EconomicConditions = "XX";
				AssertNoMessageErrorContaining("Entered", invoiceLine.ZG_EconomicConditionsInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckZG_EconomicConditions_ValidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_A2055, "Economic Conditions");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_A2055, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
			invoiceLine.JI_CEI = instruction.PK;

			AssertInvalidCodeMessageError(invoiceLine.ZG_EconomicConditionsInfo, "@", "01");
		}

		public void TestCheckZG_IdentificationMeansType_Mandatory()
		{
			CombineAssertions(() =>
			{
				invoiceLine.AddInfoValidation.ValidateZG_IdentificationMeansType();
				AssertNoMessageErrorContaining("No Entry Instruction", invoiceLine.ZG_IdentificationMeansTypeInfo, MandatoryValidation.YouHaveNotEntered);
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.AddInfoValidation.ValidateZG_IdentificationMeansType();
				AssertNoMessageErrorContaining("Not Simplified Grant", invoiceLine.ZG_IdentificationMeansTypeInfo, MandatoryValidation.YouHaveNotEntered);
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
				invoiceLine.AddInfoValidation.ValidateZG_IdentificationMeansType();
				AssertHasMessageErrorContaining("Is Simplified Grant", invoiceLine.ZG_IdentificationMeansTypeInfo, MandatoryValidation.YouHaveNotEntered);
				invoiceLine.ZG_IdentificationMeansType = "X";
				AssertNoMessageErrorContaining("Entered", invoiceLine.ZG_IdentificationMeansTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckZG_IdentificationMeansType_ValidCode()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
			invoiceLine.JI_CEI = instruction.PK;

			AssertInvalidCodeMessageError(invoiceLine.ZG_IdentificationMeansTypeInfo, "@", IdentificationMeansTypeList.Codes.D);
		}

		public void TestCheckZG_CessionFlag()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "40", "00", "0C9", "", "IMP");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.TAXFLAG, "01");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.TAXFLAG, "02");
			Factory.Save();

			invoiceLine.JI_FormattedProcedure = "40000C9";
			declaration.JE_MessageType = "EXP";
			invoiceLine.JI_CessionFlag = "01";
			AssertNoMessageError(invoiceLine.JI_CessionFlagInfo, "The code you have selected is not in the list.");

			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_CessionFlag = "04";
			AssertHasMessageError(invoiceLine.JI_CessionFlagInfo, "The code you have selected is not in the list.");

			invoiceLine.JI_CessionFlag = "01";
			AssertNoMessageError(invoiceLine.JI_CessionFlagInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckZG_NetPrice_Mandatory()
		{
			CombineAssertions(() =>
			{
				invoiceHeader.JobDeclaration.ZG_IsHighValueOvrd = true;
				invoiceLine.JI_NetPrice = ZDecimal.Zero;
				invoiceLine.JI_FormattedProcedure = "40008E2";
				AssertHasMessageError("ZG_IsHighValueOvrd True, Concession 8E2, JI_NetPrice = 0", invoiceLine.JI_NetPriceInfo, "You have not entered a value.");

				invoiceLine.JI_NetPrice = new ZDecimal(1);
				AssertNoMessageError("ZG_IsHighValueOvrd True, Concession 8E2, JI_NetPrice <> 0", invoiceLine.JI_NetPriceInfo, "You have not entered a value.");

				invoiceHeader.JobDeclaration.ZG_IsHighValueOvrd = false;
				invoiceLine.JI_NetPrice = ZDecimal.Zero;
				AssertNoMessageError("ZG_IsHighValueOvrd False, Concession 8E2, JI_NetPrice = 0", invoiceLine.JI_NetPriceInfo, "You have not entered a value.");

				invoiceHeader.JobDeclaration.ZG_IsHighValueOvrd = true;
				invoiceLine.JI_FormattedProcedure = "4000E01";
				invoiceLine.JI_NetPrice = ZDecimal.Zero;
				AssertNoMessageError("ZG_IsHighValueOvrd True, Concession E01, JI_NetPrice = 0", invoiceLine.JI_NetPriceInfo, "You have not entered a value.");

				invoiceLine.JI_FormattedProcedure = "4000E02";
				invoiceLine.JI_NetPrice = ZDecimal.Zero;
				AssertNoMessageError("ZG_IsHighValueOvrd True, Concession E02, JI_NetPrice = 0", invoiceLine.JI_NetPriceInfo, "You have not entered a value.");
			});
		}

		public void TestCheckZG_NetPrice_Negative()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_NetPrice = -1m;
				AssertHasMessageErrorContaining("Negative", invoiceLine.JI_NetPriceInfo, MandatoryValidation.ValueCannotBeNegative);

				invoiceLine.JI_NetPrice = 1m;
				AssertNoMessageErrorContaining("Positive", invoiceLine.JI_NetPriceInfo, MandatoryValidation.ValueCannotBeNegative);
			});
		}

		public void TestCheckZG_NetPrice_LessOrEqualJI_LinePrice()
		{
			const string message = "Net. Price must be less or equal [42] Price.";

			invoiceLine.JI_LinePrice = 50m;
			invoiceLine.JI_NetPrice = 100m;
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("JI_NetPrice > JI_LinePrice", invoiceLine.JI_NetPriceInfo, message);

				invoiceLine.JI_NetPrice = 50m;
				AssertNoMessageErrorContaining("JI_NetPrice == JI_LinePrice", invoiceLine.JI_NetPriceInfo, message);

				invoiceLine.JI_NetPrice = 40m;
				AssertNoMessageErrorContaining("JI_NetPrice < JI_LinePrice", invoiceLine.JI_NetPriceInfo, message);
			});
		}

		public void TestCheckZG_NetPrice_IsHighValueOvrd()
		{
			const string warningMessage = "The sum of Net Price and Discount Value (Charges Code 'DIS') is not equal to Price.";
			declaration.ZG_IsHighValueOvrd = true;
			invoiceLine.JI_NetPrice = 450;
			var lineCharge = invoiceLine.Charges.AddNew();
			lineCharge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.Discount;
			lineCharge.J7_Amount = 50;

			CombineAssertions(() =>
			{
				invoiceLine.JI_LinePrice = 500;
				invoiceLine.AddInfoValidation.ValidateZG_NetPrice();
				AssertNoWarning("JI_NetPrice + DIS = JI_LinePrice", invoiceLine.JI_NetPriceInfo, warningMessage);

				invoiceLine.JI_LinePrice = 480;
				invoiceLine.AddInfoValidation.ValidateZG_NetPrice();
				AssertHasWarning("JI_NetPrice + DIS <> JI_LinePrice", invoiceLine.JI_NetPriceInfo, warningMessage);
			});
		}

		public void TestCheckZG_NetPrice_IsHighValueOvrdFalse()
		{
			const string warningMessage = "The sum of Net Price and Discount Value (Charges Code 'DIS') is not equal to Price.";
			declaration.ZG_IsHighValueOvrd = false;
			invoiceLine.JI_NetPrice = 450;
			invoiceLine.JI_LinePrice = 500;
			var lineCharge = invoiceLine.Charges.AddNew();
			lineCharge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.Discount;
			lineCharge.J7_Amount = 10;

			invoiceLine.AddInfoValidation.ValidateZG_NetPrice();
			AssertEquals(false, invoiceLine.JI_NetPriceInfo.HasWarning(warningMessage));
		}

		public void TestCheckZG_QuotaQty()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_ConcessionOrder = "1";
				invoiceLine.AddInfoValidation.ValidateZG_QuotaQty();
				AssertHasMessageErrorContaining(invoiceLine.ZG_QuotaQtyInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_ConcessionOrder = "1";
				invoiceLine.ZG_QuotaQty = 1;
				invoiceLine.AddInfoValidation.ValidateZG_QuotaQty();
				AssertNoMessageErrorContaining(invoiceLine.ZG_QuotaQtyInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_ConcessionOrder = ZString.Empty;
				invoiceLine.ZG_QuotaQty = 0;
				invoiceLine.AddInfoValidation.ValidateZG_QuotaQty();
				AssertNoMessageErrorContaining(invoiceLine.ZG_QuotaQtyInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckZG_QuotaUQ()
		{
			SetupCustomsUQReferenceData();

			declaration.JE_MessageType = "IMP";
			CombineAssertions(() =>
			{
				invoiceLine.ZG_QuotaUQ = "XX";
				AssertHasMessageErrorContaining(invoiceLine.ZG_QuotaUQInfo, ListValidation.InvalidCodeMessageError);

				invoiceLine.ZG_QuotaUQ = "ABC";
				AssertNoMessageErrorContaining(invoiceLine.ZG_QuotaUQInfo, ListValidation.InvalidCodeMessageError);

				invoiceLine.ZG_QuotaQty = 1;
				invoiceLine.ZG_QuotaUQ = "";
				invoiceLine.AddInfoValidation.ValidateZG_QuotaUQ();
				AssertHasMessageErrorContaining(invoiceLine.ZG_QuotaUQInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.ZG_QuotaQty = 0;
				invoiceLine.ZG_QuotaUQ = "";
				invoiceLine.AddInfoValidation.ValidateZG_QuotaUQ();
				AssertNoMessageErrorContaining(invoiceLine.ZG_QuotaUQInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		}

		void SetupCustomsUQReferenceData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			Factory.Save();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
