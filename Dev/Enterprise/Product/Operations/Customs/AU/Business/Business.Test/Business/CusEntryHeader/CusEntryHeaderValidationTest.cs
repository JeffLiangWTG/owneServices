using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusEntryHeaderValidationTest : TestCaseWithFactory
	{
		public void TestValidateCH_BGMReferenceForMessageErrors()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "1";

			declaration.DoMerge();

			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one entry line", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			line.JI_IsPackToBondForLine = true;
			declaration.DoMerge();
			//Should not fire entry.RunPreSaveValidation manually. Merge process should take care of that.
			AssertEquals(false, entry.CH_BGMReferenceInfo.HasMessageErrors());
			AssertEquals(false, entry.CH_BGMReferenceInfo.HasErrors());
			AssertEquals(true, declaration.CustomsEntryHeaders.Contains(entry));

			entry.CH_HighestLineNumber = 1;
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entry.EntryNumber = "ABC";
			AssertEquals("HasBeenLodged", true, entry.HasBeenLodgedAtCustoms);
			AssertEquals(CusEntryHeader.NatureTypesForImportCMR.Nature20, entry.ZA_DetailsNotToBeAmended);

			line.JI_IsPackToBondForLine = false;
			declaration.DoMerge();
			//Should not fire entry.RunPreSaveValidation manually. Merge process should take care of that.
			AssertEquals(true, entry.CH_BGMReferenceInfo.HasMessageError(string.Format(CusEntryHeaderValidation.HasNonAmendableNatureChanges, "N20", "N10")));
		}

		public void TestValidateCH_BGMReferenceForErrors()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.JZ_ValuationDateOverride = ZDateTime.Today;
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LineNo = 1;
			line.JI_Tariff = "1";

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-1);
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "2";
			line2.JI_LineNo = 1;

			declaration.DoMerge();
			AssertEquals("two entries", 2, declaration.CustomsEntryHeaders.Count);

			line2.CusEntryLine.Header.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			line2.CusEntryLine.Header.EntryNumber = "ABC";
			AssertEquals("PreCondition:HasBeenLodged", false, line.CusEntryLine.Header.HasBeenLodgedAtCustoms);
			AssertEquals("PreCondition:HasBeenLodged", true, line2.CusEntryLine.Header.HasBeenLodgedAtCustoms);

			CusEntryHeader entry = line.CusEntryLine.Header;
			invoice.JZ_ValuationDateOverride = invoice2.JZ_ValuationDateOverride;
			declaration.DoMerge();//should validate the existing entry
			line2.CusEntryLine.Header.RunPreSaveValidation();
			AssertEquals("entry is deleted", true, entry.IsDeleted);
			AssertEquals("entry2 should have no errors", false, line2.CusEntryLine.Header.HasErrors);

			invoice.JZ_ValuationDateOverride = ZDateTime.Today;
			declaration.DoMerge();
			AssertEquals("two entries", 2, declaration.CustomsEntryHeaders.Count);

			entry = line.CusEntryLine.Header;
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entry.EntryNumber = "BBB";
			CusEntryHeader entry2 = line2.CusEntryLine.Header;
			AssertEquals("PreCondition:HasBeenLodged", true, line.CusEntryLine.Header.HasBeenLodgedAtCustoms);
			AssertEquals("PreCondition:HasBeenLodged", true, line2.CusEntryLine.Header.HasBeenLodgedAtCustoms);

			invoice.JZ_ValuationDateOverride = invoice2.JZ_ValuationDateOverride;
			declaration.DoMerge();//should validate the existing entry
			entry.RunPreSaveValidation();
			entry2.RunPreSaveValidation();
			AssertEquals("entry is not deleted", true, declaration.CustomsEntryHeaders.Contains(entry));
			AssertEquals("entry2 is not deleted", true, declaration.CustomsEntryHeaders.Contains(entry2));

			AssertNotEquals("one of the entries is deactivated", entry.IsActive, entry2.IsActive);

			AssertNotEquals("one of the entries should have an error to prevent a saving", entry.CH_BGMReferenceInfo.HasErrors(), entry2.CH_BGMReferenceInfo.HasErrors());
		}
	}
}
