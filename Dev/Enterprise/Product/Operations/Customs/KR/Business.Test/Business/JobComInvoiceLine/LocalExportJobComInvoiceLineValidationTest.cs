using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LocalExportJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521022", new ZDateTime(2015, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "Test Tariff");

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff may not be empty");

			invoiceLine.JI_Tariff = "8429521021";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "The Tariff Code entered is not valid for the current context.");

			invoiceLine.JI_Tariff = "8429521022";
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
		}

		public void TestCheckQuantity()
		{
			CombineAssertions("Check JobComInvoiceLine.JI_InvoiceQuantity", () =>
			{
				invoiceLine.JI_InvoiceQuantity = -10;
				AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, "Please enter an 'Invoice Qty' greater than 0.");
				invoiceLine.JI_InvoiceQuantity = 0;
				AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, "Please enter an 'Invoice Qty' greater than 0.");
				invoiceLine.JI_InvoiceQuantity = 60;
				AssertNoMessageErrors(invoiceLine.JI_InvoiceQuantityInfo);
			});
		}

		public void TestCheckQuantityUnit()
		{
			CombineAssertions("Check JobComInvoiceLine.JI_InvoiceUQ", () =>
			{
				invoiceLine.Validation.ValidateJI_InvoiceUQ();
				AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, MandatoryValidation.YouHaveNotEntered);
				invoiceLine.JI_InvoiceUQ = "-1";
				AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, ListValidation.InvalidCodeMessageError);
				invoiceLine.JI_InvoiceUQ = "BO";
				AssertNoMessageErrors(invoiceLine.JI_InvoiceUQInfo);
			});
		}

		public void TestCheckNetWeight()
		{
			CombineAssertions("Check JobComInvoiceLine.JI_NetWeight and JobComInvoiceLine.JI_NetWeightUQ", () =>
			{
				invoiceLine.JI_NetWeight = -10;
				AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightInfo, "Please enter a 'Net Weight' greater than 0.");
				invoiceLine.JI_NetWeight = 0;
				AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightInfo, "Please enter a 'Net Weight' greater than 0.");
				invoiceLine.JI_NetWeight = 60;
				AssertNoMessageErrors(invoiceLine.JI_NetWeightInfo);

				invoiceLine.JI_NetWeightUQ = ZString.Empty;
				AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
				invoiceLine.JI_NetWeightUQ = "A";
				AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				AssertNoMessageErrors(invoiceLine.JI_NetWeightUQInfo);
			});
		}

		public void TestCheckPreviousEntryNumber()
		{
			invoiceLine.Declaration.JE_ExportGoodsType = "1";
			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_OriginalStateDocType = OriginalStateDocTypeList.Codes._01;
			invoiceLine.JI_PreviousEntryNumber = "AAA";
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, "If 'Previous Document Type' is '01', the length of 'Previous Document No.' must be 19 or 20 characters.");
			var previousEntryNumberWith19digits = "1234567890123456789";
			invoiceLine.JI_PreviousEntryNumber = previousEntryNumberWith19digits;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryNumberInfo);

			invoiceLine.JI_OriginalStateDocType = OriginalStateDocTypeList.Codes._02;
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, "If 'Previous Document Type' is '02', the length of 'Previous Document No.' must be 17 or 18 characters.");
			var previousEntryNumberWith17digits = "12345678901234567";
			invoiceLine.JI_PreviousEntryNumber = previousEntryNumberWith17digits;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryNumberInfo);

			invoiceLine.Declaration.JE_ExportGoodsType = "2";
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryNumberInfo);
		}

		public void TestCheckSupportingDocumentCode()
		{
			invoiceLine.SupportingDocumentCode = "";
			AssertHasMessageErrorContaining(invoiceLine.SupportingDocumentCodeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.SupportingDocumentCode = "01";
			AssertNoMessageErrors(invoiceLine.SupportingDocumentCodeInfo);

			invoiceLine.SupportingDocumentCode = "XX";
			AssertHasMessageErrorContaining(invoiceLine.SupportingDocumentCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckSupportingDocumentReferenceNumber()
		{
			CombineAssertions("Check JobComInvoiceLine.SupportingDocumentReferenceNumber", () =>
			{
				invoiceLine.SupportingDocumentReferenceNumber = "";
				AssertHasMessageErrorContaining(invoiceLine.SupportingDocumentReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.SupportingDocumentReferenceNumber = "01";
				AssertNoMessageErrors(invoiceLine.SupportingDocumentReferenceNumberInfo);
			});
		}

		public void TestCheckPackages()
		{
			CombineAssertions("Check JobComInvoiceLine.JI_NoOfPacks", () =>
			{
				invoiceLine.JI_NoOfPacks = -1;
				AssertHasMessageErrorContaining(invoiceLine.JI_NoOfPacksInfo, "Please enter a 'Packages' greater than 0.");

				invoiceLine.JI_NoOfPacks = 0;
				AssertHasMessageErrorContaining(invoiceLine.JI_NoOfPacksInfo, "Please enter a 'Packages' greater than 0.");

				invoiceLine.JI_NoOfPacks = 1;
				AssertNoMessageErrors(invoiceLine.JI_NoOfPacksInfo);
			});
		}

		public void TestCheckPackagesType()
		{
			CombineAssertions("Check JobComInvoiceLine.JI_PackType", () =>
			{
				invoiceLine.JI_PackType = ZString.Empty;
				AssertHasMessageErrorContaining(invoiceLine.JI_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_PackType = "12";
				AssertHasMessageErrorContaining(invoiceLine.JI_PackTypeInfo, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_PackType = PackageKindCodeList.Codes.BG;
				AssertNoMessageErrors(invoiceLine.JI_PackTypeInfo);
			});
		}

		public void TestCheckOriginalStateDocType()
		{
			invoiceLine.Declaration.JE_ExportGoodsType = "1";
			invoiceLine.JI_OriginalStateDocType = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_OriginalStateDocTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_OriginalStateDocType = "01";
			AssertNoMessageErrors(invoiceLine.JI_OriginalStateDocTypeInfo);

			invoiceLine.JI_OriginalStateDocType = "05";
			AssertHasMessageErrorContaining(invoiceLine.JI_OriginalStateDocTypeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.Declaration.JE_ExportGoodsType = "2";
			invoiceLine.JI_OriginalStateDocType = "01";
			AssertHasMessageErrorContaining(invoiceLine.JI_OriginalStateDocTypeInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestCheckIngredient()
		{
			invoiceLine.Declaration.JE_MessageSubType = "01";
			invoiceLine.JI_Ingredient = "";
			AssertNoMessageErrors(invoiceLine.JI_IngredientInfo);

			invoiceLine.JI_Ingredient = "AAA";
			AssertNoMessageErrors(invoiceLine.JI_IngredientInfo);

			invoiceLine.Declaration.JE_MessageSubType = "02";
			invoiceLine.JI_Ingredient = "BBB";
			AssertHasMessageErrorContaining(invoiceLine.JI_IngredientInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_Ingredient = "";
			AssertNoMessageErrors(invoiceLine.JI_IngredientInfo);
		}

		public void TestCheckInboundDate()
		{
			invoiceLine.Declaration.JE_MessageSubType = "07";
			invoiceLine.JI_InboundDate = ZDateTime.Empty;
			AssertNoMessageErrors(invoiceLine.JI_InboundDateInfo);

			invoiceLine.Declaration.JE_MessageSubType = "01";
			invoiceLine.Validation.ValidateJI_InboundDate();
			AssertHasMessageErrorContaining(invoiceLine.JI_InboundDateInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_InboundDate = ZDateTime.Today;
			AssertHasMessageErrorContaining(invoiceLine.JI_InboundDateInfo, "The inbound date should be in the past.");

			invoiceLine.JI_InboundDate = ZDateTime.Today.AddDays(1);
			AssertHasMessageErrorContaining(invoiceLine.JI_InboundDateInfo, "The inbound date should be in the past.");

			invoiceLine.JI_InboundDate = ZDateTime.Today.AddDays(-1);
			AssertNoMessageErrors(invoiceLine.JI_InboundDateInfo);

			invoiceLine.Declaration.JE_MessageSubType = "07";
			invoiceLine.Validation.ValidateJI_InboundDate();
			AssertHasMessageErrorContaining(invoiceLine.JI_InboundDateInfo, MandatoryValidation.DoNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			Factory.Save();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
