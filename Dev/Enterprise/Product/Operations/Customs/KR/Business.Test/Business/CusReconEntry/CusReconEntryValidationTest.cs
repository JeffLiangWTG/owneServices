using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconEntryValidation))]
	sealed class CusReconEntryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryNumbers()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryReleaseDate = new ZDateTime(2025, 01, 01);
			entry1.CH_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryNum1 = entry1.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNum1.CE_EntryNum = "1234525000001M";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryReleaseDate = ZDateTime.Empty;
			entry2.CH_EntryStatus = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNum2.CE_EntryNum = "1234525000002M";
			Factory.Save();

			var refundDeclaration = Factory.New<CusReconDeclaration>();
			var refundEntry = refundDeclaration.CusReconEntries.AddNew();

			refundEntry.CRE_OriginalEntryNumber = ZString.Empty;
			AssertNoMessageErrors(refundEntry.CRE_OriginalEntryNumberInfo);

			refundEntry.CRE_OriginalEntryNumber = "XXXXXXX";
			AssertHasMessageErrorContaining(refundEntry.CRE_OriginalEntryNumberInfo, ListValidation.InvalidCodeMessageError);

			refundEntry.CRE_OriginalEntryNumber = "1234525000002M";
			AssertHasMessageErrorContaining(refundEntry.CRE_OriginalEntryNumberInfo, ListValidation.InvalidCodeMessageError);

			refundEntry.CRE_OriginalEntryNumber = "1234525000001M";
			AssertNoMessageErrors(refundEntry.CRE_OriginalEntryNumberInfo);
		}

		public void TestCRE_CustomsBillNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "6N00225000001M";
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_StatementType = "I";
			statementHeader.B2_PaymentStatus = "PYC";
			var statementHeaderLine = statementHeader.StatementLines.AddNew();
			statementHeaderLine.B3_EntryNum = "6N00225000001M";
			statementHeaderLine.B3_AssociatedEntry = "030511900081001";
			Factory.Save();

			var refundDeclaration = Factory.New<CusReconDeclaration>();
			var refundEntry1 = refundDeclaration.CusReconEntryLines.AddNew().Header;
			refundEntry1.CRE_CustomsBillNumber = ZString.Empty;
			AssertNoMessageErrors(refundEntry1.CRE_CustomsBillNumberInfo);

			refundEntry1.CRE_CustomsBillNumber = "XXXXXXXXXXXXXXX";
			AssertHasMessageErrorContaining(refundEntry1.CRE_CustomsBillNumberInfo, ListValidation.InvalidCodeMessageError);

			refundEntry1.CRE_CustomsBillNumber = "030511900081001";
			AssertNoMessageErrors(refundEntry1.CRE_CustomsBillNumberInfo);
		}

		public void TestCRE_Amendment5WNVersionNumber()
		{
			var refundDeclaration = Factory.New<CusReconDeclaration>();
			var refundEntry = (CusReconEntry)refundDeclaration.CusReconEntries.AddNew();

			var fileReader = new TestFileReader(typeof(CusReconEntryValidationTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "ImportEntryOrEntryLine_1.xml");
			var snapshot = refundEntry.CusReconSnapshots.AddNew();
			snapshot.CRS_SnapshotXml = messageText;

			refundEntry.Amendment5WNVersionNumber = "3";
			AssertHasMessageErrorContaining(refundEntry.Amendment5WNVersionNumberInfo, ListValidation.InvalidCodeMessageError);

			refundEntry.Amendment5WNVersionNumber = "1";
			AssertNoMessageErrors(refundEntry.Amendment5WNVersionNumberInfo);

			refundEntry.Amendment5WNVersionNumber = "";
			AssertNoMessageErrors(refundEntry.Amendment5WNVersionNumberInfo);
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
