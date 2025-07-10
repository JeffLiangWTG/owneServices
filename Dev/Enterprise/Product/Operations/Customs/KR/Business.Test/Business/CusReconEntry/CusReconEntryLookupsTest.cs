using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconEntryLookups))]
	sealed class CusReconEntryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntry()
		{
			var parent = Factory.New<CusReconEntry>();
			AssertEquals(parent.Lookups.Parent, parent);
		}

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
			entry2.CH_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNum2.CE_EntryNum = "1234525000002M";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_EntryReleaseDate = new ZDateTime(2025, 01, 01);
			entry3.CH_EntryStatus = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
			var entryNum3 = entry3.EntryNumbers.AddNew();
			entryNum3.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNum3.CE_EntryNum = "1234525000003M";
			Factory.Save();

			var refundDeclaration = Factory.New<CusReconDeclaration>();
			var refundEntry = refundDeclaration.CusReconEntries.AddNew();
			var lookups = (CusReconEntryLookups)refundEntry.Lookups;

			AssertEquals("List count should be 1, but CH_EntryStatus is not checked yet.", 2, lookups.ImportEntryNumbers.Count);
		}

		public void TestCustomsBillsViewCollection()
		{
			var otherCompany = Factory.New<GlbCompany>();
			var otherBranch = otherCompany.Branches.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_GC = GlbCompany.CurrentCompany.PK;
			SetData("6N00225000001M", "030511900081001", StatementHeaderPaymentStatusList.Codes.PYC);
			SetData("6N00225000002M", "030511900081002", StatementHeaderPaymentStatusList.Codes.PYC);
			SetData("6N00225000003M", "030511900081003", StatementHeaderPaymentStatusList.Codes.PYI);
			Factory.Save();

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			reconDeclaration.CRD_GB_Branch = ZGuid.Empty;
			var reconEntry = reconDeclaration.CusReconEntries.AddNew();
			var lookups = (CusReconEntryLookups)reconEntry.Lookups;
			AssertEquals(0, lookups.CustomsBillsViewCollection.Count);

			reconDeclaration.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			AssertEquals(2, lookups.CustomsBillsViewCollection.Count);
			Assert(lookups.CustomsBillsViewCollection.Any(x => x.KEB_CustomsDisbursementBillNumber == "030511900081001"));
			Assert(lookups.CustomsBillsViewCollection.Any(x => x.KEB_CustomsDisbursementBillNumber == "030511900081002"));

			reconDeclaration.CRD_GB_Branch = otherBranch.PK;
			AssertEquals(0, lookups.CustomsBillsViewCollection.Count);

			void SetData(string importEntryNumber, string customsBillNumber, string paymentStatus)
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.EntryNumber = importEntryNumber;
				var entryLine = entry.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				var statementHeader = Factory.New<CusStatementHeader>();
				statementHeader.B2_StatementType = "I";
				statementHeader.B2_PaymentStatus = paymentStatus;
				var statementHeaderLine = statementHeader.StatementLines.AddNew();
				statementHeaderLine.B3_EntryNum = importEntryNumber;
				statementHeaderLine.B3_AssociatedEntry = customsBillNumber;
			}
		}

		public void TestCRE_Amendment5WNVersionNumber()
		{
			var refundDeclaration = Factory.New<CusReconDeclaration>();
			var refundEntry = refundDeclaration.CusReconEntries.AddNew();

			var lookups = (CusReconEntryLookups)refundEntry.Lookups;

			AssertEquals("The list count is 0. No refund RefundAmounts has been added yet.", 0, lookups.Amendment5WNVersionNumbers.Count);

			var fileReader = new TestFileReader(typeof(CusReconEntryLookupsTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "ImportEntryOrEntryLine_1.xml");
			var snapshot = refundEntry.CusReconSnapshots.AddNew();
			snapshot.CRS_SnapshotXml = messageText;

			AssertEquals("The list count is 2. No refund RefundAmounts has been added yet.", 2, lookups.Amendment5WNVersionNumbers.Count);
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
