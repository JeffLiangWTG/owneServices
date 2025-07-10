using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ReExportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest<ReExportJobComInvoiceHeaderValidation>
	{
		public void TestJZ_InvoiceDate()
		{
			(_, var invoice, var declaration) = SetupData();
			invoice.JZ_InvoiceDate = ZDateTime.Empty;
			AssertNoWarnings("No warning when empty", invoice.JZ_InvoiceDateInfo);
		}

		public void TestJZ_InvoiceNumber_Empty()
		{
			(_, var invoice, var declaration) = SetupData();
			invoice.JZ_InvoiceNumber = ZString.Empty;
			AssertNoWarnings("No warning when empty", invoice.JZ_InvoiceNumberInfo);
		}

		public void TestCheckJZ_IncoTerm()
		{
			(_, var invoice, _) = SetupData();
			invoice.JZ_IncoTerm = ZString.Empty;
			AssertNoNotifications($"Do not perform validation on Incoterm when JE_MessageType is REX", invoice.JZ_IncoTermInfo);
			invoice.JZ_IncoTerm = "!";
			AssertHasWarningContaining(invoice.JZ_IncoTermInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestJZ_IncoTermPlace()
		{
			(_, var invoice, _) = SetupData();
			invoice.JZ_IncoTerm = ZString.Empty;
			invoice.JZ_IncoTermPlace = ZString.Empty;
			AssertNoMessageErrors($"Do not perform validation on IncotermPlace when JE_MessageType is REX", invoice.JZ_IncoTermPlaceInfo);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			AssertNoMessageErrors($"Do not perform validation on IncotermPlace when JE_MessageType is REX", invoice.JZ_IncoTermPlaceInfo);
		}

		public void TestJZ_AdditionalTerms()
		{
			(_, var invoice, _) = SetupData();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoice.JZ_AdditionalTerms = ZString.Empty;
			AssertNoMessageErrors($"JZ_AdditionalTerms is not required for JE_MessageType REX", invoice.JZ_AdditionalTermsInfo);
		}

		public void TestAdditionalInfos_TransportDocumentsMandatory()
		{
			(_, var invoiceHeader1, var declaration) = SetupData();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction1.PK;

			var message = "Please provide at least one Transport Document.";

			CombineAssertions(() =>
			{
				var addInfo1 = invoiceHeader1.AdditionalInfos.AddNew();
				addInfo1.CSI_SubType = "XXX";
				addInfo1.CSI_Description = "XXX Description";

				var addInfo2 = invoiceHeader2.AdditionalInfos.AddNew();
				addInfo2.CSI_SubType = "YYY";
				addInfo2.CSI_Description = "YYY Description";

				invoiceHeader1.Validation.ValidateAll();
				invoiceHeader2.Validation.ValidateAll();

				AssertNoRowMessageError("All invoices have no Transport document 1 (Error should be on Entry Instruction)", invoiceHeader1, message);
				AssertNoRowMessageError("All invoices have no Transport document 2 (Error should be on Entry Instruction)", invoiceHeader2, message);

				addInfo1 = invoiceHeader1.AdditionalInfos.AddNew();
				addInfo1.CSI_SubType = "XXX";
				addInfo1.CSI_Description = "XXX Description";

				addInfo2 = invoiceHeader2.AdditionalInfos.AddNew();
				addInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfo2.CSI_Description = "TRA Description";

				invoiceHeader1.Validation.ValidateAll();
				invoiceHeader2.Validation.ValidateAll();

				AssertHasRowMessageError("Invoice Header withOUT transport doc", invoiceHeader1, message);
				AssertNoRowMessageError("Invoice Header with transport doc", invoiceHeader2, message);

				var addInfo3 = instruction1.AdditionalInfos.AddNew();
				addInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfo3.CSI_Description = "TRA Description";

				invoiceHeader1.Validation.ValidateAll();
				invoiceHeader2.Validation.ValidateAll();

				AssertNoRowMessageError("Entry Instruction with transport doc 1", invoiceHeader1, message);
				AssertNoRowMessageError("Entry Instruction with transport doc 2", invoiceHeader2, message);
			});
		}

		public void TestCheckJZ_ValuationCode()
		{
			(_, var invoice, _) = SetupData();
			invoice.JZ_ValuationCode = ZString.Empty;
			AssertNoNotifications(invoice.JZ_ValuationCodeInfo);
			invoice.JZ_ValuationCode = "!";
			AssertHasWarningContaining(invoice.JZ_ValuationCodeInfo, ListValidation.InvalidCodeMessage);
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.ReExport;
	}
}
