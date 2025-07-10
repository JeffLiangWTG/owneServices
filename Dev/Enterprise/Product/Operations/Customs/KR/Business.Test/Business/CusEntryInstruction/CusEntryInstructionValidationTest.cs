using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	public class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEI_Style()
		{
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageErrors(instruction.CEI_StyleInfo);
			instruction.CEI_Style = "XXXXX";
			AssertNoMessageErrors(instruction.CEI_StyleInfo);
		}

		public void TestCheckCEI_AgreedRateApp()
		{
			instruction.CEI_AgreedRateApp = ZString.Empty;
			AssertHasMessageErrorContaining(instruction.CEI_AgreedRateAppInfo, MandatoryValidation.YouHaveNotEntered);

			instruction.CEI_AgreedRateApp = YesNoList.Codes.Yes;
			AssertNoMessageErrors(instruction.CEI_AgreedRateAppInfo);

			instruction.CEI_AgreedRateApp = YesNoList.Codes.No;
			AssertNoMessageErrors(instruction.CEI_AgreedRateAppInfo);

			instruction.CEI_AgreedRateApp = "X";
			AssertHasMessageErrorContaining(instruction.CEI_AgreedRateAppInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCEI_PackQty()
		{
			instruction.CEI_PackQty = -1;
			AssertHasMessageErrorContaining(instruction.CEI_PackQtyInfo, "Total Pack Qty cannot be negative.");

			declaration.JE_TotalNoOfPacksPackType = PackageKindCodeList.Codes.BA;
			instruction.CEI_PackQty = 0;
			AssertHasMessageErrorContaining(instruction.CEI_PackQtyInfo, "Total Pack Qty must be greater than zero.");

			declaration.JE_TotalNoOfPacksPackType = ZString.Empty;
			instruction.CEI_PackQty = 10;
			AssertHasMessageErrorContaining(instruction.CEI_PackQtyInfo, "Since the pack type is empty, Total Pack Qty should be 0.");

			declaration.JE_TotalNoOfPacksPackType = PackageKindCodeList.Codes.VG;
			instruction.CEI_PackQty = 0;
			AssertNoMessageErrors(instruction.CEI_PackQtyInfo);
		}

		public void TestCheckCEI_RefundType()
		{
			instruction.Validation.ValidateCEI_RefundType();
			AssertNoMessageErrors(instruction.CEI_RefundTypeInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5UL);
			instruction.Validation.ValidateCEI_RefundType();
			AssertHasMessageErrorContaining(instruction.CEI_RefundTypeInfo, MandatoryValidation.YouHaveNotEntered);

			instruction.CEI_RefundType = RefundTypeList.Codes.A;
			AssertNoMessageErrors(instruction.CEI_RefundTypeInfo);

			instruction.CEI_RefundType = "X";
			AssertHasMessageErrorContaining(instruction.CEI_RefundTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCEI_RefundCauseCode()
		{
			instruction.CEI_RefundType = RefundTypeList.Codes.A;
			instruction.Validation.ValidateCEI_RefundCauseCode();
			AssertNoMessageErrors(instruction.CEI_RefundCauseCodeInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5UL);
			instruction.Validation.ValidateCEI_RefundCauseCode();
			AssertHasMessageErrorContaining(instruction.CEI_RefundCauseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			instruction.CEI_RefundType = RefundTypeList.Codes.B;
			instruction.Validation.ValidateCEI_RefundCauseCode();
			AssertNoMessageErrors(instruction.CEI_RefundCauseCodeInfo);

			instruction.CEI_RefundType = RefundTypeList.Codes.A;
			instruction.CEI_RefundCauseCode = RefundCauseCodeList.Codes._11;
			AssertNoMessageErrors(instruction.CEI_RefundCauseCodeInfo);

			instruction.CEI_RefundCauseCode = "X";
			AssertHasMessageErrorContaining(instruction.CEI_RefundCauseCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCEI_RefundReasonCode()
		{
			instruction.CEI_RefundCauseCode = RefundCauseCodeList.Codes._04;
			instruction.Validation.ValidateCEI_RefundReasonCode();
			AssertNoMessageErrors(instruction.CEI_RefundReasonCodeInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5UL);
			instruction.Validation.ValidateCEI_RefundReasonCode();
			AssertHasMessageErrorContaining(instruction.CEI_RefundReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);

			instruction.CEI_RefundCauseCode = RefundCauseCodeList.Codes._11;
			instruction.Validation.ValidateCEI_RefundReasonCode();
			AssertNoMessageErrors(instruction.CEI_RefundReasonCodeInfo);

			instruction.CEI_RefundCauseCode = RefundCauseCodeList.Codes._04;
			instruction.CEI_RefundReasonCode = RefundReasonCodeList.Codes._01;
			AssertNoMessageErrors(instruction.CEI_RefundReasonCodeInfo);

			instruction.CEI_RefundReasonCode = "X";
			AssertHasMessageErrorContaining(instruction.CEI_RefundReasonCodeInfo, ListValidation.InvalidCodeMessageError);
		}
		public void TestCheckCEI_BondedFactoryUseCode()
		{
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._12;
			instruction.CEI_BondedFactoryUseCode = ZString.Empty;
			AssertHasMessageErrorContaining(instruction.CEI_BondedFactoryUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CEI_BondedFactoryUseCode = BondedFactoryUseCodeList.Codes.A;
			AssertNoMessageErrors(instruction.CEI_BondedFactoryUseCodeInfo);
			instruction.CEI_BondedFactoryUseCode = BondedFactoryUseCodeList.Codes.B;
			AssertNoMessageErrors(instruction.CEI_BondedFactoryUseCodeInfo);
			instruction.CEI_BondedFactoryUseCode = "X";
			AssertHasMessageErrorContaining(instruction.CEI_BondedFactoryUseCodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_ProcedureType = ZString.Empty;
			instruction.CEI_BondedFactoryUseCode = ZString.Empty;
			AssertNoMessageErrors(instruction.CEI_BondedFactoryUseCodeInfo);

			instruction.CEI_BondedFactoryUseCode = BondedFactoryUseCodeList.Codes.A;
			AssertNoMessageErrors(instruction.CEI_BondedFactoryUseCodeInfo);
			instruction.CEI_BondedFactoryUseCode = BondedFactoryUseCodeList.Codes.B;
			AssertNoMessageErrors(instruction.CEI_BondedFactoryUseCodeInfo);
			instruction.CEI_BondedFactoryUseCode = "X";
			AssertHasMessageErrorContaining(instruction.CEI_BondedFactoryUseCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCEI_BondedFactoryArrivalDate()
		{
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._12;
			instruction.CEI_BondedFactoryArrivalDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(instruction.CEI_BondedFactoryArrivalDateInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CEI_BondedFactoryArrivalDate = ZDateTime.Today;
			AssertNoMessageErrors(instruction.CEI_BondedFactoryArrivalDateInfo);

			declaration.JE_ProcedureType = ZString.Empty;
			instruction.Validation.ValidateCEI_BondedFactoryArrivalDate();
			AssertNoMessageErrors(instruction.CEI_BondedFactoryArrivalDateInfo);
			instruction.CEI_BondedFactoryArrivalDate = ZDateTime.Today;
			AssertNoMessageErrors(instruction.CEI_BondedFactoryArrivalDateInfo);
		}

		public void TestCheckCEI_FTARelationArticleCode()
		{
			instruction.Validation.ValidateCEI_FTARelationArticleCode();
			AssertNoMessageErrors(instruction.CEI_FTARelationArticleCodeInfo);
			instruction.CEI_FTARelationArticleCode = FTALawCodeList.Codes._1;
			AssertNoMessageErrors(instruction.CEI_FTARelationArticleCodeInfo);
			instruction.CEI_FTARelationArticleCode = "A";
			AssertHasMessageErrorContaining(instruction.CEI_FTARelationArticleCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction = declaration.CustomsEntryInstructions.AddNew();
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
	}
}
