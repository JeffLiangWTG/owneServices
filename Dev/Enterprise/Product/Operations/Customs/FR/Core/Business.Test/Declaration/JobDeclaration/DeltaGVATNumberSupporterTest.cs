using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaGVATNumberSupporterTest : TestCaseWithFactory
	{
		public void TestGetVATDeferNumberForAutoliquidation()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			var declaration = Factory.New<JobDeclaration>();
			var vatNumberSupporter = new DeltaGVATNumberSupporter(declaration);

			AssertEquals("VAT number for Autoliquidation should be empty when organisation has no VAT number set yet.", ZString.Empty, vatNumberSupporter.GetVATDeferNumberForAutoliquidation(importer));

			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA_Importer", Core.Constants.CountryCodes.France);
			AssertEquals("VAT number for Autoliquidation should equal organisation VAT number.", "TVA_Importer", vatNumberSupporter.GetVATDeferNumberForAutoliquidation(importer));
		}

		public void TestValidateVATNumber_MessageErrorCorrectlyAdded_WhenImporterTVARegistrationNumberIsOCCASIONNEL()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "50", "0", "   ", "", "IMP", "");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			SetUpImporter();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			using (declaration.SuspendValidationTesting())
			{
				invoiceLine.JI_FormattedProcedure = "500";
				declaration.AdditionalInfos.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;

				var propertyInfo = declaration.ZG_VATDeferTypeInfo;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertNoMessageErrors("No message errors should be added as G0008 special mention is provided as it's required.", propertyInfo);

				declaration.AdditionalInfos.RemoveAndDeleteAll();
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertHasMessageError("Message error should be added as G0008 special mention is not provided while it's required..", propertyInfo, "Procedure 500 on invoice line 1 requires a G0008 special mention for VAT number.");
			}

			void SetUpImporter()
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				importer.CustomsCodes.AddNew("TVA", "OCCASIONNEL", Core.Constants.CountryCodes.France);
				declaration.JE_OH_Importer = importer.PK;
			}
		}

		public void TestValidateVATNumber_MessageErrorCorrectlyAdded_WhenVATNumberDocumentIsRequired()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "50", "0", "   ", "", "IMP", "");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			using (declaration.SuspendValidationTesting())
			{
				invoiceLine.JI_FormattedProcedure = "500";
				var propertyInfo = declaration.ZG_VATDeferTypeInfo;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertHasMessageError("Message error should be added as no 1008 or G008 supporting document is not provided while VATNumberDocument is required.", propertyInfo, "Procedure 500 on invoice line 1 requires a 1008 or G008 supporting document for VAT number.");

				propertyInfo.ClearAllNotifications();
				var supportingDocument = declaration.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertNoMessageErrors("No message errors should be added as 1008 supporting document is provided as VATNumberDocument is required.", propertyInfo);

				propertyInfo.ClearAllNotifications();
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertNoMessageErrors("No message errors should be added as G008 supporting document is provided as VATNumberDocument is required..", propertyInfo);
			}
		}

		public void TestValidateVATNumber_MessageErrorCorrectlyAdded_WhenVATNumberDocumentIsNotRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			using (declaration.SuspendValidationTesting())
			{
				var propertyInfo = declaration.ZG_VATDeferTypeInfo;

				var supportingDocument = declaration.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertHasMessageError("Message error should be added as G008 supporting document is provided while no VATNumberDocument is required.", propertyInfo, "No procedure requires a 1008 nor G008 supporting document for VAT number.");

				propertyInfo.ClearAllNotifications();
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertHasMessageError("Message error should be added as 1008 supporting document is provided while no VATNumberDocument is required.", propertyInfo, "No procedure requires a 1008 nor G008 supporting document for VAT number.");

				propertyInfo.ClearAllNotifications();
				declaration.SupportingDocuments.RemoveAndDeleteAll();
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertNoMessageErrors("No message errors should be added as 1008 or G008 supporting document are both not provided as no VATNumberDocument is required.", propertyInfo);
			}
		}

		public void TestSetIdentifiedVATNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.VATNumberSupporter.SetIdentifiedVATNumber();
			AssertEquals(1, declaration.SupportingDocuments.Count);
			AssertEquals(VATDeferStrategyCodeList.Codes.IdentifiedVATNumber, declaration.SupportingDocuments[0].CSI_Code);
		}

		public void TestSetUnidentifiedVATNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.VATNumberSupporter.SetUnidentifiedVATNumber();
			AssertEquals(1, declaration.SupportingDocuments.Count);
			AssertEquals(VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber, declaration.SupportingDocuments[0].CSI_Code);
		}

		public void TestHasIdentifiedVATNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			Assert(!declaration.VATNumberSupporter.HasIdentifiedVATNumber(invoiceLine));

			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
			Assert(declaration.VATNumberSupporter.HasIdentifiedVATNumber(invoiceLine));
		}

		public void TestHasUnidentifiedVATNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			Assert(!declaration.VATNumberSupporter.HasUnidentifiedVATNumber(invoiceLine));

			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
			Assert(declaration.VATNumberSupporter.HasUnidentifiedVATNumber(invoiceLine));
		}

		public void TestCodeIsVATNumberRelated()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
			Assert(declaration.VATNumberSupporter.CodeIsVATNumberRelated(supportingDocument));

			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
			Assert(declaration.VATNumberSupporter.CodeIsVATNumberRelated(supportingDocument));

			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.Ai2_SpecialMention;
			Assert(!declaration.VATNumberSupporter.CodeIsVATNumberRelated(supportingDocument));
		}
	}
}
