using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconEntryLineLookups))]
	sealed class CusReconEntryLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLine()
		{
			var parent = Factory.New<CusReconEntryLine>();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		public void TestEntryLineNumbers()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryReleaseDate = new ZDateTime(2025, 01, 01);
			entry1.CH_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine1_1 = entry1.MergedLines.AddNew();
			entryLine1_1.CL_LineNumber = 2;
			var entryNum1 = entry1.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNum1.CE_EntryNum = "1234525000001M";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryReleaseDate = new ZDateTime(2025, 01, 01);
			entry2.CH_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;
			var entryLine2_1 = entry2.MergedLines.AddNew();
			entryLine2_1.CL_LineNumber = 3;
			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNum2.CE_EntryNum = "1234525000002M";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_EntryReleaseDate = new ZDateTime(2025, 01, 01);
			entry3.CH_EntryStatus = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
			var entryLine3 = entry3.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 4;
			var entryNum3 = entry3.EntryNumbers.AddNew();
			entryNum3.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNum3.CE_EntryNum = "1234525000003M";
			Factory.Save();

			var refundDeclaration = Factory.New<CusReconDeclaration>();
			var refundEntry = refundDeclaration.CusReconEntries.AddNew();
			var refundEntryLine = refundDeclaration.CusReconEntryLines.AddNew();
			refundEntryLine.CRL_CRE = refundEntry.PK;

			refundEntry.CRE_OriginalEntryNumber = "1234525000000M";
			var lookups = refundEntryLine.Lookups;
			AssertEquals(0, lookups.ImportEntryLineNumbers.Count);

			refundEntry.CRE_OriginalEntryNumber = entryNum1.CE_EntryNum;
			lookups = refundEntryLine.Lookups;
			AssertEquals("Entrylines of entry1", 2, lookups.ImportEntryLineNumbers.Count);
			Assert(lookups.ImportEntryLineNumbers.Any(x => x.KEL_LineNumber == 1));
			Assert(lookups.ImportEntryLineNumbers.Any(x => x.KEL_LineNumber == 2));

			refundEntry.CRE_OriginalEntryNumber = entryNum2.CE_EntryNum;
			lookups = refundEntryLine.Lookups;
			AssertEquals("Entrylines of entry3", 2, lookups.ImportEntryLineNumbers.Count);
			Assert(lookups.ImportEntryLineNumbers.Any(x => x.KEL_LineNumber == 1));
			Assert(lookups.ImportEntryLineNumbers.Any(x => x.KEL_LineNumber == 3));

			refundEntry.CRE_OriginalEntryNumber = entryNum3.CE_EntryNum;
			lookups = refundEntryLine.Lookups;
			AssertEquals("Entryline of entry3", 1, lookups.ImportEntryLineNumbers.Count);
			Assert(lookups.ImportEntryLineNumbers.Any(x => x.KEL_LineNumber == 4));
		}
	}
}
