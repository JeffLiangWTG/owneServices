using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExportAddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_TransportChargesMethodOfPayment_RequiredByCEIStyle()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var mopRequiredMessage = "Method of Payment is mandatory and should be present.";
			invoice.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
			var targetInfo = invoice.ZG_TransportChargesMethodOfPaymentInfo;
			AssertNoMessageError("Instruction without required CEI_Style, no mandatory check.", targetInfo, mopRequiredMessage);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			instruction.CEI_Style = ExitSummaryDeclarationTypeList.Codes.A1;
			invoice.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
			AssertHasMessageError("Mandatory check(A1).", targetInfo, mopRequiredMessage);

			instruction.CEI_Style = ExitSummaryDeclarationTypeList.Codes.A2;
			invoice.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
			AssertHasMessageError("Mandatory check(A2).", targetInfo, mopRequiredMessage);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			invoice.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
			AssertHasMessageError("Mandatory check(B1).", targetInfo, mopRequiredMessage);
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B2;
			invoice.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
			AssertHasMessageError("Mandatory check(B2).", targetInfo, mopRequiredMessage);
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.C1;
			invoice.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
			AssertHasMessageError("Mandatory check(C1).", targetInfo, mopRequiredMessage);
			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion;

			invoice.ZG_TransportChargesMethodOfPayment = "A";
			AssertNoMessageError("Mandatory check(C1)(Validation passes).", targetInfo, mopRequiredMessage);

			invoice.ZG_TransportChargesMethodOfPayment = string.Empty;
			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;
			invoice.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
			AssertNoMessageError("NOT Mandatory when JE_EntryStyle is CO.", targetInfo, mopRequiredMessage);

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			invoice.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
			AssertNoMessageError("No Mandatory check(B3).", targetInfo, mopRequiredMessage);
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B4;
			invoice.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
			AssertNoMessageError("No Mandatory check(B4).", targetInfo, mopRequiredMessage);
		}

		public void TestCheckZG_AgreedPlaceCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();

			var errorMessageZG_AgreedPlaceCode = $"Please do not enter an {invoice.ZG_AgreedPlaceCodeInfo.HumanReadableName}. This should be empty when {invoice.JZ_IncoTermInfo.HumanReadableName} is {Core.Constants.IncoTerms.Other}.";

			invoice.JZ_IncoTerm = "";
			invoice.ZG_AgreedPlaceCode = "";
			AssertNoMessageError("Should not have 'This should be empty...' error when JZ_IncoTerm is empty", invoice.ZG_AgreedPlaceCodeInfo, errorMessageZG_AgreedPlaceCode);
			AssertNoMessageError("Should not have 'INCO terms place is required when INCO terms is present.' message error when ZG_AgreedPlaceCode is empty and JZ_IncoTerm is empty", invoice.ZG_AgreedPlaceCodeInfo, "INCO terms place is required when INCO terms is present.");

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoice.ZG_AgreedPlaceCode = "";
			AssertNoMessageError("Should not have 'This should be empty...' error when JZ_IncoTerm is XXX and ZG_AgreedPlaceCode is empty", invoice.ZG_AgreedPlaceCodeInfo, errorMessageZG_AgreedPlaceCode);
			AssertNoMessageError("Should not have 'INCO terms place is required when INCO terms is present.' message error when ZG_AgreedPlaceCode is empty and JZ_IncoTerm is XXX", invoice.ZG_AgreedPlaceCodeInfo, "INCO terms place is required when INCO terms is present.");

			invoice.ZG_AgreedPlaceCode = "DEF";
			AssertHasMessageError("Should have error when ZG_AgreedPlaceCode is populated", invoice.ZG_AgreedPlaceCodeInfo, errorMessageZG_AgreedPlaceCode);
			AssertNoMessageError("Should not have 'INCO terms place is required when INCO terms is present.' message error when ZG_AgreedPlaceCode is not empty and JZ_IncoTerm is XXX", invoice.ZG_AgreedPlaceCodeInfo, "INCO terms place is required when INCO terms is present.");

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedFreeOnBoard;
			invoice.ZG_AgreedPlaceCode = "";
			AssertNoMessageError("Should not have 'This should be empty...' error when JZ_IncoTerm is not XXX and not empty", invoice.ZG_AgreedPlaceCodeInfo, errorMessageZG_AgreedPlaceCode);
			AssertHasMessageError("Should have 'INCO terms place is required when INCO terms is present.' message error when ZG_AgreedPlaceCode is empty,  JZ_IncoTerm is not empty and not XXX ", invoice.ZG_AgreedPlaceCodeInfo, "INCO terms place is required when INCO terms is present.");

			invoice.ZG_AgreedPlaceCode = "DEF";
			AssertNoMessageError("Should not have 'INCO terms place is required when INCO terms is present.' message error when ZG_AgreedPlaceCode is not empty and JZ_IncoTerm is not empty and not XXX", invoice.ZG_AgreedPlaceCodeInfo, "INCO terms place is required when INCO terms is present.");
		}
	}
}
