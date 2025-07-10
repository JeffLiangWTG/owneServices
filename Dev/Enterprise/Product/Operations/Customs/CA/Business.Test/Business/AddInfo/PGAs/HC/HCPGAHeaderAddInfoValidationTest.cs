using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	sealed class HCPGAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_IntendedUseCodeAPI()
		{
			header.CA_APIProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodeAPIInfo, "HC13");
		}
		public void TestCheckCA_IntendedUseCodeBBC()
		{
			header.CA_BBCProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodeBBCInfo, "HC01");
		}
		public void TestCheckCA_IntendedUseCodeCTO()
		{
			header.CA_CTOProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodeCTOInfo, "HC01");
		}
		public void TestCheckCA_IntendedUseCodeCPR()
		{
			header.CA_CPRProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodeCPRInfo, "HC21");
			string madatoryMsg = "You have not entered a value.";
			header.CA_IntendedUseCodeCPR = "";
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeCPRInfo, madatoryMsg);
		}
		public void TestCheckCA_IntendedUseCodeDSE()
		{
			header.CA_DSEProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodeDSEInfo, "HC02");
			string madatoryMsg = "You have not entered a value.";
			header.CA_IntendedUseCodeDSE = "";
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeDSEInfo, madatoryMsg);
		}
		public void TestCheckCA_IntendedUseCodeHDR()
		{
			header.CA_HDRProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodeHDRInfo, "HC05");
			string madatoryMsg = "You have not entered a value.";
			header.CA_IntendedUseCodeHDR = "";
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeHDRInfo, madatoryMsg);
		}
		public void TestCheckCA_IntendedUseCodeNHP()
		{
			header.CA_NHPProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodeNHPInfo, "HC07");
			string madatoryMsg = "You have not entered a value.";
			header.CA_IntendedUseCodeNHP = "";
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeNHPInfo, madatoryMsg);
		}
		public void TestCheckCA_IntendedUseCodeOCS()
		{
			header.CA_OCSProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodeOCSInfo, "HC18");
			string madatoryMsg = "You have not entered a value.";
			header.CA_IntendedUseCodeOCS = "";
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeOCSInfo, madatoryMsg);
		}
		public void TestCheckCA_IntendedUseCodeMDE()
		{
			header.CA_MDEProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodeMDEInfo, "HC04");

			var messageError = "If 'Medical Device Establishment License Exemption' is indicated, the Intended Use Code must equal HC01: Human Therapeutic Use.";
			header.CA_MDE_LEX = true;

			header.CA_IntendedUseCodeMDE = HCIntendedUseCode.Codes.HC02;
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeMDEInfo, messageError);

			header.CA_IntendedUseCodeMDE = HCIntendedUseCode.Codes.HC01;
			AssertNoMessageErrorContaining(header.CA_IntendedUseCodeMDEInfo, messageError);

			header.CA_MDE_LEX = false;
			header.AddInfoValidation.ValidateCA_IntendedUseCodeMDE();
			AssertNoMessageErrorContaining(header.CA_IntendedUseCodeMDEInfo, messageError);
		}
		public void TestCheckCA_IntendedUseCodePES()
		{
			header.CA_PESProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodePESInfo, "HC09");
		}
		public void TestCheckCA_IntendedUseCodeVET()
		{
			header.CA_VETProgramInd = YesNoList.Codes.Yes;
			ValidateCA_IntendedUseCodeList(header, header.CA_IntendedUseCodeVETInfo, "HC11");
		}
		void ValidateCA_IntendedUseCodeList(HCPGAHeader hederHC, ZPropertyInfo propertyIntendedUseCodeAddInfo, ZString caIntentedUseCode)
		{
			propertyIntendedUseCodeAddInfo.Value = (ZString)"XXX";
			hederHC.AddInfoValidation.ValidateAll();
			AssertHasMessageError(propertyIntendedUseCodeAddInfo, ListValidation.InvalidCodeMessageError);

			propertyIntendedUseCodeAddInfo.Value = caIntentedUseCode;
			hederHC.AddInfoValidation.ValidateAll();
			AssertNoMessageError(propertyIntendedUseCodeAddInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCA_CategoryAPI()
		{
			header.CA_APIProgramInd = YesNoList.Codes.Yes;
			ValidateCA_CategoryLookupList(header, header.CA_IntendedUseCodeAPIInfo, header.CA_CategoryAPIInfo, "HC13", "HC01");

			header.CA_CategoryAPI = "";
			string madatoryMsg = "You have not entered a value.";
			AssertHasMessageErrorContaining(header.CA_CategoryAPIInfo, madatoryMsg);
		}

		public void TestCheckCA_CategoryBBC()
		{
			header.CA_BBCProgramInd = YesNoList.Codes.Yes;
			ValidateCA_CategoryLookupList(header, header.CA_IntendedUseCodeBBCInfo, header.CA_CategoryBBCInfo, "HC01", "HC02");

			header.CA_CategoryBBC = "";
			string madatoryMsg = "You have not entered a value.";
			AssertHasMessageErrorContaining(header.CA_CategoryBBCInfo, madatoryMsg);
		}

		public void TestCheckCA_CategoryCTO()
		{
			header.CA_CTOProgramInd = YesNoList.Codes.Yes;
			ValidateCA_CategoryLookupList(header, header.CA_IntendedUseCodeCTOInfo, header.CA_CategoryCTOInfo, "HC01", "HC26");

			header.CA_CategoryCTO = "";
			string madatoryMsg = "You have not entered a value.";
			AssertHasMessageErrorContaining(header.CA_CategoryCTOInfo, madatoryMsg);
		}
		public void TestCheckCA_CategoryHDR()
		{
			header.CA_HDRProgramInd = YesNoList.Codes.Yes;
			ValidateCA_CategoryLookupList(header, header.CA_IntendedUseCodeHDRInfo, header.CA_CategoryHDRInfo, "HC29", "HC05");

			header.CA_CategoryHDR = "";
			string madatoryMsg = "You have not entered a value.";
			AssertHasMessageErrorContaining(header.CA_CategoryHDRInfo, madatoryMsg);
		}

		public void TestCheckCA_CategoryOCS()
		{
			header.CA_OCSProgramInd = YesNoList.Codes.Yes;
			ValidateCA_CategoryLookupList(header, header.CA_IntendedUseCodeOCSInfo, header.CA_CategoryOCSInfo, "HC17", "HC23");

			header.CA_CategoryOCS = "";
			string madatoryMsg = "You have not entered a value.";
			AssertHasMessageErrorContaining(header.CA_CategoryOCSInfo, madatoryMsg);
		}

		public void TestCheckCA_CategoryMDE()
		{
			header.CA_MDEProgramInd = YesNoList.Codes.Yes;
			header.CA_IntendedUseCodeMDE = "";
			header.CA_CategoryMDE = "";
			ValidateCA_CategoryLookupList(header, header.CA_IntendedUseCodeMDEInfo, header.CA_CategoryMDEInfo, "HC01", "HC11");

			header.CA_CategoryMDE = "";
			string madatoryMsg = "You have not entered a value.";
			AssertHasMessageErrorContaining(header.CA_CategoryMDEInfo, madatoryMsg);
		}

		public void TestCheckCA_CategoryNHP()
		{
			header.CA_NHPProgramInd = YesNoList.Codes.Yes;
			ValidateCA_CategoryLookupList(header, header.CA_IntendedUseCodeNHPInfo, header.CA_CategoryNHPInfo, "HC05", "HC15");

			header.CA_CategoryNHP = "";
			string madatoryMsg = "You have not entered a value.";
			AssertHasMessageErrorContaining(header.CA_CategoryNHPInfo, madatoryMsg);
		}

		public void TestCheckCA_CategoryPES()
		{
			header.CA_PESProgramInd = YesNoList.Codes.Yes;
			ValidateCA_CategoryLookupList(header, header.CA_IntendedUseCodePESInfo, header.CA_CategoryPESInfo, "HC06", "HC38");

			header.CA_CategoryPES = "";
			string madatoryMsg = "You have not entered a value.";
			AssertHasMessageErrorContaining(header.CA_CategoryPESInfo, madatoryMsg);
		}
		public void TestCheckCA_CategoryRED()
		{
			header.CA_REDProgramInd = YesNoList.Codes.Yes;
			ValidateCA_CategoryLookupList(header, header.CA_IntendedUseCodeREDInfo, header.CA_CategoryREDInfo, "", "HC41");

			header.CA_CategoryRED = "";
			string madatoryMsg = "You have not entered a value.";
			AssertHasMessageErrorContaining(header.CA_CategoryREDInfo, madatoryMsg);
		}
		public void TestCheckCA_CategoryVET()
		{
			header.CA_VETProgramInd = YesNoList.Codes.Yes;
			ValidateCA_CategoryLookupList(header, header.CA_IntendedUseCodeVETInfo, header.CA_CategoryVETInfo, "HC10", "HC16");

			header.CA_CategoryVET = "";
			string madatoryMsg = "You have not entered a value.";
			AssertHasMessageErrorContaining(header.CA_CategoryVETInfo, madatoryMsg);
		}

		void ValidateCA_CategoryLookupList(HCPGAHeader hederHC, ZPropertyInfo propertyIntendedUseCodeAddInfo, ZPropertyInfo propertyCategoryAddInfo, ZString caIntentedUseCode, ZString caCategory)
		{
			propertyCategoryAddInfo.Value = (ZString)"XXX";
			hederHC.AddInfoValidation.ValidateAll();
			AssertHasMessageError(propertyCategoryAddInfo, ListValidation.InvalidCodeMessageError);

			propertyIntendedUseCodeAddInfo.Value = caIntentedUseCode;
			propertyCategoryAddInfo.Value = caCategory;
			header.AddInfoValidation.ValidateAll();
			AssertNoMessageError(propertyCategoryAddInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCA_BatchLotNumber()
		{
			header.CA_APIProgramInd = YesNoList.Codes.Yes;
			header.AddInfoValidation.ValidateCA_BatchLotNumber();
			AssertHasMessageErrorContaining(header.CA_BatchLotNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.CA_BatchLotNumber = "XXX";
			AssertNoMessageErrorContaining(header.CA_BatchLotNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_APIProgramInd = YesNoList.Codes.No;
			header.CA_CPRProgramInd = YesNoList.Codes.Yes;
			header.CA_BatchLotNumber = string.Empty;
			AssertNoMessageErrorContaining(header.CA_BatchLotNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_CPRProgramInd = YesNoList.Codes.No;
			header.CA_OCSProgramInd = YesNoList.Codes.Yes;
			header.AddInfoValidation.ValidateCA_BatchLotNumber();
			AssertNoMessageErrorContaining(header.CA_BatchLotNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_OCSProgramInd = YesNoList.Codes.No;
			header.CA_PESProgramInd = YesNoList.Codes.Yes;
			header.AddInfoValidation.ValidateCA_BatchLotNumber();
			AssertNoMessageErrorContaining(header.CA_BatchLotNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_ComplianceStatement()
		{
			header.CA_DSEProgramInd = YesNoList.Codes.Yes;
			header.CA_IntendedUseCodeDSE = HCIntendedUseCode.Codes.HC01;

			header.AddInfoValidation.ValidateCA_ComplianceStatement();
			AssertHasMessageErrorContaining(header.CA_ComplianceStatementInfo, "Certify should be ticked.");

			header.CA_ComplianceStatement = true;
			AssertNoMessageErrorContaining(header.CA_ComplianceStatementInfo, "Certify should be ticked.");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			header = invoiceLine.HCPGAHeader;
		}
		HCPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
