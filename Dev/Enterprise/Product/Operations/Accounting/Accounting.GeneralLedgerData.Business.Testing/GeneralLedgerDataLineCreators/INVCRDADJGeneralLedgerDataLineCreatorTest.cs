using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class INVCRDADJGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public void TestCreateDRCREntries_ARINVPosted()
		{
			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV0001", TestObjectCreator.USD, 2m, 100m, 0m, 200m, 0m);
			aRInvoice.Lines[0].AL_PostDate = DateTime.Now;
			aRInvoice.Lines[0].AL_ReverseDate = DateTime.Now;
			aRInvoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			Factory.Save();

			var row = ((INeedRow)aRInvoice.Lines[0]).Row;
			row.SetAdded();

			GeneralLedgerDataRetriever.GetTransactionHeaderInfo(new ReadOnlyBusinessObjectFactory(), (Guid)row[AccTransactionLinesSchema.Constants.AL_AH]);

			var creator = new INVCRDADJGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)aRInvoice.Lines[0]).Row);

			var aRSuspenseControlAccount = AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Value;

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = GLControlAccounts.Instance.ARControlAccount.PK, LocalAmount = 200m, OSAmount = 400m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARControlAccount, JournalDate = aRInvoice.Lines[0].AL_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = aRSuspenseControlAccount, LocalAmount = -200m, OSAmount = -400m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = aRInvoice.Lines[0].AL_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = aRSuspenseControlAccount, LocalAmount = 200m, OSAmount = 400m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = aRInvoice.Lines[0].AL_ReverseDate, GLDType = AccountingConstants.GLDTypeCodes.Recognition, Period = 0 },
				new DebitCreditEntryItem { AccountPK = aRInvoice.Lines[0].AL_AG, LocalAmount = -200m, OSAmount = -400m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = aRInvoice.Lines[0].AL_ReverseDate, GLDType = AccountingConstants.GLDTypeCodes.Recognition, Period = 0 },
			};
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		[TestDate(2023, 03, 04)]
		public void TestCreateDRCREntries_ARINVWithoutPostDate_Unchanged()
		{
			var journalDate = ZDateTime.Today;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeaderNTE2.PK, LocalAmount = 200m, OSAmount = 400m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = journalDate, GLDType = AccountingConstants.GLDTypeCodes.Recognition, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK, LocalAmount = -200m, OSAmount = -400m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = journalDate, GLDType = AccountingConstants.GLDTypeCodes.Recognition, Period = 0 },
			};

			AssertCreateDRCREntries_ARINV(true, false, expectedDRCRLines);
		}

		[TestDate(2023, 03, 04)]
		public void TestCreateDRCREntries_ARINVWithoutPostDate_Added()
		{
			var journalDate = ZDateTime.Today;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeaderNTE2.PK, LocalAmount = 200m, OSAmount = 400m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = journalDate, GLDType = AccountingConstants.GLDTypeCodes.Recognition, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK, LocalAmount = -200m, OSAmount = -400m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = journalDate, GLDType = AccountingConstants.GLDTypeCodes.Recognition, Period = 0 },
			};

			AssertCreateDRCREntries_ARINV(true, true, expectedDRCRLines);
		}

		[TestDate(2023, 03, 04)]
		public void TestCreateDRCREntries_ARINVWithoutReverseDate_Added()
		{
			var journalDate = ZDateTime.Today;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK, LocalAmount = 200m, OSAmount = 400m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARControlAccount, JournalDate = journalDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeaderNTE2.PK, LocalAmount = -200m, OSAmount = -400m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = journalDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 }
			};

			AssertCreateDRCREntries_ARINV(false, true, expectedDRCRLines);
		}

		[TestDate(2023, 03, 04)]
		public void TestCreateDRCREntries_ARINVWithoutReverseDate_Unchanged()
		{
			var journalDate = ZDateTime.Today;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK, LocalAmount = 200m, OSAmount = 400m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARControlAccount, JournalDate = journalDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeaderNTE2.PK, LocalAmount = -200m, OSAmount = -400m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = journalDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 }
			};

			AssertCreateDRCREntries_ARINV(false, false, expectedDRCRLines);
		}

		void AssertCreateDRCREntries_ARINV(bool isReverse, bool isAdded, DebitCreditEntryItem[] expectedDRCRLines)
		{
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE2.PK.ToGuid());

			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV0001", TestObjectCreator.USD, 2m, 100m, 0m, 200m, 0m, TestObjectCreator.ABIGAS, TestObjectCreator.DSBChargeCode.PK);
			if (!isReverse)
			{
				aRInvoice.AH_PostDate = ZDateTime.Empty;
			}

			Factory.Save();

			if (isReverse)
			{
				aRInvoice.Lines[0].AL_PostDate = ZDateTime.Empty;
			}

			var row = ((INeedRow)aRInvoice.Lines[0]).Row;
			row.AcceptChanges();
			if (isAdded)
			{
				row.SetAdded();
			}

			GeneralLedgerDataRetriever.GetTransactionHeaderInfo(new ReadOnlyBusinessObjectFactory(), (Guid)row[AccTransactionLinesSchema.Constants.AL_AH]);

			var creator = new INVCRDADJGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(row);
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public override void TestHasValidControlAccount()
		{
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV0001", TestObjectCreator.USD, 2m, 100m, 1m, 200m, 1m);
			aRInvoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			Factory.Save();

			var row = ((INeedRow)aRInvoice.Lines[0]).Row;
			row.SetAdded();

			GeneralLedgerDataRetriever.GetTransactionHeaderInfo(new ReadOnlyBusinessObjectFactory(), (Guid)row[AccTransactionLinesSchema.Constants.AL_AH]);

			var creator = new INVCRDADJGeneralLedgerDataLineCreator();

			var entry = new DebitCreditEntry { EntryItems = Array.Empty<DebitCreditEntryItem>() };
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			AssertNoExceptionThrown(() =>
			{
				entry = creator.CreateDRCREntries(row);
			});
			Assert(entry.EntryItems.Any());
		}

		public void TestDRCREntries_CMTLines()
		{
			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV0001", TestObjectCreator.USD, 2m, 100m, 0m, 200m, 0m, TestObjectCreator.ABIGAS, TestObjectCreator.CommentChargeCode.PK);
			Factory.Save();

			var creator = new INVCRDADJGeneralLedgerDataLineCreator();
			var row = ((INeedRow)aRInvoice.Lines[0]).Row;
			row.SetAdded();

			GeneralLedgerDataRetriever.GetTransactionHeaderInfo(new ReadOnlyBusinessObjectFactory(), (Guid)row[AccTransactionLinesSchema.Constants.AL_AH]);

			var entry = new DebitCreditEntry { EntryItems = Array.Empty<DebitCreditEntryItem>() };
			AssertNoExceptionThrown("No exception when amount of CMT line is zero", () =>
			{
				entry = creator.CreateDRCREntries(row);
			});
			Assert(!entry.EntryItems.Any());

			aRInvoice.Lines[0].AL_LineAmount = 1m;
			row = ((INeedRow)aRInvoice.Lines[0]).Row;
			row.AcceptChanges();
			row.SetAdded();

			AssertExceptionThrown<InvalidAccountingJournalOperationException>("Exception occurs when amount of CMT line is not zero", () =>
			{
				entry = creator.CreateDRCREntries(row);
			});
			Assert(!entry.EntryItems.Any());

			aRInvoice.Lines[0].AL_AC = Guid.Empty;
			aRInvoice.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;
			row = ((INeedRow)aRInvoice.Lines[0]).Row;
			row.AcceptChanges();
			row.SetAdded();

			AssertNoExceptionThrown("No exception when chargeCode is null and AL_AG is not null", () =>
			{
				entry = creator.CreateDRCREntries(row);
			});
			Assert(entry.EntryItems.Any());
		}

		public void TestDRCREntries_INVWithEmptyGlHeader()
		{
			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV0001", TestObjectCreator.USD, 2m, 100m, 0m, 200m, 0m, TestObjectCreator.ABIGAS, Guid.Empty);
			aRInvoice.AH_PostDate = ZDateTime.Empty;
			Factory.Save();

			aRInvoice.Lines[0].AL_AG = Guid.Empty;
			var creator = new INVCRDADJGeneralLedgerDataLineCreator();
			var row = ((INeedRow)aRInvoice.Lines[0]).Row;
			row.AcceptChanges();
			row.SetAdded();

			AssertExceptionThrown<InvalidAccountingJournalOperationException>("Exception occurs when AL_AG is empty", () =>
			{
				creator.CreateDRCREntries(row);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE2.PK.ToGuid());
		}
	}
}
