using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExitSummaryJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest<ExitSummaryJobComInvoiceHeaderValidation>
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

		public void TestJZ_Incoterm_MessageType()
		{
			(_, var invoice, _) = SetupData();
			invoice.JZ_IncoTerm = ZString.Empty;
			AssertNoMessageErrors($"Do not perform validation on Incoterm when JE_MessageType is EXS", invoice.JZ_IncoTermInfo);
		}

		public void TestJZ_IncoTermPlace()
		{
			(_, var invoice, _) = SetupData();
			invoice.JZ_IncoTerm = ZString.Empty;
			AssertNoMessageErrors($"Do not perform validation on IncotermPlace when JE_MessageType is EXS", invoice.JZ_IncoTermPlaceInfo);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			AssertNoMessageErrors($"Do not perform validation on IncotermPlace when JE_MessageType is EXS", invoice.JZ_IncoTermPlaceInfo);
		}

		public void TestJZ_AdditionalTerms()
		{
			(_, var invoice, _) = SetupData();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoice.JZ_AdditionalTerms = ZString.Empty;
			AssertNoMessageErrors($"JZ_AdditionalTerms is not required for JE_MessageType EXS", invoice.JZ_AdditionalTermsInfo);
		}

		public void TestJZ_ValuationCode()
		{
			// Tran. Nature
			(_, var invoice, var declaration) = SetupData();
			var entry = declaration.CustomsEntryInstructions.AddNew();
			entry.CEI_Style = "A1";
			entry.CEI_SubStyle = "A";
			var line = invoice.InvoiceLines.AddNew();
			line.JI_CEI = entry.PK;
			invoice.JZ_ValuationCode = ZString.Empty;
			AssertNoMessageErrors($"Tran. Nature should not have any validation when JE_MessageType is EXS", invoice.JZ_ValuationCodeInfo);
		}

		public void TestCheckTransportDocuments()
		{
			(_, var invoice, var declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;

			string rowMessageError = CommonResStrings.ProvideAtLeastOneTransportDocument;
			invoice2.Validation.ValidateAll();
			AssertNoRowMessageError("When no TransportDocuments", invoice, rowMessageError);

			var additionalInfo1 = invoice.AdditionalInfos.AddNew();
			additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			var additionalInfo2 = invoice2.AdditionalInfos.AddNew();
			additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			invoice2.Validation.ValidateAll();
			AssertHasRowMessageError("When partial invoices have TransportDocuments", invoice2, rowMessageError);

			additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoice2.Validation.ValidateAll();
			AssertNoRowMessageError("When all invoices have TransportDocuments", invoice2, rowMessageError);
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.ExitSummary;
	}
}
