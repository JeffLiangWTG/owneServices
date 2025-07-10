using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;
using static Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestHasMutuallyExclusiveSupportingDocument()
		{
			AssertEquals("empty collection", false, instruction.HasMutuallyExclusiveSupportingDocument);

			var supportingDoc = instruction.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = "666";
			AssertEquals("without target supportingDocument", false, instruction.HasMutuallyExclusiveSupportingDocument);

			supportingDoc.CSI_Code = "U164";
			var supportingDoc2 = instruction.SupportingDocuments.AddNew();
			supportingDoc2.CSI_Code = "U165";
			AssertEquals(true, instruction.HasMutuallyExclusiveSupportingDocument);

			supportingDoc.CSI_Code = "U167";
			AssertEquals("Specific case", false, instruction.HasMutuallyExclusiveSupportingDocument);
		}

		public void TestAddInfo()
		{
			AssertType<AddInfoCusEntryInstruction>(((IAddInfoManager)instruction).AddInfo);
		}

		public void TestRequestedDocumentsReadOnly()
		{
			AssertEquals(true, instruction.RequestedDocuments.ReadOnly);
		}

		public void TestAdditionalProcedures()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			var invoice1Line3 = invoice1.JobComInvoiceLines.AddNew();

			invoice1Line1.JI_CEI = instruction.PK;
			invoice1Line1.JI_FormattedProcedure = "4000C07";
			var invoice1Line1Collection = invoice1Line1.AdditionalProcedureCodes;
			var invoice1Line1AdditionalProcedureCode1 = invoice1Line1Collection.AddNew();
			invoice1Line1AdditionalProcedureCode1.CY_Code = "4000F48";
			var invoice1Line1AdditionalProcedureCode2 = invoice1Line1Collection.AddNew();
			invoice1Line1AdditionalProcedureCode2.CY_Code = "4000C08";

			invoice1Line2.JI_FormattedProcedure = "4000C01";
			var invoice1Line2Collection = invoice1Line2.AdditionalProcedureCodes;
			var invoice1Line2AdditionalProcedureCode1 = invoice1Line2Collection.AddNew();
			invoice1Line2AdditionalProcedureCode1.CY_Code = "4000C09";

			invoice1Line3.JI_CEI = instruction.PK;
			invoice1Line3.JI_FormattedProcedure = "4000F02";
			var invoice1Line3Collection = invoice1Line3.AdditionalProcedureCodes;
			var invoice1Line3AdditionalProcedureCode1 = invoice1Line3Collection.AddNew();
			invoice1Line3AdditionalProcedureCode1.CY_Code = "4000C08";
			var invoice1Line3AdditionalProcedureCode2 = invoice1Line3Collection.AddNew();
			invoice1Line3AdditionalProcedureCode2.CY_Code = "4000D07";

			var invoice2 = declaration.Invoices.AddNew();
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_CEI = instruction.PK;
			invoice2Line1.JI_FormattedProcedure = "4000B02";
			var invoice2Line1Collection = invoice2Line1.AdditionalProcedureCodes;
			var invoice2Line1AdditionalProcedureCode1 = invoice2Line1Collection.AddNew();
			invoice2Line1AdditionalProcedureCode1.CY_Code = "5000K12";

			AssertContainsExactElementsInAnyOrder("All line additional procedures", new[] { "C07", "F48", "C08", "F02", "D07", "B02", "K12" }, instruction.AdditionalProcedures);

			invoice1Line3AdditionalProcedureCode2.CY_Code = "4000D13";
			invoice2Line1.JI_FormattedProcedure = "4000B04";
			AssertContainsExactElementsInAnyOrder("All line additional procedures", new[] { "C07", "F48", "C08", "F02", "D13", "B04", "K12" }, instruction.AdditionalProcedures);
		}

		public void TestIsBR11106MutuallyExclusiveCodesSubsetOfAdditionalProcedures()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_FormattedProcedure = "4000C07";
			var collection = invoiceLine.AdditionalProcedureCodes;
			var procedureCode1 = collection.AddNew();
			procedureCode1.CY_Code = "4000F48";
			var procedureCode2 = collection.AddNew();
			procedureCode2.CY_Code = "4000C09";

			CombineAssertions(() =>
			{
				AssertEquals("AdditionalProcedures: C07, F48, C09", false, instruction.IsBR11106MutuallyExclusiveCodesSubsetOfAdditionalProcedures);

				procedureCode2.CY_Code = "4000C08";
				AssertEquals("AdditionalProcedures: C07, F48, C08", true, instruction.IsBR11106MutuallyExclusiveCodesSubsetOfAdditionalProcedures);
			});
		}

		public void TestIsBR11107MutuallyExclusiveCodesSubsetOfAdditionalProcedures()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_FormattedProcedure = "4000C07";
			var collection = invoiceLine.AdditionalProcedureCodes;
			var procedureCode1 = collection.AddNew();
			procedureCode1.CY_Code = "4000F48";
			var procedureCode2 = collection.AddNew();
			procedureCode2.CY_Code = "4000C09";

			CombineAssertions(() =>
			{
				AssertEquals("AdditionalProcedures: C07, F48, C09", false, instruction.IsBR11107MutuallyExclusiveCodesSubsetOfAdditionalProcedures);

				procedureCode2.CY_Code = "40001C1";
				AssertEquals("AdditionalProcedures: C07, F48, 1C1", true, instruction.IsBR11107MutuallyExclusiveCodesSubsetOfAdditionalProcedures);
			});
		}

		public void TestIsProcedureF48()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				invoiceLine.JI_FormattedProcedure = "4000F48";
				AssertEquals("AdditionalProcedure is F48.", true, instruction.IsProcedureF48);

				invoiceLine.JI_FormattedProcedure = "4000F49";
				AssertEquals("AdditionalProcedure is F49.", false, instruction.IsProcedureF48);
			});
		}

		public void TestIsProcedureF49()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				invoiceLine.JI_FormattedProcedure = "4000F48";
				AssertEquals("AdditionalProcedure is F48.", false, instruction.IsProcedureF49);

				invoiceLine.JI_FormattedProcedure = "4000F49";
				AssertEquals("AdditionalProcedure is F49.", true, instruction.IsProcedureF49);
			});
		}

		public void TestIsTotalPriceLessThanOrEqualTo22EUR()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();

			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_LinePrice = 11m;
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_LinePrice = 11m;
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_LinePrice = 0m;

			AssertEquals("line1(11 EUR) + line2(11 EUR) + line3(0 EUR) <= 22 EUR", true, instruction.IsTotalPriceLessThanOrEqualTo22EUR);

			invoiceLine2.JI_LinePrice = 12m;
			AssertEquals("line1(11 EUR) + line2(12 EUR) > 22 EUR", false, instruction.IsTotalPriceLessThanOrEqualTo22EUR);

			invoiceLine2.JI_LinePrice = 10m;
			invoiceLine3.JI_LinePrice = 1.1m;
			AssertEquals("line1(11 EUR) + line2(10 EUR) + line3(1.1 AUD) < 22 EUR", true, instruction.IsTotalPriceLessThanOrEqualTo22EUR);

			invoiceLine3.JI_LinePrice = 5m;
			AssertEquals("line1(11 EUR) + line2(10 EUR) + line3(5 AUD) > 22 EUR", false, instruction.IsTotalPriceLessThanOrEqualTo22EUR);
		}

		public void TestIsI1EntryAndTotalPriceLessThanOrEqualTo22EUR()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_LinePrice = 11m;
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_LinePrice = 11m;
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_LinePrice = 0m;

			AssertEquals(false, instruction.IsI1EntryAndTotalPriceLessThanOrEqualTo22EUR);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			AssertEquals("CEI_Style is I1, line1(11 EUR) + line2(11 EUR) + line3(0 EUR) <= 22 EUR", true, instruction.IsI1EntryAndTotalPriceLessThanOrEqualTo22EUR);

			invoiceLine3.JI_LinePrice = 1m;
			AssertEquals("CEI_Style is I1, line1(11 EUR) + line2(11 EUR) + line3(1 EUR) > 22 EUR", false, instruction.IsI1EntryAndTotalPriceLessThanOrEqualTo22EUR);
		}

		public void TestPopulateCH_MessageType()
		{
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			AssertEquals("EntryHeader.CH_MessageType", ExportDeclarationTypeList.Codes.B1, instruction.EntryHeader.CH_MessageType);
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.C1;
			AssertEquals("EntryHeader.CH_MessageType", ExportDeclarationTypeList.Codes.C1, instruction.EntryHeader.CH_MessageType);
		}

		public void TestCreateAndPopulateAdditionalRefType1D23()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			declaration.JE_DateAtOrigin = ZDateTime.BrettsBirthday;
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;

			AssertEquals("Additional Document added", 1, instruction.AdditionalInfos.Count);
			AssertEquals("1D23 AdditionalCode Added - code", "1D23", instruction.AdditionalInfos[0].CSI_Code);
			AssertEquals("1D23 AdditionalCode Added - reference", "197109180000", instruction.AdditionalInfos[0].CSI_ReferenceNumber);
			AssertEquals("1D23 AdditionalCode Added - sub-type", "REF", instruction.AdditionalInfos[0].CSI_SubType);

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			AssertEquals("Additional Document not added - 1D23 already added", 1, instruction.AdditionalInfos.Count);
			AssertEquals("Additional Document added", 1, instruction2.AdditionalInfos.Count);
			AssertEquals("1D23 AdditionalCode Added - code", "197109180000", instruction2.AdditionalInfos[0].CSI_ReferenceNumber);
		}

		public void TestCreateAndPopulateAdditionalRefType1D24()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			declaration.JE_DateAtFinalDestination = ZDateTime.BrettsBirthday;
			instruction.AdditionalInfos.RemoveAndDeleteAll();
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			var added1D24message = instruction.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(item => item.CSI_Code == "1D24");
			AssertNull("1D24 AdditionalCode not Added", added1D24message);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			added1D24message = instruction.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(item => item.CSI_Code == "1D24");
			AssertNotNull("1D24 AdditionalCode Added", added1D24message);
			AssertEquals("1D24 AdditionalCode Added - reference", "197109180000", added1D24message.CSI_ReferenceNumber);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			AssertEquals("Additional Document not added - 1D24 already added", 1, instruction.SupportingDocuments.Count);
		}

		public void TestHandleAuthorizations()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			instruction.CEI_SubStyle = "C";
			AssertEquals("CusAuthorizationUsages.Count", 0, instruction.CusAuthorizationUsages.Count);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			instruction.CEI_SubStyle = "B";
			AssertEquals("CusAuthorizationUsages.Count", 0, instruction.CusAuthorizationUsages.Count);

			instruction.CEI_SubStyle = "C";
			AssertEquals("CusAuthorizationUsages.Count", 1, instruction.CusAuthorizationUsages.Count);
			AssertEquals("CusAuthorizationUsages should have a record with type SDE", CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, instruction.CusAuthorizationUsages[0].AGC_Code);

			instruction.CEI_SubStyle = "F";
			AssertEquals("CusAuthorizationUsages.Count", 1, instruction.CusAuthorizationUsages.Count);
			AssertEquals("The record will not be added again", CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, instruction.CusAuthorizationUsages[0].AGC_Code);
		}

		public void TestHasMultipleDeliveryTerms()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "FoB";
			invoice1.JZ_IncoTermPlace = "BoB'S PLACE";
			invoice1.ZG_AgreedPlaceCode = "HeRE";
			invoice1.JZ_AdditionalTerms = "These Terms";
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line.JI_CEI = instruction.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_IncoTermPlace = "BOB'S PLACE";
			invoice2.ZG_AgreedPlaceCode = "HERE";
			invoice2.JZ_AdditionalTerms = "THESE Terms";
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				AssertEquals("HasMultipleDeliveryTerms", false, instruction.HasMultipleDeliveryTerms);
				invoice2.JZ_IncoTerm = "CIF";
				AssertEquals("HasMultipleDeliveryTerms - Different JZ_IncoTerm", true, instruction.HasMultipleDeliveryTerms);
				invoice2.JZ_IncoTerm = "FOB";
				invoice2.JZ_IncoTermPlace = "JOE'S PLACE";
				AssertEquals("HasMultipleDeliveryTerms - Different JZ_IncoTermPlace", true, instruction.HasMultipleDeliveryTerms);
				invoice2.JZ_IncoTermPlace = "BOB'S PLACE";
				invoice2.ZG_AgreedPlaceCode = "THERE";
				AssertEquals("HasMultipleDeliveryTerms - Different ZG_AgreedPlaceCode", true, instruction.HasMultipleDeliveryTerms);
				invoice2.JZ_IncoTermPlace = "BOB'S PLACE";
				invoice2.ZG_AgreedPlaceCode = "HERE";
				invoice2.JZ_AdditionalTerms = "Other Terms";
				AssertEquals("HasMultipleDeliveryTerms - Different JZ_AdditionalTerms", true, instruction.HasMultipleDeliveryTerms);
				invoice2.Delete();
				AssertEquals("HasMultipleDeliveryTerms - only one invoice", false, instruction.HasMultipleDeliveryTerms);
				invoice1.Delete();
				AssertEquals("HasMultipleDeliveryTerms - zero invoice", false, instruction.HasMultipleDeliveryTerms);
			});
		}

		public void TestDoAllInvoiceLinesHaveFR5FiscalReference()
		{
			AssertEquals("No Invoice Lines", false, instruction.DoAllInvoiceLinesHaveFR5FiscalReference);
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line.JI_CEI = instruction.PK;
			AssertEquals("1 InvoiceLine", false, instruction.DoAllInvoiceLinesHaveFR5FiscalReference);
			var invoice2 = declaration.Invoices.AddNew();
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_CEI = instruction.PK;
			var invoice2LineFiscalReference = invoice2Line.FiscalReferences.AddNew();
			invoice2LineFiscalReference.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR5_Vendor;
			AssertEquals("2 InvoiceLines, only InvoiceLine2 has it", false, instruction.DoAllInvoiceLinesHaveFR5FiscalReference);
			var invoice1LineFiscalReference = invoice1Line.FiscalReferences.AddNew();
			invoice1LineFiscalReference.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR2_Customer;
			AssertEquals("InvoiceLine1 has non-FR5", false, instruction.DoAllInvoiceLinesHaveFR5FiscalReference);
			invoice1LineFiscalReference.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR5_Vendor;
			AssertEquals("DoAllInvoiceLinesHaveFR5FiscalReference", true, instruction.DoAllInvoiceLinesHaveFR5FiscalReference);
		}

		public void TestHasInvoiceLineWithPreviousProcedure0700()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Procedure = "0000";
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Procedure = "0711";
			AssertEquals("HasInvoiceLineWithPreviousProcedure0700", false, instruction.HasInvoiceLineWithPreviousProcedure0700);
			invoiceLine2.JI_Procedure = "0700";
			AssertEquals("HasInvoiceLineWithPreviousProcedure0700", true, instruction.HasInvoiceLineWithPreviousProcedure0700);
		}

		public void TestHasMultipleCurrencies()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line.JI_CEI = instruction.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				AssertEquals("same JZ_RX_NKInvoice_Currency", false, instruction.HasMultipleCurrencies);
				invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertEquals("different JZ_RX_NKInvoice_Currency", true, instruction.HasMultipleCurrencies);
				invoice2.Delete();
				AssertEquals("only one invoice", false, instruction.HasMultipleCurrencies);
				invoice1.Delete();
				AssertEquals("zero invoice", false, instruction.HasMultipleCurrencies);
			});
		}

		public void TestHasMultipleNatureOfTransactions()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationCode = NatureOfTransactionList.Codes._11;
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line.JI_CEI = instruction.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = NatureOfTransactionList.Codes._11;
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				AssertEquals("same JZ_ValuationCode", false, instruction.HasMultipleNatureOfTransactions);
				invoice2.JZ_ValuationCode = NatureOfTransactionList.Codes._12;
				AssertEquals("different JZ_ValuationCode", true, instruction.HasMultipleNatureOfTransactions);
				invoice2.Delete();
				AssertEquals("only one invoice", false, instruction.HasMultipleNatureOfTransactions);
				invoice1.Delete();
				AssertEquals("zero invoice", false, instruction.HasMultipleNatureOfTransactions);
			});
		}

		public void TestCusAuthorizationUsages()
		{
			AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>>(instruction.CusAuthorizationUsages);
		}

		public void TestEntryHeader()
		{
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			AssertType<CusEntryHeader>(instruction.EntryHeader);
		}

		public void TestJobDeclaration()
		{
			AssertType<JobDeclaration>(instruction.JobDeclaration);
		}

		public void TestIsSupplementaryDeclarationForCode()
		{
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
			AssertEquals("Y", true, instruction.IsSupplementaryDeclarationForCode);
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic;
			AssertEquals("Z", false, instruction.IsSupplementaryDeclarationForCode);
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
			AssertEquals("X", true, instruction.IsSupplementaryDeclarationForCode);
		}

		public void TestLookups()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<ImportCusEntryInstructionLookups>(instruction.Lookups);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<ExportCusEntryInstructionLookups>(instruction.Lookups);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<CusEntryInstructionLookups>(instruction.Lookups);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertType<ExitSummaryCusEntryInstructionLookups>(instruction.Lookups);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertType<ReExportCusEntryInstructionLookups>(instruction.Lookups);
		}

		public void TestValidation_ImportUCC5()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "V1";
			AssertType<UCC5ImportCusEntryInstructionValidation>(instruction.Validation);
		}

		public void TestValidation_ImportUCC6()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "V2";
			AssertType<UCC6ImportCusEntryInstructionValidation>(instruction.Validation);
		}

		public void TestValidation_Export()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<ExportCusEntryInstructionValidation>(instruction.Validation);
		}

		public void TestValidation_ExitSummary()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertType<ExitSummaryCusEntryInstructionValidation>(instruction.Validation);
		}

		public void TestValidation_ReExport()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertType<ReExportCusEntryInstructionValidation>(instruction.Validation);
		}

		public void TestValidation_MiscellaneousCustoms()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<CusEntryInstructionValidation>(instruction.Validation);
		}

		public void TestCEI_StyleBasedProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			CombineAssertions("CEI_Style based properties", () =>
			{
				AssertEquals("StatisticalValueRequired", true, instruction.StatisticalValueRequired);
				AssertEquals("IsExportOrReExportDeclaration", true, instruction.IsB1Declaration);
				AssertEquals("StatisticalValueRequired", false, instruction.IsCustomsWarehousingOfUnionGoods);

				instruction.CEI_Style = ExportDeclarationTypeList.Codes.B2;
				AssertEquals("StatisticalValueRequired", true, instruction.StatisticalValueRequired);
				AssertEquals("IsExportOrReExportDeclaration", false, instruction.IsB1Declaration);
				AssertEquals("StatisticalValueRequired", false, instruction.IsCustomsWarehousingOfUnionGoods);

				instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
				AssertEquals("StatisticalValueRequired", false, instruction.StatisticalValueRequired);
				AssertEquals("IsExportOrReExportDeclaration", false, instruction.IsB1Declaration);
				AssertEquals("StatisticalValueRequired", true, instruction.IsCustomsWarehousingOfUnionGoods);

				instruction.CEI_Style = ExportDeclarationTypeList.Codes.C1;
				AssertEquals("StatisticalValueRequired", false, instruction.StatisticalValueRequired);
				AssertEquals("IsExportOrReExportDeclaration", false, instruction.IsB1Declaration);
				AssertEquals("StatisticalValueRequired", false, instruction.IsCustomsWarehousingOfUnionGoods);
			});

			CombineAssertions("CEI_Style based properties (StatisticalValueRequired During Transition Period", () =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
				{
					instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
					AssertEquals("StatisticalValueRequired", true, instruction.StatisticalValueRequired);

					instruction.CEI_Style = ExportDeclarationTypeList.Codes.B2;
					AssertEquals("StatisticalValueRequired", true, instruction.StatisticalValueRequired);

					instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
					AssertEquals("StatisticalValueRequired", false, instruction.StatisticalValueRequired);

					instruction.CEI_Style = ExportDeclarationTypeList.Codes.B4;
					AssertEquals("StatisticalValueRequired", true, instruction.StatisticalValueRequired);

					instruction.CEI_Style = ExportDeclarationTypeList.Codes.C1;
					AssertEquals("StatisticalValueRequired", true, instruction.StatisticalValueRequired);
				}
			});
		}

		public void TestIsH1()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			Assert(instruction.IsH1);
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			Assert(!instruction.IsH1);
		}

		public void TestIsH2()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			Assert(instruction.IsH2);
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			Assert(!instruction.IsH1);
		}

		public void TestIsH3()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H3;
			Assert(instruction.IsH3);
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			Assert(!instruction.IsH1);
		}

		public void TestIsH4()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H4;
			Assert(instruction.IsH4);
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			Assert(!instruction.IsH1);
		}

		public void TestIsH5()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H5;
			Assert(instruction.IsH5);
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			Assert(!instruction.IsH1);
		}

		public void TestIsH6()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H6;
			Assert(instruction.IsH6);
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			Assert(!instruction.IsH1);
		}

		public void TestIsI1()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			Assert(instruction.IsI1);
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			Assert(instruction.IsH1);
		}

		public void TestZG_PeriodForDischargeCaption_ImportV1()
		{
			var info = instruction.ZG_PeriodForDischargeInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 4/17] Period (Monthly)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 4/17] Period (Monthly)", resourceStringDataAttribute.Caption);
			});
		}

		public void TestZG_PeriodForDischargeAutoExtensionCaption_ImportV1()
		{
			var info = instruction.ZG_PeriodForDischargeAutoExtensionInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 4/17] Automatic Extension?", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 4/17] Automatic Extension?", resourceStringDataAttribute.Caption);
			});
		}

		public void TestPeriodForDischargeDetailsCaption_ImportV1()
		{
			var info = instruction.PeriodForDischargeDetailsInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 4/17] Details", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 4/17] Details", resourceStringDataAttribute.Caption);
			});
		}

		public void TestZG_BillOfDischargeDeadlineCaption_ImportV1()
		{
			var info = instruction.ZG_BillOfDischargeDeadlineInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 4/18] Deadline", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 4/18] Deadline", resourceStringDataAttribute.Caption);
			});
		}

		public void TestZG_BillOfDischargeIsNecessaryCaption_ImportV1()
		{
			var info = instruction.ZG_BillOfDischargeIsNecessaryInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 4/18] Necessary?", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 4/18] Necessary?", resourceStringDataAttribute.Caption);
			});
		}

		public void TestBillOfDischargeDetailsCaption_ImportV1()
		{
			var info = instruction.BillOfDischargeDetailsInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 4/18] Details", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 4/18] Details", resourceStringDataAttribute.Caption);
			});
		}

		public void TestDetailsOfPlannedActivities_Caption_ImportV1()
		{
			var info = instruction.DetailsOfPlannedActivitiesInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 7/5] Planned Act.", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MediumCaption", "[Art. 163 7/5] Planned Activity", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Caption", "[Article 163 7/5] Details of Planned Activities", resourceStringDataAttribute.Caption);
			});
		}

		public void TestZG_Article86_3_UCCCaption_ImportV1()
		{
			var info = instruction.ZG_Article86_3_UCCInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 8/13] Import Duty Amount", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MediumCaption", "[Art. 163 8/13] Import Duty Amount (per article 86(3))", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Caption", "[Art. 163 8/13] Import Duty Amount in accordance with Article 86(3) of the Code", resourceStringDataAttribute.Caption);
				AssertEquals("Full Description", "[Article 163 8/13] Calculation of the amount of the import duty in accordance with Article 86(3) of the Code", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestAdditionalInformationCaption_ImportV1()
		{
			var info = instruction.AdditionalInformationInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 8/5] Additional Info.", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 8/5] Additional Information", resourceStringDataAttribute.Caption);
			});
		}

		public void TestZG_RateOfYieldCaption_ImportV1()
		{
			var info = instruction.ZG_RateOfYieldInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 5/5] Rate of Yield", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 5/5] Rate of Yield", resourceStringDataAttribute.Caption);
			});
		}

		public void TestZG_ProcessedProductsCommodityCodeCaption_ImportV1()
		{
			var info = instruction.ZG_ProcessedProductsCommodityCodeInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 5/7] Commodity Code", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 5/7] Commodity Code", resourceStringDataAttribute.Caption);
			});
		}

		public void TestProcessedProductDescriptionCaption_ImportV1()
		{
			var info = instruction.ProcessedProductDescriptionInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 5/7] Goods Desc.", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 5/7] Goods Description", resourceStringDataAttribute.Caption);
			});
		}

		public void TestZG_IdOfGoodCodeCaption_ImportV1()
		{
			var info = instruction.ZG_IdOfGoodCodeInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 5/8] Code", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 5/8] Code", resourceStringDataAttribute.Caption);
			});
		}

		public void TestIdentificationofGoodsDetailsCaption_ImportV1()
		{
			var info = instruction.IdentificationofGoodsDetailsInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 5/8] Details", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 5/8] Details", resourceStringDataAttribute.Caption);
			});
		}

		public void TestZG_ProcessingProcedureCodeCaption_ImportV1()
		{
			var info = instruction.ZG_ProcessingProcedureCodeInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 6/2] Processing Procedure", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 6/2] Processing Procedure", resourceStringDataAttribute.Caption);
			});
		}

		public void TestProcessingProcedureDetailsCaption_ImportV1()
		{
			var info = instruction.ProcessingProcedureDetailsInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[Art. 163 6/2] Details", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Caption", "[Article 163 6/2] Details", resourceStringDataAttribute.Caption);
			});
		}

		public void TestGoodsLocationDescriptionCaption()
		{
			var info = instruction.GoodsLocationDescriptionInfo;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);

			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("ShortCaption", "[5/23] Goods Loc.", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MediumCaption", "[5/23] Goods Location", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Caption", "[5/23] Location of goods", resourceStringDataAttribute.Caption);
			});
		}

		public void TestCEI_Style_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var info = instruction.CEI_StyleInfo;

			CombineAssertions("Export", () =>
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);
				AssertEquals("Caption", "Declaration Type", resourceStringDataAttribute.Caption);
				AssertEquals("FullDescription", "[11 01 001 000] Declaration Type", resourceStringDataAttribute.FullDescription);
			});

			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);
			CombineAssertions("Import", () =>
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);
				AssertEquals("ShortCaption", "Decl. Type", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MediumCaption", "[1/1] Decl.Type", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Caption", "[1/1] Declaration Type", resourceStringDataAttribute.Caption);
			});
		}

		public void TestCEI_SubStyle_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var info = instruction.CEI_SubStyleInfo;

			CombineAssertions("Export", () =>
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);
				AssertEquals("Caption", "Sub Style", resourceStringDataAttribute.Caption);
				AssertEquals("FullDescription", "[11 02 001 000] Additional Declaration Type", resourceStringDataAttribute.FullDescription);
			});

			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);
			CombineAssertions("Import", () =>
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, instruction.MultipleKeysToUse);
				AssertEquals("ShortCaption", "[1/2]Add. Type", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MediumCaption", "[1/2] Add. Decl. Type", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Caption", "[1/2] Add. Declaration Type", resourceStringDataAttribute.Caption);
				AssertEquals("FullDescription", "[1/2] Additional Declaration Type", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestCEI_OA_Warehouse2_DefaultAuthorization()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;
			authorizationHeader.CPH_Number = "001";
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OA_AppliesTo = warehouse.MainAddress.PK;

			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

			var usages = entryInstruction.CusAuthorizationUsages;
			CombineAssertions("Default Authorization", () =>
			{
				AssertEquals(1, usages.Count);
				var usage = usages[0];
				AssertEquals(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, usage.AGC_Code);
				AssertEquals("001", usage.AGC_Number);
				AssertEquals(owner.PK, usage.AGC_OH_Owner);
			});
		}

		public void TestCEI_OA_Warehouse_DefaultAuthorization()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;
			authorizationHeader.CPH_Number = "001";
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OA_AppliesTo = warehouse.MainAddress.PK;

			entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;

			var usages = entryInstruction.CusAuthorizationUsages;
			CombineAssertions("Default Authorization", () =>
			{
				AssertEquals(1, usages.Count);
				var usage = usages[0];
				AssertEquals(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, usage.AGC_Code);
				AssertEquals("001", usage.AGC_Number);
				AssertEquals(owner.PK, usage.AGC_OH_Owner);
			});
		}

		public void TestToWarehouseTypeAndToWarehouseCode()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseAddress = warehouse.MainAddress;
			var owner = Factory.NewWithValidTestData<OrgHeader>();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;
			authorizationHeader.CPH_Number = "001";
			authorizationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
			entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			cusAuthorizationUsage.AGC_Number = "001";
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;

			CombineAssertions("When does not have CCP", () =>
			{
				AssertEquals("FromWarehouseType", WarehouseTypeList.Codes.CustomsWarehousingCW1, entryInstruction.ToWarehouseType);
				AssertEquals("FromWarehouseCode", ZString.Empty, entryInstruction.ToWarehouseCode);
			});

			warehouseAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U002", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			entryInstruction.Factory.ClearCachedValue<CusAuthorisationHeader[]>($"GetAuthorisationHeaders|IE|{ZDateTime.Today.ToISO8601ShortDateString()}|{authorizationHeader.CPH_OA_AppliesTo.ToStringKey()}|CW1_CW2_CWP_TST");

			CombineAssertions("When has CCP", () =>
			{
				AssertEquals("FromWarehouseType", WarehouseTypeList.Codes.CustomsWarehousingCW1, entryInstruction.ToWarehouseType);
				AssertEquals("FromWarehouseCode", "U002", entryInstruction.ToWarehouseCode);
			});
		}

		public void TestIsExitSummary()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertEquals("IsExitSummary returns false when no Declaration linked.", false, instruction.IsExitSummary);

			var declaration = Factory.New<JobDeclaration>();
			instruction.CEI_JE = declaration.PK;
			AssertEquals("IsExitSummary returns false with empty JE_MessageType.", false, instruction.IsExitSummary);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("IsExitSummary returns true with EXS.", true, instruction.IsExitSummary);
		}

		public void TestIsAir()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("IsAir returns false when JE_TransportMode is Sea.", false, instruction.IsAir);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("IsAir returns true when JE_TransportMode is Air.", true, instruction.IsAir);
		}

		public void TestFromWarehouseTypeAndFromWarehouseCode()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseAddress = warehouse.MainAddress;
			var owner = Factory.NewWithValidTestData<OrgHeader>();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;
			authorizationHeader.CPH_Number = "001";
			authorizationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
			entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			cusAuthorizationUsage.AGC_Number = "001";
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;

			CombineAssertions("When does not have CCP", () =>
			{
				AssertEquals("FromWarehouseType", WarehouseTypeList.Codes.CustomsWarehousingCW2, entryInstruction.FromWarehouseType);
				AssertEquals("FromWarehouseCode", ZString.Empty, entryInstruction.FromWarehouseCode);
			});

			warehouseAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U002", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			entryInstruction.Factory.ClearCachedValue<CusAuthorisationHeader[]>($"GetAuthorisationHeaders|IE|{ZDateTime.Today.ToISO8601ShortDateString()}|{authorizationHeader.CPH_OA_AppliesTo.ToStringKey()}|CW1_CW2_CWP_TST");

			CombineAssertions("When has CCP", () =>
			{
				AssertEquals("FromWarehouseType", WarehouseTypeList.Codes.CustomsWarehousingCW2, entryInstruction.FromWarehouseType);
				AssertEquals("FromWarehouseCode", "U002", entryInstruction.FromWarehouseCode);
			});
		}

		public void TestTriggerRefreshWarehouseDataWhenDefaultAuthorization()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var isTriggered = false;
			instruction.FromWarehouseTypeInfo.ValueChanged += (sender, e) =>
			{
				isTriggered = true;
			};
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			instruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			AssertEquals("Triggered when CEI_OA_Warehouse2 changed", true, isTriggered);

			isTriggered = false;
			instruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			AssertEquals("Triggered when CEI_OA_Warehouse changed", true, isTriggered);

			isTriggered = false;
			instruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			AssertEquals("Not triggered when value not change", false, isTriggered);

			isTriggered = false;
			instruction.CEI_OA_Warehouse = ZGuid.Empty;
			AssertEquals("Not triggered when value is invalid", false, isTriggered);
		}

		public void TestAuthorizationUsagesCached()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;
			authorizationHeader.CPH_Number = "001";
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OA_AppliesTo = warehouse.MainAddress.PK;

			var authorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			authorizationUsage.AGC_OH_Owner = owner.PK;

			instruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			instruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

			var fromWarehouseAuthorizationUsages = instruction.FromWarehouseAuthorizationUsages;
			var toWarehouseAuthorizationUsages = instruction.ToWarehouseAuthorizationUsages;
			AssertSame("FromWarehouseAuthorizationUsages cached", fromWarehouseAuthorizationUsages, instruction.FromWarehouseAuthorizationUsages);
			AssertSame("ToWarehouseAuthorizationUsages cached", toWarehouseAuthorizationUsages, instruction.ToWarehouseAuthorizationUsages);

			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			Assert("FromWarehouseAuthorizationUsages recalculated", fromWarehouseAuthorizationUsages != instruction.FromWarehouseAuthorizationUsages);
			Assert("ToWarehouseAuthorizationUsages recalculated", toWarehouseAuthorizationUsages != instruction.ToWarehouseAuthorizationUsages);
		}

		public void TestGetAuthorizationUsages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			authorizationUsage.AGC_OH_Owner = owner.PK;

			instruction.CEI_OA_Warehouse = owner.MainAddress.PK;

			CombineAssertions("Warehouse OrgPK equals to Usage Owner", () =>
			{
				AssertEquals(1, instruction.FromWarehouseAuthorizationUsages.Count);
				AssertEquals(authorizationUsage.PK, instruction.FromWarehouseAuthorizationUsages[0].PK);
			});

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OH_PermitHolder = permitHolder.PK;
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			authorizationHeader.CPH_OA_AppliesTo = warehouse.MainAddress.PK;

			instruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

			CombineAssertions("Permit Holder equals to Usage Owner", () =>
			{
				AssertEquals(1, instruction.ToWarehouseAuthorizationUsages.Count);
				AssertEquals(authorizationHeader.CPH_OH_PermitHolder, instruction.ToWarehouseAuthorizationUsages[0].AGC_OH_Owner);
			});
		}

		public void TestHasAuthorisationForSpecialProcedure()
		{
			CombineAssertions(() =>
			{
				AssertEquals("HasAuthorisationForSpecialProcedure - No AdditionalInfos", false, instruction.HasAuthorisationForSpecialProcedure);

				var additionalInfo = instruction.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

				var transportDoc = instruction.AdditionalInfos.AddNew();
				transportDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

				var additionalRef = instruction.AdditionalInfos.AddNew();
				additionalRef.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

				additionalRef.CSI_Code = "00100";
				AssertEquals("HasAuthorisationForSpecialProcedure - AdditionalReference is invalid", false, instruction.HasAuthorisationForSpecialProcedure);

				transportDoc.CSI_Code = "00100";
				AssertEquals("HasAuthorisationForSpecialProcedure - TransportDocument is invalid", false, instruction.HasAuthorisationForSpecialProcedure);

				additionalInfo.CSI_Code = "00100";
				AssertEquals("HasAuthorisationForSpecialProcedure - AdditionalInformation is valid", true, instruction.HasAuthorisationForSpecialProcedure);
			});
		}

		public void TestHasSupportingDocumentForExportWithInvoiceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - No supporting Docs", false, instruction.HasSupportingDocumentForExportWithInvoiceNumber);
				var supportingDoc1 = instruction.SupportingDocuments.AddNew();
				supportingDoc1.CSI_Code = "XXXX";
				supportingDoc1.CSI_ReferenceNumber = string.Empty;
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - 1 invalid supporting doc", false, instruction.HasSupportingDocumentForExportWithInvoiceNumber);
				supportingDoc1.CSI_Code = Constants.SupportingDocumentCodes._N325;
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - 1 invalid supporting doc", false, instruction.HasSupportingDocumentForExportWithInvoiceNumber);
				supportingDoc1.CSI_ReferenceNumber = "REFNO1";
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - 1 valid supporting doc", true, instruction.HasSupportingDocumentForExportWithInvoiceNumber);
				supportingDoc1.CSI_Code = "XXXX";
				var supportingDoc2 = instruction.SupportingDocuments.AddNew();
				supportingDoc2.CSI_Code = Constants.SupportingDocumentCodes._D005;
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - 2 invalid supporting docs", false, instruction.HasSupportingDocumentForExportWithInvoiceNumber);
				supportingDoc2.CSI_ReferenceNumber = "REFNO2";
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - 1 valid, 1 invalid supporting docs", true, instruction.HasSupportingDocumentForExportWithInvoiceNumber);
			});
		}

		public void TestHasExportInvoiceHeaderWithSupportingDocumentHavingInvoiceNumber()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - No supporting Docs", false, instruction.HasExportInvoiceHeaderWithSupportingDocumentHavingInvoiceNumber);
				var supportingDoc1 = invoice.SupportingDocuments.AddNew();
				supportingDoc1.CSI_Code = "XXXX";
				supportingDoc1.CSI_ReferenceNumber = string.Empty;
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - 1 invalid supporting doc", false, instruction.HasExportInvoiceHeaderWithSupportingDocumentHavingInvoiceNumber);
				supportingDoc1.CSI_Code = Constants.SupportingDocumentCodes._N325;
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - 1 invalid supporting doc", false, instruction.HasExportInvoiceHeaderWithSupportingDocumentHavingInvoiceNumber);
				supportingDoc1.CSI_ReferenceNumber = "REFNO1";
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - 1 valid supporting doc", true, instruction.HasExportInvoiceHeaderWithSupportingDocumentHavingInvoiceNumber);
				supportingDoc1.CSI_Code = "XXXX";
				var supportingDoc2 = invoice.SupportingDocuments.AddNew();
				supportingDoc2.CSI_Code = Constants.SupportingDocumentCodes._D005;
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - 2 invalid supporting docs", false, instruction.HasExportInvoiceHeaderWithSupportingDocumentHavingInvoiceNumber);
				supportingDoc2.CSI_ReferenceNumber = "REFNO2";
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - 1 valid, 1 invalid supporting docs", true, instruction.HasExportInvoiceHeaderWithSupportingDocumentHavingInvoiceNumber);
			});
		}

		public void TestHasEstimatedTimeOfDepartureAdditionalInfo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - No additional infos", false, instruction.HasEstimatedTimeOfDepartureAdditionalInfo);
				var additionalInfo1 = instruction.AdditionalInfos.AddNew();
				additionalInfo1.CSI_Code = "XXXX";
				additionalInfo1.CSI_ReferenceNumber = "202210101000";
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - 1 invalid additional infos", false, instruction.HasEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo1.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - 1 valid supporting doc", true, instruction.HasEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo1.CSI_Code = "XXXX";
				var additionalInfo2 = instruction.AdditionalInfos.AddNew();
				additionalInfo2.CSI_Code = "YYYY";
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - 2 invalid additional infos", false, instruction.HasEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo2.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
				additionalInfo2.CSI_ReferenceNumber = "202210101000";
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - 1 valid, 1 invalid additional infos", true, instruction.HasEstimatedTimeOfDepartureAdditionalInfo);
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - 1 valid, 1 invalid additional infos, CEI_Substyle not applicable", true, instruction.HasEstimatedTimeOfDepartureAdditionalInfo);
			});
		}

		public void TestHasInvoiceHeaderWithEstimatedTimeOfDepartureAdditionalInfo()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - No additional infos", false, instruction.HasInvoiceHeaderWithEstimatedTimeOfDepartureAdditionalInfo);
				var additionalInfo1 = invoice.AdditionalInfos.AddNew();
				additionalInfo1.CSI_Code = "XXXX";
				additionalInfo1.CSI_ReferenceNumber = "202210101000";
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - 1 invalid additional infos", false, instruction.HasInvoiceHeaderWithEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo1.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - 1 valid additional infos", true, instruction.HasInvoiceHeaderWithEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo1.CSI_Code = "XXXX";
				var additionalInfo2 = invoice.AdditionalInfos.AddNew();
				additionalInfo2.CSI_Code = "YYYY";
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - 2 invalid additional infos", false, instruction.HasInvoiceHeaderWithEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo2.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
				additionalInfo2.CSI_ReferenceNumber = "202210101000";
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - 1 valid, 1 invalid additional infos", true, instruction.HasInvoiceHeaderWithEstimatedTimeOfDepartureAdditionalInfo);
			});
		}

		public void TestHasTransportDocument()
		{
			CombineAssertions(() =>
			{
				var addInfo2 = instruction.AdditionalInfos.AddNew();
				addInfo2.CSI_SubType = "YYY";
				addInfo2.CSI_Description = "YYY Description";

				AssertEquals("HasTransportDocument - does not have TransportDocument", false, instruction.HasTransportDocument);

				var addInfo3 = instruction.AdditionalInfos.AddNew();
				addInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfo3.CSI_Description = "TRA Description";

				AssertEquals("HasTransportDocument - has TransportDocument", true, instruction.HasTransportDocument);
			});
		}

		public void TestHasInvoiceHeaderWithTransportDocument()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			CombineAssertions(() =>
			{
				var invHeaderAddInfo = invoiceHeader.AdditionalInfos.AddNew();
				invHeaderAddInfo.CSI_SubType = "YYY";
				invHeaderAddInfo.CSI_Description = "YYY Description";

				AssertEquals("HasInvoiceHeaderWithTransportDocument - does not have TransportDocument", false, instruction.HasInvoiceHeaderWithTransportDocument);

				invHeaderAddInfo = invoiceHeader.AdditionalInfos.AddNew();
				invHeaderAddInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				invHeaderAddInfo.CSI_Description = "TRA Description";

				AssertEquals("HasInvoiceHeaderWithTransportDocument - has TransportDocument", true, instruction.HasInvoiceHeaderWithTransportDocument);
			});
		}

		public void TestGetCusSupportingInfoTypes_PreviousDocument()
		{
			AssertEquals(typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)instruction).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		}

		public void TestGetCusSupportingInfoTypes_SupportingDocument()
		{
			AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)instruction).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		}

		public void TestGetCusSupportingInfoTypes_AdditionalInfo()
		{
			AssertEquals(typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)instruction).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestPreviousDocuments()
		{
			AssertType<PreviousDocumentCollection>(instruction.PreviousDocuments);
		}

		public void TestAdditionalInfos()
		{
			AssertType<AdditionalInfoCollection>(instruction.AdditionalInfos);
		}

		public void TestSupportingDocuments()
		{
			AssertType<SupportingDocumentCollection>(instruction.SupportingDocuments);
		}

		public void TestValidationModes()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			instruction.ValidationModesCalculator.RecalculateValidationModes();
			AssertEquals("NONE for instruction without an entry with MRN.", EU.Business.Declaration.ValidationModes.None, instruction.ValidationModes);
			entryHeader.MovementReferenceNumberSetter("MRN001");
			instruction.ValidationModesCalculator.RecalculateValidationModes();
			AssertEquals("NONE, Amendment for instruction with an entry with MRN.", (EU.Business.Declaration.ValidationModes)5, instruction.ValidationModes);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			instruction.ValidationModesCalculator.RecalculateValidationModes();
			AssertEquals("NONE for instruction in REX job.", EU.Business.Declaration.ValidationModes.None, instruction.ValidationModes);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			instruction.ValidationModesCalculator.RecalculateValidationModes();
			AssertEquals("NONE, Amendment for Import declaration instruction with an entry with MRN.", (EU.Business.Declaration.ValidationModes)5, instruction.ValidationModes);
		}

		public void TestIsAmendmentValidationMode()
		{
			instruction.ValidationModes = EU.Business.Declaration.ValidationModes.None;
			AssertEquals("False for ValidationModes.None", false, instruction.IsAmendmentValidationMode);
			instruction.ValidationModes = EU.Business.Declaration.ValidationModes.Amendment;
			AssertEquals("True for ValidationModes.ExportAmendment", true, instruction.IsAmendmentValidationMode);
			instruction.ValidationModes = EU.Business.Declaration.ValidationModes.None | EU.Business.Declaration.ValidationModes.Amendment;
			AssertEquals("True for None|ExportAmendment", true, instruction.IsAmendmentValidationMode);
		}

		public void TestGuarantees()
		{
			AssertType<GuaranteeForEntryInstructionCollection>(instruction.Guarantees);
		}

		public void TestHasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel()
		{
			AssertEquals("HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel: false by default.", false, instruction.HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel);

			instruction.AdditionalInfos.AddNew(AdditionalInformationCodes._00100, string.Empty).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel: true with INF 00100.", true, instruction.HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel);

			instruction.AdditionalInfos.RemoveAndDeleteAll();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AdditionalInfos.AddNew(AdditionalInformationCodes._00100, string.Empty).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			invoiceHeader.InvoiceLines.AddNew().JI_CEI = instruction.PK;
			AssertEquals("HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel: true with INF 00100 on JZ.", true, instruction.HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel);
		}

		public void TestHasAuthorisationInwardProcessingProcedureOnShipmentLevel()
		{
			AssertEquals("HasAuthorisationInwardProcessingProcedureOnShipmentLevel: false by default.", false, instruction.HasAuthorisationInwardProcessingProcedureOnShipmentLevel);

			instruction.SupportingDocuments.AddNew(SupportingDocumentCodes.AuthorisationInwardProcessingProcedure, string.Empty).CSI_Code = SupportingDocumentCodes.AuthorisationInwardProcessingProcedure;
			AssertEquals("HasAuthorisationInwardProcessingProcedureOnShipmentLevel: true with C601.", true, instruction.HasAuthorisationInwardProcessingProcedureOnShipmentLevel);

			instruction.SupportingDocuments.RemoveAndDeleteAll();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.SupportingDocuments.AddNew(SupportingDocumentCodes.AuthorisationInwardProcessingProcedure, string.Empty).CSI_Code = SupportingDocumentCodes.AuthorisationInwardProcessingProcedure;
			invoiceHeader.InvoiceLines.AddNew().JI_CEI = instruction.PK;
			AssertEquals("HasAuthorisationInwardProcessingProcedureOnShipmentLevel: true with C601.", true, instruction.HasAuthorisationInwardProcessingProcedureOnShipmentLevel);
		}

		public void TestHasBothC601SupportingDocumentAnd00100AdditionalInformation()
		{
			AssertEquals("HasBothC601SupportingDocumentAnd00100AdditionalInformation: false by default.", false, instruction.HasBothC601SupportingDocumentAnd00100AdditionalInformation);

			var supportingDocumentC601 = instruction.SupportingDocuments.AddNew(SupportingDocumentCodes.AuthorisationInwardProcessingProcedure, string.Empty);
			supportingDocumentC601.CSI_Code = SupportingDocumentCodes.AuthorisationInwardProcessingProcedure;
			AssertEquals("HasBothC601SupportingDocumentAnd00100AdditionalInformation: false when only C601.", false, instruction.HasBothC601SupportingDocumentAnd00100AdditionalInformation);

			instruction.AdditionalInfos.AddNew(AdditionalInformationCodes._00100, string.Empty).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("HasBothC601SupportingDocumentAnd00100AdditionalInformation: true when C601 & 00100 INF.", true, instruction.HasBothC601SupportingDocumentAnd00100AdditionalInformation);

			instruction.SupportingDocuments.RemoveAndDelete(supportingDocumentC601);
			AssertEquals("HasBothC601SupportingDocumentAnd00100AdditionalInformation: false when only 00100 INF.", false, instruction.HasBothC601SupportingDocumentAnd00100AdditionalInformation);
		}

		public void TestHasN018SupportingDocument()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var supDoc = instruction.SupportingDocuments.AddNew();
			CombineAssertions(() =>
			{
				supDoc.CSI_Code = "YYYY";
				AssertEquals(false, instruction.HasN018SupportingDocument);

				supDoc.CSI_Code = Constants.SupportingDocumentCodes._N018;
				AssertEquals(true, instruction.HasN018SupportingDocument);
			});
		}

		public void TestGetSupportingDocumentsAtAnyLevel()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			var doc1 = instruction.SupportingDocuments.AddNew();
			doc1.CSI_Code = "DOC1";

			var invoiceHeader = declaration.Invoices.AddNew();
			var doc2 = invoiceHeader.SupportingDocuments.AddNew();
			doc2.CSI_Code = "DOC2";

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var doc3 = invoiceLine.SupportingDocuments.AddNew();
			doc3.CSI_Code = "DOC3";

			var supportingDocs = instruction.SupportingDocumentsAtAnyLevel;
			AssertEquals(true, supportingDocs.Any(doc => doc.CSI_Code == "DOC1"));
			AssertEquals(true, supportingDocs.Any(doc => doc.CSI_Code == "DOC2"));
			AssertEquals(true, supportingDocs.Any(doc => doc.CSI_Code == "DOC3"));
		}

		public void TestGetTransportDocumentsAtAnyLevel()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			var doc1 = instruction.AdditionalInfos.AddNew();
			doc1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			doc1.CSI_Code = "DOC1";

			var invoiceHeader = declaration.Invoices.AddNew();
			var doc2 = invoiceHeader.AdditionalInfos.AddNew();
			doc2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			doc2.CSI_Code = "DOC2";

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var doc3 = invoiceLine.AdditionalInfos.AddNew();
			doc3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			doc3.CSI_Code = "DOC3";

			var transportDocs = instruction.AdditionalInfosAtAnyLevel;
			AssertEquals(true, transportDocs.Any(doc => doc.CSI_Code == "DOC1"));
			AssertEquals(true, transportDocs.Any(doc => doc.CSI_Code == "DOC2"));
			AssertEquals(true, transportDocs.Any(doc => doc.CSI_Code == "DOC3"));
		}

		public void TestBR8011KeyCounts()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Tariff = "0303001010";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine1.ZG_CountryOfSupply = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine1.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
			invoiceLine1.JI_ConcessionOrder = "100001";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Tariff = "0303001010";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine2.ZG_CountryOfSupply = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine2.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
			invoiceLine2.JI_ConcessionOrder = "100001";

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_Tariff = "0303001010";
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine3.ZG_CountryOfSupply = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine3.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
			invoiceLine3.JI_ConcessionOrder = "100003";

			AssertEquals(3, instruction.BR8011KeyCounts.Count);
			AssertEquals(1, instruction.BR8011KeyCounts["AU_0303001010_100001"]);
			AssertEquals(1, instruction.BR8011KeyCounts["US_0303001010_100001"]);
			AssertEquals(1, instruction.BR8011KeyCounts["AU_0303001010_100003"]);

			invoiceLine3.JI_ConcessionOrder = "100001";
			AssertEquals(2, instruction.BR8011KeyCounts.Count);
			AssertEquals(2, instruction.BR8011KeyCounts["AU_0303001010_100001"]);
			AssertEquals(1, instruction.BR8011KeyCounts["US_0303001010_100001"]);
		}

		public void TestDuplicatedEntryInstructionAndInvoiceC100SupportingDocReferences()
		{
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			var invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			var invoiceLine3 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction2.PK;
			var invoiceLine4 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = instruction.PK;
			var invoiceLine5 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = instruction2.PK;
			var invoiceLine6 = invoiceHeader3.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_CEI = instruction3.PK;
			var supDoc1 = instruction.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = SupportingDocumentCodes._C100;
			supDoc1.CSI_ReferenceNumber = "S1";
			var supDoc2 = instruction.SupportingDocuments.AddNew();
			supDoc2.CSI_Code = SupportingDocumentCodes._U059;
			supDoc2.CSI_ReferenceNumber = "S2";
			var supDoc3 = instruction.SupportingDocuments.AddNew();
			supDoc3.CSI_Code = SupportingDocumentCodes._C100;
			supDoc3.CSI_ReferenceNumber = "S3";
			var supDoc4 = instruction.SupportingDocuments.AddNew();
			supDoc4.CSI_Code = SupportingDocumentCodes._C100;
			supDoc4.CSI_ReferenceNumber = "S1";
			var supDoc5 = invoiceHeader1.SupportingDocuments.AddNew();
			supDoc5.CSI_Code = SupportingDocumentCodes._C100;
			supDoc5.CSI_ReferenceNumber = "S4";
			var supDoc6 = invoiceHeader1.SupportingDocuments.AddNew();
			supDoc6.CSI_Code = SupportingDocumentCodes._C100;
			supDoc6.CSI_ReferenceNumber = "S5";
			var supDoc7 = invoiceHeader1.SupportingDocuments.AddNew();
			supDoc7.CSI_Code = SupportingDocumentCodes._U111;
			supDoc7.CSI_ReferenceNumber = "S6";
			var supDoc8 = invoiceHeader2.SupportingDocuments.AddNew();
			supDoc8.CSI_Code = SupportingDocumentCodes._C100;
			supDoc8.CSI_ReferenceNumber = "S1";
			var supDoc9 = invoiceHeader2.SupportingDocuments.AddNew();
			supDoc9.CSI_Code = SupportingDocumentCodes._C100;
			supDoc9.CSI_ReferenceNumber = "S7";
			var supDoc10 = invoiceHeader2.SupportingDocuments.AddNew();
			supDoc10.CSI_Code = SupportingDocumentCodes._C100;
			supDoc10.CSI_ReferenceNumber = "S5";
			var supDoc11 = invoiceLine1.SupportingDocuments.AddNew();
			supDoc11.CSI_Code = SupportingDocumentCodes._C100;
			supDoc11.CSI_ReferenceNumber = "S4";
			var supDoc12 = instruction2.SupportingDocuments.AddNew();
			supDoc12.CSI_Code = SupportingDocumentCodes._C100;
			supDoc12.CSI_ReferenceNumber = "S5";
			var supDoc13 = invoiceHeader3.SupportingDocuments.AddNew();
			supDoc13.CSI_Code = SupportingDocumentCodes._C100;
			supDoc13.CSI_ReferenceNumber = "S8";
			var supDoc14 = invoiceHeader3.SupportingDocuments.AddNew();
			supDoc14.CSI_Code = SupportingDocumentCodes._C100;
			supDoc14.CSI_ReferenceNumber = "S8";
			var supDoc15 = invoiceHeader3.SupportingDocuments.AddNew();
			supDoc15.CSI_Code = SupportingDocumentCodes._C100;
			supDoc15.CSI_ReferenceNumber = "S9";
			var supDoc16 = invoiceHeader3.SupportingDocuments.AddNew();
			supDoc16.CSI_Code = SupportingDocumentCodes._U059;
			supDoc16.CSI_ReferenceNumber = "S9";

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("instruction", new[] { "S1", "S5" }, instruction.DuplicatedEntryInstructionAndInvoiceC100SupportingDocReferences);
				AssertContainsExactElementsInAnyOrder("instruction2", new[] { "S5" }, instruction2.DuplicatedEntryInstructionAndInvoiceC100SupportingDocReferences);
				AssertContainsExactElementsInAnyOrder("instruction3", new[] { "S8" }, instruction3.DuplicatedEntryInstructionAndInvoiceC100SupportingDocReferences);

				var supDoc17 = invoiceHeader3.SupportingDocuments.AddNew();
				supDoc17.CSI_Code = SupportingDocumentCodes._C100;
				supDoc17.CSI_ReferenceNumber = "S9";
				AssertContainsExactElementsInAnyOrder("instruction3", new[] { "S8", "S9" }, instruction3.DuplicatedEntryInstructionAndInvoiceC100SupportingDocReferences);
			});
		}

		public void TestDuplicatedInvoiceLineC100SupportingDocReferences()
		{
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			var supDoc1 = invoiceLine1.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = SupportingDocumentCodes._C100;
			supDoc1.CSI_ReferenceNumber = "S1";
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			var supDoc2 = invoiceLine2.SupportingDocuments.AddNew();
			supDoc2.CSI_Code = SupportingDocumentCodes._C100;
			supDoc2.CSI_ReferenceNumber = "S1";
			var supDoc3 = invoiceLine2.SupportingDocuments.AddNew();
			supDoc3.CSI_Code = SupportingDocumentCodes._C100;
			supDoc3.CSI_ReferenceNumber = "S2";
			var supDoc4 = invoiceLine2.SupportingDocuments.AddNew();
			supDoc4.CSI_Code = SupportingDocumentCodes._C990;
			supDoc4.CSI_ReferenceNumber = "S2";

			var invoiceLine3 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction2.PK;
			var supDoc5 = invoiceLine3.SupportingDocuments.AddNew();
			supDoc5.CSI_Code = SupportingDocumentCodes._C100;
			supDoc5.CSI_ReferenceNumber = "S2";
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("instruction", new[] { "S1" }, instruction.DuplicatedInvoiceLineC100SupportingDocReferences);
				Assert("instruction2.DuplicatedEntryIntructionAndInvoiceC100SupportingDocReferences is empty.", !instruction2.DuplicatedEntryInstructionAndInvoiceC100SupportingDocReferences.Any());

				var supDoc6 = invoiceLine2.SupportingDocuments.AddNew();
				supDoc6.CSI_Code = SupportingDocumentCodes._C100;
				supDoc6.CSI_ReferenceNumber = "S2";
				AssertContainsExactElementsInAnyOrder("instruction", new[] { "S1", "S2" }, instruction.DuplicatedInvoiceLineC100SupportingDocReferences);
				Assert("instruction2.DuplicatedEntryIntructionAndInvoiceC100SupportingDocReferences is empty.", !instruction2.DuplicatedEntryInstructionAndInvoiceC100SupportingDocReferences.Any());

				supDoc6.CSI_ReferenceNumber = ZString.Empty;
				var supDoc7 = invoiceLine3.SupportingDocuments.AddNew();
				supDoc7.CSI_Code = SupportingDocumentCodes._C100;
				supDoc7.CSI_ReferenceNumber = "S2";
				AssertContainsExactElementsInAnyOrder("instruction", new[] { "S1" }, instruction.DuplicatedInvoiceLineC100SupportingDocReferences);
				AssertContainsExactElementsInAnyOrder("instruction2", new[] { "S2" }, instruction2.DuplicatedInvoiceLineC100SupportingDocReferences);
			});
		}

		public void TestCusGoodsLocationProvider_ProviderKey()
		{
			AssertEquals("ProviderKey", "IEDECL", (instruction as ICusGoodsLocationProviderWithUCCVersion).ProviderKey);
		}

		public void TestCusGoodsLocationProvider_UCCVersionProperty()
		{
			var provider = instruction as ICusGoodsLocationProviderWithUCCVersion;
			AssertEquals("UCCVersionDependingOnPropertyInfo returns JE_ApplicationCodeInfo",
				declaration.JE_ApplicationCodeInfo,
				((ZWrappedPropertyInfo)provider.IsUCC5Info).InnerInfo
			);
		}

		public void TestCusGoodsLocationProvider_UCCVersions()
		{
			var provider = instruction as ICusGoodsLocationProviderWithUCCVersion;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			AssertEquals("IsUCC5 true for V1 declaration", true, provider.IsUCC5);
			AssertEquals("IsUCC6 false for V1 declaration", false, provider.IsUCC6);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			AssertEquals("IsUCC5 false for V2 declaration", false, provider.IsUCC5);
			AssertEquals("IsUCC6 true for V2 declaration", true, provider.IsUCC6);
		}

		public void TestIsUCC5AndIsImport()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("IsUCC5AndIsImport false for Export", false, instruction.IsUCC5AndIsImport);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			AssertEquals("IsUCC5AndIsImport true for Import V1", true, instruction.IsUCC5AndIsImport);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			AssertEquals("IsUCC5AndIsImport false for Import V2", false, instruction.IsUCC5AndIsImport);
		}

		public void TestIsPlacesOfUsageRequired()
		{
			AssertIsPlacesOfUsageRequired(ImportDeclarationTypeList.Codes.H1, ProcedureCodes.ProcedureCode._44);
			AssertIsPlacesOfUsageRequired(ImportDeclarationTypeList.Codes.H3, ProcedureCodes.ProcedureCode._53);
			AssertIsPlacesOfUsageRequired(ImportDeclarationTypeList.Codes.H4, ProcedureCodes.ProcedureCode._51);
		}

		public void TestCusAuthorizationUsagesExceptOldOwnerForChangeOfOwner()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.NewWithValidTestData<CusEntryInstructionForTest>();
			declaration.CustomsEntryInstructions.Add(instruction);

			var oldOwner = Factory.NewWithValidTestData<OrgHeader>();
			var newOwner = Factory.NewWithValidTestData<OrgHeader>();
			var anotherOwner = Factory.NewWithValidTestData<OrgHeader>();
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();

			var usageIPO1 = instruction.CusAuthorizationUsages.AddNew();
			usageIPO1.AGC_OH_Owner = oldOwner.PK;
			usageIPO1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			usageIPO1.AGC_Number = "IPO1 OLD";
			var usageIPO2 = instruction.CusAuthorizationUsages.AddNew();
			usageIPO2.AGC_OH_Owner = newOwner.PK;
			usageIPO2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			usageIPO2.AGC_Number = "IPO2 NEW";
			var usageIPO3 = instruction.CusAuthorizationUsages.AddNew();

			instruction.SetPropertiesForTest(isIntoWarehouse: false, isOutOfWarehouse: false, isIntoTemporaryExportProcedure: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false);
			instruction.CEI_OH_Owner = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("pre-req", false, instruction.HasAnyChangeOfOwnershipProcedure);
			AssertEquals("pre-req", ZGuid.Empty, instruction.CEI_OH_Owner);
			AssertEquals("pre-req", null, instruction.OldOwner);
			AssertEquals("pre-req", 3, instruction.CusAuthorizationUsages.Count);

			var usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			var usageList = usages.ToList();
			AssertEquals("Return all usages - no HasAnyChangeOfOwnershipProcedure, no owner, no old owner", 3, usageList.Count);
			instruction.CEI_OH_Owner = oldOwner.PK;
			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			AssertEquals("Return all usages - no HasAnyChangeOfOwnershipProcedure, owner, no old owner", 3, usageList.Count);
			declaration.JE_OH_Importer = oldOwner.PK;
			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			AssertEquals("Return all usages - no HasAnyChangeOfOwnershipProcedure, owner, old owner", 3, usageList.Count);

			instruction.SetPropertiesForTest(isIntoWarehouse: false, isOutOfWarehouse: false, isIntoTemporaryExportProcedure: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: true, isOutOfInwardProcessing: true
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false);
			instruction.CEI_OH_Owner = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("pre-req", true, instruction.HasAnyChangeOfOwnershipProcedure);
			AssertEquals("pre-req", ZGuid.Empty, instruction.CEI_OH_Owner);
			AssertEquals("pre-req", null, instruction.OldOwner);

			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			AssertEquals("Return all usages - HasAnyChangeOfOwnershipProcedure, no owner, no old owner", 3, usageList.Count);
			instruction.CEI_OH_Owner = oldOwner.PK;
			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			AssertEquals("Return all usages - HasAnyChangeOfOwnershipProcedure, owner, no old owner", 3, usageList.Count);
			declaration.JE_OH_Importer = oldOwner.PK;
			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			CombineAssertions("Return usages that don't belong to old owner", () =>
			{
				AssertEquals("Has HasAnyChangeOfOwnershipProcedure, has owner, has old owner", 2, usageList.Count);
				AssertEquals(usageIPO2.PK, usageList[0].PK);
				AssertEquals(usageIPO3.PK, usageList[1].PK);
			});

			declaration.JE_OH_Importer = anotherOwner.PK;
			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			CombineAssertions("Return all usages - old owner not found", () =>
			{
				AssertEquals(3, usageList.Count);
			});
		}

		public void TestHasAnyChangeOfOwnershipProcedure()
		{
			declaration.CustomsEntryInstructions.RemoveAll();
			var instructionForTest = Factory.NewWithValidTestData<CusEntryInstructionForTest>();
			declaration.CustomsEntryInstructions.Add(instructionForTest);

			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into warehousing and not out of warehousing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: true, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When into warehousing and not out of warehousing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: true
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into warehousing and out of warehousing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: true, isOutOfWarehouse: true
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: true
				, assertionMessage: "When into warehousing and out of warehousing HasAnyChangeOfOwnershipProcedure should be true");

			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into outward processing and not out of outward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: true, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When into outward processing and not out of outward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: true
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into outward processing and out of outward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: true, isOutOfOutwardProcessing: true
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: true
				, assertionMessage: "When into outward processing and into out of outward processing HasAnyChangeOfOwnershipProcedure should be true");

			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into inward processing and not out of inward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: true, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into inward processing and not out of inward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: true
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into inward processing and not out of inward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: true, isOutOfInwardProcessing: true
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: true
				, assertionMessage: "When into inward processing and out of inward processing HasAnyChangeOfOwnershipProcedure should be true");

			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into temporary import procedure and not out of temporary import procedure HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: true, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When into temporary import procedure and not out of temporary import procedure HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: true
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into temporary import procedure and out of temporary import procedure HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: true, isOutOfTemporaryImportProcedure: true
				, assertionExpected: true
				, assertionMessage: "When into temporary import procedure and out of temporary import procedure HasAnyChangeOfOwnershipProcedure should be true");
		}

		void SetupAndAssertHasAnyChangeOfOwnershipProcedure(CusEntryInstructionForTest instructionForTest
		, bool isIntoWarehouse, bool isOutOfWarehouse
		, bool isIntoOutwardProcessing, bool isOutOfOutwardProcessing
		, bool isIntoInwardProcessing, bool isOutOfInwardProcessing
		, bool isIntoTemporaryImportProcedure, bool isOutOfTemporaryImportProcedure
		, ZBool assertionExpected
		, string assertionMessage)
		{
			instructionForTest.SetPropertiesForTest(isIntoTemporaryImportProcedure, isOutOfTemporaryImportProcedure
				, isIntoTemporaryExportProcedure: false
				, isIntoWarehouse, isOutOfWarehouse
				, isIntoOutwardProcessing, isOutOfOutwardProcessing
				, isIntoInwardProcessing, isOutOfInwardProcessing);

			AssertEquals("pre-req HasIntoTemporaryImportProcedure", isIntoTemporaryImportProcedure, instructionForTest.HasIntoTemporaryImportProcedure);
			AssertEquals("pre-req HasOutOfTemporaryImportProcedure", isOutOfTemporaryImportProcedure, instructionForTest.HasOutOfTemporaryImportProcedure);
			AssertEquals("pre-req HasIntoWarehouseProcedure", isIntoWarehouse, instructionForTest.HasIntoWarehouseProcedure);
			AssertEquals("pre-req HasOutOfWarehouseProcedure", isOutOfWarehouse, instructionForTest.HasOutOfWarehouseProcedure);
			AssertEquals("pre-req HasIntoOutwardProcessingProcedure", isIntoOutwardProcessing, instructionForTest.HasIntoOutwardProcessingProcedure);
			AssertEquals("pre-req HasOutOfOutwardProcessingProcedure", isOutOfOutwardProcessing, instructionForTest.HasOutOfOutwardProcessingProcedure);
			AssertEquals("pre-req HasIntoInwardProcessingProcedure", isIntoInwardProcessing, instructionForTest.HasIntoInwardProcessingProcedure);
			AssertEquals("pre-req HasOutOfInwardProcessingProcedure", isOutOfInwardProcessing, instructionForTest.HasOutOfInwardProcessingProcedure);

			AssertEquals(assertionMessage, assertionExpected, instructionForTest.HasAnyChangeOfOwnershipProcedure);
		}

		void AssertIsPlacesOfUsageRequired(string style, string procedure)
		{
			instruction.CEI_Style = style;
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = procedure;

			var additionalInfo = invoiceLine.AdditionalInfos.FirstOrAddNew<AdditionalInfo>();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = AdditionalInformationCodes._00100;

			CombineAssertions("IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired", () =>
			{
				AssertEquals($"Case1 CEI_Style {instruction.CEI_Style}, JI_Procedure {invoiceLine.JI_Procedure}, CSI_Code {additionalInfo.CSI_Code}, should return true.", true, instruction.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired);

				additionalInfo.CSI_Code = AdditionalReferenceCodes.RoRoShipID;
				AssertEquals($"Case2 CEI_Style {instruction.CEI_Style}, JI_Procedure {invoiceLine.JI_Procedure}, CSI_Code {additionalInfo.CSI_Code}, should return false.", false, instruction.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired);
				additionalInfo.CSI_Code = AdditionalInformationCodes._00100;

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals($"Case3 CEI_Style {instruction.CEI_Style}, JI_Procedure {invoiceLine.JI_Procedure}, CSI_SubType {additionalInfo.CSI_SubType}, should return false.", false, instruction.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired);
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
				AssertEquals($"Case4 CEI_Style {ImportDeclarationTypeList.Codes.H2}, JI_Procedure {invoiceLine.JI_Procedure}, should return false.", false, instruction.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired);
				instruction.CEI_Style = style;

				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._21;
				AssertEquals($"Case5 CEI_Style {instruction.CEI_Style}, JI_Procedure {invoiceLine.JI_Procedure}, should return false.", false, instruction.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired);
				invoiceLine.JI_Procedure = procedure;
				AssertEquals($"Case6 CEI_Style {instruction.CEI_Style}, JI_Procedure {invoiceLine.JI_Procedure}, should return true.", true, instruction.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired);
			});
		}

		public void TestFiscalReferencesType()
		{
			AssertType<CusFiscalReference>(instruction.FiscalReferences.AddNew());
		}

		protected override BusinessObject GetNewBusinessObject() => instruction;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructions.AddNew();
		}
		JobDeclaration declaration;
		CusEntryInstruction instruction;
	}

	class CusEntryInstructionForTest : CusEntryInstruction
	{
		public CusEntryInstructionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void SetPropertiesForTest(bool isOutOfWarehouse = false, bool isOutOfInwardProcessing = false, bool isOutOfOutwardProcessing = false, bool isOutOfTemporaryImportProcedure = false)
		{
			this.isOutOfWarehouse = isOutOfWarehouse;
			this.isOutOfInwardProcessing = isOutOfInwardProcessing;
			this.isOutOfOutwardProcessing = isOutOfOutwardProcessing;
			this.isOutOfTemporaryImportProcedure = isOutOfTemporaryImportProcedure;
		}

		public void SetPropertiesForTest(bool isIntoTemporaryImportProcedure, bool isOutOfTemporaryImportProcedure
			, bool isIntoTemporaryExportProcedure
			, bool isIntoWarehouse, bool isOutOfWarehouse
			, bool isIntoOutwardProcessing, bool isOutOfOutwardProcessing
			, bool isIntoInwardProcessing, bool isOutOfInwardProcessing)
		{
			this.isIntoTemporaryImportProcedure = isIntoTemporaryImportProcedure;
			this.isOutOfTemporaryImportProcedure = isOutOfTemporaryImportProcedure;
			this.isIntoTemporaryExportProcedure = isIntoTemporaryExportProcedure;
			this.isIntoWarehouse = isIntoWarehouse;
			this.isOutOfWarehouse = isOutOfWarehouse;
			this.isIntoOutwardProcessing = isIntoOutwardProcessing;
			this.isOutOfOutwardProcessing = isOutOfOutwardProcessing;
			this.isIntoInwardProcessing = isIntoInwardProcessing;
			this.isOutOfInwardProcessing = isOutOfInwardProcessing;
		}

		protected override bool HasIntoTemporaryImportProcedureCore => isIntoTemporaryImportProcedure;
		protected override bool HasOutOfTemporaryImportProcedureCore => isOutOfTemporaryImportProcedure;
		protected override bool HasIntoTemporaryExportProcedureCore => isIntoTemporaryExportProcedure;
		protected override bool HasIntoWarehouseProcedureCore => isIntoWarehouse;
		protected override bool HasOutOfWarehouseProcedureCore => isOutOfWarehouse;
		protected override bool HasIntoOutwardProcessingProcedureCore => isIntoOutwardProcessing;
		protected override bool HasOutOfOutwardProcessingProcedureCore => isOutOfOutwardProcessing;
		protected override bool HasIntoInwardProcessingProcedureCore => isIntoInwardProcessing;
		protected override bool HasOutOfInwardProcessingProcedureCore => isOutOfInwardProcessing;

		bool isIntoTemporaryImportProcedure;
		bool isOutOfTemporaryImportProcedure;
		bool isIntoTemporaryExportProcedure;
		bool isIntoWarehouse;
		bool isOutOfWarehouse;
		bool isIntoOutwardProcessing;
		bool isOutOfOutwardProcessing;
		bool isIntoInwardProcessing;
		bool isOutOfInwardProcessing;
	}
}
