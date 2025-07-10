using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class GACPGAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_CommodityCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.CA_PermitApplication = false;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.GACPGAHeader;
			pgaHeader.CA_CommodityCode = string.Empty;

			AssertNoMessageError(pgaHeader.CA_CommodityCodeInfo, GACPGAHeaderAddInfoValidation.CommodityCodeMustBe14Digit);

			declaration.CA_PermitApplication = true;
			pgaHeader.AddInfoValidation.ValidateCA_CommodityCode();
			AssertNoMessageError(pgaHeader.CA_CommodityCodeInfo, GACPGAHeaderAddInfoValidation.CommodityCodeMustBe14Digit);

			pgaHeader.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;
			pgaHeader.AddInfoValidation.ValidateCA_CommodityCode();
			AssertHasMessageError(pgaHeader.CA_CommodityCodeInfo, GACPGAHeaderAddInfoValidation.CommodityCodeMustBe14Digit);

			pgaHeader.CA_CommodityCode = "abc134";
			AssertHasMessageError(pgaHeader.CA_CommodityCodeInfo, GACPGAHeaderAddInfoValidation.CommodityCodeMustBe14Digit);

			pgaHeader.CA_CommodityCode = "12345678901234";
			AssertNoMessageError(pgaHeader.CA_CommodityCodeInfo, GACPGAHeaderAddInfoValidation.CommodityCodeMustBe14Digit);
		}

		public void TestCheckCA_FTACode()
		{
			header.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(header.CA_FTACodeInfo, "XX", FTAProcessingCodes.Codes.FA01);
		}

		public void TestCheckCA_FibreCountryOfOrigin()
		{
			header.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_FibreCountryOfOriginInfo, "XX", "CA");

			header.CA_FTACode = FTAProcessingCodes.Codes.FA01;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.CA_FibreCountryOfOriginInfo);
		}

		public void TestWhenTariffPreferenceLevelPermitApplicationForTextilesClothing()
		{
			header.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;
			header.CA_FTACode = FTAProcessingCodes.Codes.FA01;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.CA_FibreCountryOfOriginInfo);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.CA_YarnCountryOfOriginInfo);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.CA_FabricCountryOfOriginInfo);

			header.CA_FibreCountryOfOrigin = "XX";
			header.CA_YarnCountryOfOrigin = "XX";
			header.CA_FabricCountryOfOrigin = "XX";
			string msg = ListValidation.InvalidCodeMessageError.ToString();
			CombineAssertions(() =>
			{
				TestCaseWithFactory.AssertHasMessageErrorContaining("Fibre C/O", header.CA_FibreCountryOfOriginInfo, msg);
				TestCaseWithFactory.AssertHasMessageErrorContaining("Yarn C/O", header.CA_YarnCountryOfOriginInfo, msg);
				TestCaseWithFactory.AssertHasMessageErrorContaining("Fabric C/O", header.CA_FabricCountryOfOriginInfo, msg);

				header.CA_FibreCountryOfOrigin = GACPGAHeader.The3rdPartyCountry;
				header.CA_YarnCountryOfOrigin = GACPGAHeader.The3rdPartyCountry;
				header.CA_FabricCountryOfOrigin = GACPGAHeader.The3rdPartyCountry;
				TestCaseWithFactory.AssertNoMessageError("No Message Error on Fibre C/O", header.CA_FibreCountryOfOriginInfo, msg);
				TestCaseWithFactory.AssertNoMessageError("No Message Error on Yarn C/O", header.CA_YarnCountryOfOriginInfo, msg);
				TestCaseWithFactory.AssertNoMessageError("No Message Error on Fabric C/O", header.CA_FabricCountryOfOriginInfo, msg);
			});
		}

		public void TestCheckCA_YarnCountryOfOrigin()
		{
			header.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_YarnCountryOfOriginInfo, "XX", "CA");

			header.CA_FTACode = FTAProcessingCodes.Codes.FA01;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.CA_YarnCountryOfOriginInfo);
		}

		public void TestCheckCA_FabricCountryOfOrigin()
		{
			header.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_FabricCountryOfOriginInfo, "XX", "CA");

			header.CA_FTACode = FTAProcessingCodes.Codes.FA01;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.CA_FabricCountryOfOriginInfo);
		}

		public void TestCheckCA_ComplianceStatement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var gacHeader = invoiceLine.GACPGAHeader;

			gacHeader.CA_ComplianceStatement = false;
			var lpcoView = gacHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			var message = "Certify should be ticked.";
			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2001;
			gacHeader.AddInfoValidation.ValidateCA_ComplianceStatement();

			AssertNoMessageErrorContaining(gacHeader.CA_ComplianceStatementInfo, message);

			gacHeader.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;
			gacHeader.AddInfoValidation.ValidateCA_ComplianceStatement();
			AssertHasMessageErrorContaining(gacHeader.CA_ComplianceStatementInfo, message);

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2003;
			gacHeader.AddInfoValidation.ValidateCA_ComplianceStatement();
			AssertHasMessageErrorContaining(gacHeader.CA_ComplianceStatementInfo, message);

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2004;
			gacHeader.AddInfoValidation.ValidateCA_ComplianceStatement();
			AssertNoMessageErrorContaining(gacHeader.CA_ComplianceStatementInfo, message);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.CA_PermitApplication = false;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "52";
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			header = invoiceLine.GACPGAHeader;
		}

		GACPGAHeader header;

		#endregion
	}
}
