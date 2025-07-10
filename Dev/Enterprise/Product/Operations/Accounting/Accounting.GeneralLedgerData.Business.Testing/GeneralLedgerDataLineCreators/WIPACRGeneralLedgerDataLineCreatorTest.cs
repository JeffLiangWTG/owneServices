using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class WIPACRGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public void TestCreateDRCREntries_WIPReversed()
		{
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE2.PK.ToGuid());

			var job = TestObjectCreator.InsertJobHeader(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			job.JH_JobNum = TestObjectCreator.GetRandomString(10);

			var wip = TestObjectCreator.CreateWIP(job);
			wip.AL_LineAmount = 20m;
			wip.AL_OSAmount = 20m;
			wip.Reverse();

			var creator = new WIPACRGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)wip).Row);

			var accruedRevenueControlAccount = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = wip.GLHeader.PK, LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = wip.AL_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = accruedRevenueControlAccount, LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.AccruedRevenueControlAccount, JournalDate = wip.AL_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = accruedRevenueControlAccount, LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.AccruedRevenueControlAccount, JournalDate = wip.AL_ReverseDate, GLDType = AccountingConstants.GLDTypeCodes.Recognition, Period = 0 },
				new DebitCreditEntryItem { AccountPK = wip.GLHeader.PK, LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = wip.AL_ReverseDate, GLDType = AccountingConstants.GLDTypeCodes.Recognition, Period = 0 }
			};

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		[TestDate(2023, 03, 01)]
		public void TestCreateDRCREntries_WIPOnlyPostDate()
		{
			var dateTime = ZDateTime.Today;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK, LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = dateTime, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK, LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.AccruedRevenueControlAccount, JournalDate = dateTime, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
			};

			AssertDRCRLines(false, true, expectedDRCRLines);
			AssertDRCRLines(false, false, expectedDRCRLines);
		}

		[TestDate(2023, 03, 01)]
		public void TestCreateDRCREntries_WIPOnlyReverseDate()
		{
			var dateTime = ZDateTime.Today;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK, LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.AccruedRevenueControlAccount, JournalDate = dateTime, GLDType = AccountingConstants.GLDTypeCodes.Recognition, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK, LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = dateTime, GLDType = AccountingConstants.GLDTypeCodes.Recognition, Period = 0 }
			};

			AssertDRCRLines(true, true, expectedDRCRLines);
			AssertDRCRLines(true, false, expectedDRCRLines);
		}

		void AssertDRCRLines(bool isReverse, bool isAdded, DebitCreditEntryItem[] expectedDRCRLines)
		{
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE2.PK.ToGuid());

			var job = TestObjectCreator.InsertJobHeader(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			job.JH_JobNum = TestObjectCreator.GetRandomString(10);

			var wip = TestObjectCreator.CreateWIP(job);
			wip.AL_LineAmount = 20m;
			wip.AL_OSAmount = 20m;

			if (isReverse)
			{
				wip.Reverse();
				wip.AL_PostDate = DateTime.MinValue;
			}

			var creator = new WIPACRGeneralLedgerDataLineCreator();
			var row = ((INeedRow)wip).Row;
			if (!isAdded)
			{
				row.AcceptChanges();
			}

			var entry = creator.CreateDRCREntries(((INeedRow)wip).Row);

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public override void TestHasValidControlAccount()
		{
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			var job = TestObjectCreator.InsertJobHeader(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			job.JH_JobNum = TestObjectCreator.GetRandomString(10);

			var wip = TestObjectCreator.CreateWIP(job);

			var creator = new WIPACRGeneralLedgerDataLineCreator();
			var entry = new DebitCreditEntry { EntryItems = Array.Empty<DebitCreditEntryItem>() };
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)wip).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)wip).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)wip).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AssertNoExceptionThrown(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)wip).Row);
			});
			Assert(entry.EntryItems.Any());
		}
	}
}
