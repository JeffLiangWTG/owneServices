using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class JCJRJJNLGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public override void TestHasValidControlAccount()
		{
			var postDate = new DateTime(2023, 05, 19);
			var journal = TestObjectCreator.CreateJCJournalHeader(postDate, 20m);
			var line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.RevenueChargeCode, TestObjectCreator.Job1, postDate, 20m);

			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.RevenueChargeCode, TestObjectCreator.Job2, 20m);
			Factory.Save();

			AssertValidControlAccount(new JCJRJJNLGeneralLedgerDataLineCreator(), AccountingConfigurationRegistry.Instance.CFXAccount, ((INeedRow)line).Row);
			AssertValidControlAccount(new JCJRJJNLGeneralLedgerDataLineCreator(), AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount, ((INeedRow)jobRevenueJournal.Lines[0]).Row);

			void AssertValidControlAccount(JCJRJJNLGeneralLedgerDataLineCreator creator, GuidRegistryItem guidRegistryItem, DataRow lineRow)
			{
				lineRow.AcceptChanges();
				lineRow.SetAdded();

				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

				var entry = new DebitCreditEntry { EntryItems = Array.Empty<DebitCreditEntryItem>() };
				AssertExceptionThrown<MissingGLHeaderException>(() =>
				{
					entry = creator.CreateDRCREntries(((INeedRow)line).Row);
				});
				Assert(!entry.EntryItems.Any());

				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
				guidRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());

				AssertNoExceptionThrown(() =>
				{
					entry = creator.CreateDRCREntries(((INeedRow)line).Row);
				});
				Assert(entry.EntryItems.Any());
			}
		}

		public void TestCreateDRCREntries_JNL_OnlyPost()
		{
			var postDate = new DateTime(2023, 05, 19);
			var journal = TestObjectCreator.CreateJCJournalHeader(postDate, 20m);
			var line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, TestObjectCreator.Job1, postDate, 20m);
			Factory.Save();

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK.ToGuid(), LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK.ToGuid(), LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.CFXGLAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Posting, Period = 0 },
			};
			AssertCreateDRCREntries_JNLJRJ(((INeedRow)line).Row, AccountingConfigurationRegistry.Instance.CFXAccount, expectedDRCRLines);
		}

		public void TestCreateDRCREntries_JNL_PostAndReverse()
		{
			var postDate = new DateTime(2023, 05, 19);
			var journal = TestObjectCreator.CreateJCJournalHeader(postDate, 20m);
			var line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.RevenueChargeCode, TestObjectCreator.Job1, postDate, 20m);
			Factory.Save();
			line.AL_ReverseDate = postDate;

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			var lineRow = ((INeedRow)line).Row;
			var creator = new JCJRJJNLGeneralLedgerDataLineCreator();
			lineRow.AcceptChanges();
			lineRow.SetAdded();

			var entry = creator.CreateDRCREntries(lineRow);
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK.ToGuid(), LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK.ToGuid(), LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.CFXGLAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = line.AL_AG, LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Recognition, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK.ToGuid(), LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Recognition, Period = 0 }
			};
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public void TestCreateDRCREntries_JNL_OnlyReverse_Modified()
		{
			var postDate = new DateTime(2023, 05, 19);
			var journal = TestObjectCreator.CreateJCJournalHeader(postDate, 20m);
			var line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.RevenueChargeCode, TestObjectCreator.Job1, postDate, 20m);
			Factory.Save();
			line.AL_ReverseDate = postDate;

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = line.AL_AG, LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Recognition, Period = 0  },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK.ToGuid(), LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Recognition, Period = 0 }
			};

			AssertCreateDRCREntries_JNLJRJ(((INeedRow)line).Row, AccountingConfigurationRegistry.Instance.CFXAccount, expectedDRCRLines, DataRowState.Modified);
		}

		public void TestCreateDRCREntries_JNL_OnlyReverse_Unchanged()
		{
			var postDate = new DateTime(2023, 05, 19);
			var journal = TestObjectCreator.CreateJCJournalHeader(postDate, 20m);
			var line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.RevenueChargeCode, TestObjectCreator.Job1, postDate, 20m);
			Factory.Save();
			line.AL_PostDate = DateTime.MinValue;
			line.AL_ReverseDate = postDate;

			var row = ((INeedRow)line).Row;

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = line.AL_AG, LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Recognition, Period = 0  },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK.ToGuid(), LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Recognition, Period = 0 }
			};

			AssertCreateDRCREntries_JNLJRJ(row, AccountingConfigurationRegistry.Instance.CFXAccount, expectedDRCRLines, DataRowState.Unchanged);
		}

		public void TestCreateDRCREntries_JRJ_OnlyPost()
		{
			var expectedRevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			var revRecognitionCollection = AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.Value;
			revRecognitionCollection[0].RecognitionDateOptionCode = expectedRevRecognitionType;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revRecognitionCollection);

			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, TestObjectCreator.Job1, 20m);
			var line = jobRevenueJournal.Lines[0];
			Factory.Save();

			var postDate = line.AL_PostDate;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK.ToGuid(), LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK.ToGuid(), LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.JobRevenueJournalControlAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Posting, Period = 0 },
			};

			AssertCreateDRCREntries_JNLJRJ(((INeedRow)line).Row, AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount, expectedDRCRLines);
		}

		public void TestCreateDRCREntries_JRJ_PostAndReverse()
		{
			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.RevenueChargeCode, TestObjectCreator.Job1, 20m);
			var line = jobRevenueJournal.Lines[0];
			Factory.Save();

			var reverseDate = line.AL_ReverseDate;
			var postDate = line.AL_PostDate;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK.ToGuid(), LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK.ToGuid(), LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.JobRevenueJournalControlAccount, JournalDate = postDate, GLDType = GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = line.AL_AG, LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = reverseDate, GLDType = GLDTypeCodes.Recognition, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK.ToGuid(), LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = reverseDate, GLDType = GLDTypeCodes.Recognition, Period = 0 }
			};

			AssertCreateDRCREntries_JNLJRJ(((INeedRow)line).Row, AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount, expectedDRCRLines);
		}

		public void TestCreateDRCREntries_JRJ_OnlyReverse()
		{
			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.RevenueChargeCode, TestObjectCreator.Job1, 20m);
			var line = jobRevenueJournal.Lines[0];
			Factory.Save();

			line.AL_PostDate = DateTime.MinValue;
			var reverseDate = line.AL_ReverseDate;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = line.AL_AG, LocalAmount = 20m, OSAmount = 20m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = reverseDate, GLDType = GLDTypeCodes.Recognition, Period = 0 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK.ToGuid(), LocalAmount = -20m, OSAmount = -20m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.ARSuspenseControlAccount, JournalDate = reverseDate, GLDType = GLDTypeCodes.Recognition, Period = 0 }
			};

			AssertCreateDRCREntries_JNLJRJ(((INeedRow)line).Row, AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount, expectedDRCRLines, DataRowState.Added);
		}

		void AssertCreateDRCREntries_JNLJRJ(DataRow lineRow, GuidRegistryItem registryItem, DebitCreditEntryItem[] expectedDRCRLines, DataRowState rowState = DataRowState.Added)
		{
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			var creator = new JCJRJJNLGeneralLedgerDataLineCreator();
			lineRow.AcceptChanges();

			switch (rowState)
			{
				case DataRowState.Added:
					lineRow.SetAdded();
					break;
				case DataRowState.Modified:
					lineRow.SetModified();
					break;
				default:
					break;
			}

			var entry = creator.CreateDRCREntries(lineRow);
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}
	}
}
