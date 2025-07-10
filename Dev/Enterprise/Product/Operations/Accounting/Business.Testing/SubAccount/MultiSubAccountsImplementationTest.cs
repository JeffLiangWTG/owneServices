using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Accounting.Business.Testing
{
	[TestsSubclassesOf(typeof(ISupportMultiSubAccounts))]
	public abstract class MultiSubAccountsImplementationTest : TestCaseWithFactory
	{
		protected virtual ISupportMultiSubAccounts GetSubAccountSupportedBusinessObject()
		{
			return GetSubAccountSupportedBusinessObject(Factory);
		}

		protected ISupportMultiSubAccounts GetSubAccountSupportedBusinessObject(BusinessObjectFactory factory)
		{
			return (ISupportMultiSubAccounts)factory.NewWithValidTestData(TestedTypeHelper.GetTestedType(GetType()));
		}

		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		[ExpectNoExceptions]
		public void Test_SubClassParentTableCode_IsClearedWhen_SubClassParentId_IsCleared_OnSave()
		{
			var bizO = GetSubAccountSupportedBusinessObject();
			if (bizO.IsMultiSubAccountsSupported)
			{
				var subAccount = bizO.SubAccounts.AddNew();
				subAccount.SubAccountTypeParentTableCode = Core.Constants.SubAccountType.Organization;

				Assert("Precondition: ParentTableCode is set", !string.IsNullOrEmpty(subAccount.SubAccountTypeParentTableCode));
				Assert("Precondition: Parent Id is not set", subAccount.SubAccountParentId == ZGuid.Empty);
				AssertNoErrors("Precondition: This condition is valid as the sub class is not mandatory for the GL Account", subAccount.SubAccountParentIdInfo);

				Factory.Save();

				AssertEquals("SubAccounts is cleared", 0, bizO.SubAccounts.SubAccountElements.Count());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestValidateSubAccountParentId_When_GLHeaderPK_isSet()
		{
			var glHeader1 = TestObjectCreator.CreateGLHeader();
			var glSubAccount1 = glHeader1.SubAccountTypes.AddNew();
			glSubAccount1.ASA_SubClass = OrgHeaderSchema.Constants.Prefix;
			glSubAccount1.ASA_IsSubClassValidationRuleMandatory = true;

			var glHeader2 = TestObjectCreator.CreateGLHeader();
			var glSubAccount2 = glHeader2.SubAccountTypes.AddNew();
			glSubAccount2.ASA_SubClass = OrgHeaderSchema.Constants.Prefix;

			var bizO = GetSubAccountSupportedBusinessObject();
			if (bizO.IsMultiSubAccountsSupported)
			{
				SetGLHeader(bizO, glHeader1.PK);
				var subAccount = GetSubAccount(bizO);
				AssertEquals(OrgHeaderSchema.Constants.Prefix, subAccount.SubAccountTypeParentTableCode);
				AssertEquals(ZGuid.Empty, subAccount.SubAccountParentId);
				AssertHasError(subAccount.SubAccountParentIdInfo, "Please enter a Sub Account.");

				SetGLHeader(bizO, glHeader2.PK);
				subAccount = GetSubAccount(bizO);
				AssertEquals(OrgHeaderSchema.Constants.Prefix, subAccount.SubAccountTypeParentTableCode);
				AssertEquals(ZGuid.Empty, subAccount.SubAccountParentId);
				AssertNoErrors(subAccount.SubAccountParentIdInfo);
			}
			else
			{
				AssertEquals(0, bizO.SubAccounts.SubAccountElements.Count());
			}
		}

		public void TestSubAccountFields_OnBizOLoad()
		{
			var bizO = GetSubAccountSupportedBusinessObject();
			if (bizO.IsMultiSubAccountsSupported)
			{
				var subAccountType = OrgHeaderSchema.Constants.Prefix;
				var glHeader1 = TestObjectCreator.CreateGLHeader();
				var glSubAccount1 = glHeader1.SubAccountTypes.AddNew();
				glSubAccount1.ASA_SubClass = subAccountType;

				SetGLHeader(bizO, glHeader1.PK);
				AssertEquals("Precondition", 1, bizO.SubAccounts.SubAccountElements.Count());
				var subAccount = GetSubAccount(bizO);

				subAccount.SubAccountParentId = ZGuid.Empty;
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var result = (ISupportMultiSubAccounts)newFactory.Load(TestedTypeHelper.GetTestedType(GetType()), ((BusinessObject)bizO).PK);

				AssertEquals("After business object is loaded SubAccounts will not set again", 0, result.SubAccounts.SubAccountElements.Count());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSubAccountParentId_ReadOnly()
		{
			var bizO = GetSubAccountSupportedBusinessObject();
			if (bizO.IsMultiSubAccountsSupported)
			{
				var glAccount = TestObjectCreator.CreateGLHeader();
				var glSubAccount = glAccount.SubAccountTypes.AddNew();
				glSubAccount.ASA_SubClass = OrgHeaderSchema.Constants.Prefix;

				AssertEquals(0, bizO.SubAccounts.SubAccountElements.Count());

				SetGLHeader(bizO, glAccount.PK);
				var subAccount = GetSubAccount(bizO);
				subAccount.SubAccountParentId = TestObjectCreator.ABIGAS.PK;
				AssertEquals("SubAccountParentId is not ready only", false, subAccount.SubAccountParentIdInfo.ReadOnly);

				SetJob(bizO, Guid.NewGuid());
				if (bizO is APJournal || bizO is ARJournal)
				{
					AssertEquals(1, bizO.SubAccounts.SubAccountElements.Count());
				}
				else
				{
					AssertEquals(0, bizO.SubAccounts.SubAccountElements.Count());
				}

				SetJob(bizO, Guid.Empty);
				subAccount = GetSubAccount(bizO);
				subAccount.SubAccountTypeParentTableCode = OrgHeaderSchema.Constants.Prefix;
				AssertEquals(false, subAccount.SubAccountParentIdInfo.ReadOnly);

				var reverseTransaction = (TransactionHeader)ReverseTransaction(bizO);
				if (reverseTransaction != null)
				{
					reverseTransaction.AH_TransactionNum = "00001001";
				}
				//That makes it identical to what happens functionally on the Unapproved Transactions module.
				TransactionHeader uaHeader = null;
				var header = bizO as TransactionHeader;
				if (header != null && header.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					uaHeader = header;
				}

				var line = bizO as TransactionLine;
				if (line != null && line.AL_LineType == TransactionLineTypes.UnapprovedCost)
				{
					uaHeader = Factory.Load<TransactionHeader>(line.AL_AH);
				}

				if (uaHeader != null)
				{
					uaHeader.Delete();
				}

				Factory.Save();

				if (uaHeader != null)
				{
					Assert(uaHeader.IsReversing);
				}
				else
				{
					Assert("Precondition", IsTransactionReversing(bizO));
				}

				if (bizO is APJournal || bizO is ARJournal)
				{
					subAccount = GetSubAccount(bizO);
					Assert(subAccount.SubAccountParentIdInfo.ReadOnly);
				}
				else
				{
					AssertEquals(0, bizO.SubAccounts.SubAccountElements.Count());
				}
			}
			else
			{
				AssertEquals(0, bizO.SubAccounts.SubAccountElements.Count());
			}
		}

		public void TestSettingGLPK_Sets_SubClassType()
		{
			var bizO = GetSubAccountSupportedBusinessObject();

			var glHeader = TestObjectCreator.CreateGLHeader();
			var glSubAccount = glHeader.SubAccountTypes.AddNew();
			glSubAccount.ASA_SubClass = GlbStaffSchema.Constants.Prefix;

			SetGLHeader(bizO, ZGuid.Empty);

			AssertEquals(0, bizO.SubAccounts.SubAccountElements.Count());

			SetGLHeader(bizO, glHeader.PK);

			if (bizO.IsMultiSubAccountsSupported)
			{
				var subAccount = GetSubAccount(bizO);
				AssertEquals(GlbStaffSchema.Constants.Prefix, subAccount.SubAccountTypeParentTableCode);
			}
			else
			{
				AssertEquals(0, bizO.SubAccounts.SubAccountElements.Count());
			}

			SetGLHeader(bizO, ZGuid.Empty);
			AssertEquals(0, bizO.SubAccounts.SubAccountElements.Count());
		}

		public void TestTransactionReversal()
		{
			var subClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			var glHeader = TestObjectCreator.CreateGLHeader();
			var glSubAccount = glHeader.SubAccountTypes.AddNew();
			glSubAccount.ASA_SubClass = subClassParentTableCode;

			var bizO = GetSubAccountSupportedBusinessObject();

			if (bizO.IsMultiSubAccountsSupported)
			{
				SetGLHeader(bizO, glHeader.PK);
				var subAccount = GetSubAccount(bizO);
				AssertEquals("Precondition: SubClassParentTableCode is set for the journal", subClassParentTableCode, subAccount.SubAccountTypeParentTableCode);
				subAccount.SubAccountParentId = TestObjectCreator.ABIGAS.PK;

				Factory.Save();

				var reversedTransaction = ReverseTransaction(bizO);

				AssertSubAccountValuesOnReversedTransactions(reversedTransaction, expectedSubAccountParentTableCode: subClassParentTableCode, expectedSubAccountParentId: TestObjectCreator.ABIGAS.PK);
			}
			else
			{
				Assert(true);
			}
		}

		protected abstract IReversing ReverseTransaction(ISupportMultiSubAccounts bizO);
		protected abstract bool IsTransactionReversing(ISupportMultiSubAccounts bizO);
		protected abstract void AssertSubAccountValuesOnReversedTransactions(IReversing reversedTransaction, ZString expectedSubAccountParentTableCode, ZGuid expectedSubAccountParentId);
		protected abstract void SetGLHeader(ISupportMultiSubAccounts bizO, ZGuid glHeaderPK);
		protected abstract void SetJob(ISupportMultiSubAccounts bizO, ZGuid jobPK);

		public void TestSubAccountParentId()
		{
			var newFactory = new BusinessObjectFactory();
			var bizO = GetSubAccountSupportedBusinessObject(newFactory);
			if (bizO.IsMultiSubAccountsSupported)
			{
				AccGLHeader glHeader = TestObjectCreator.CreateGLHeader();
				var glSubAccount = glHeader.SubAccountTypes.AddNew();
				glHeader.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
				glSubAccount.ASA_SubClassDisplayName = Core.Constants.SubAccountType.Organization;
				glSubAccount.ASA_IsSubClassValidationRuleMandatory = true;
				Factory.Save();

				SetGLHeader(bizO, glHeader.PK);
				var subAccount = GetSubAccount(bizO);
				AssertEquals("Precondition", OrgHeaderSchema.Constants.Prefix, subAccount.SubAccountTypeParentTableCode);

				subAccount.SubAccountParentId = Guid.Empty;
				AssertHasError(subAccount.SubAccountParentIdInfo, "Please enter a Sub Account.");

				subAccount.SubAccountParentId = TestObjectCreator.ABIGAS.PK;

				AssertNoErrors(subAccount.SubAccountParentIdInfo);

				glSubAccount.ASA_IsSubClassValidationRuleMandatory = false;
				Factory.Save();

				subAccount.SubAccountParentId = Guid.Empty;

				AssertNoErrors(subAccount.SubAccountParentIdInfo);
			}
			else
			{
				AssertEquals(0, bizO.SubAccounts.SubAccountElements.Count());
			}
		}

		public void TestSubClassParentIdInvalid()
		{
			var newFactory = new BusinessObjectFactory();
			var bizO = GetSubAccountSupportedBusinessObject(newFactory);
			if (bizO.IsMultiSubAccountsSupported)
			{
				var subAccount = bizO.SubAccounts.AddNew();
				//OrgHeader
				subAccount.SubAccountTypeParentTableCode = OrgHeaderSchema.Constants.Prefix;
				subAccount.SubAccountParentId = ZGuid.NewZGuid();

				AssertHasError(subAccount.SubAccountParentIdInfo, "Enter a valid Sub Account.");

				var orgHeader = TestObjectCreator.CreateOrgHeader("org123", false, false);
				orgHeader.OH_IsActive = true;

				Factory.Save();

				subAccount.SubAccountParentId = orgHeader.PK;

				AssertNoError(subAccount.SubAccountParentIdInfo, "Enter a valid Sub Account.");

				//GlbStaff
				subAccount.SubAccountTypeParentTableCode = GlbStaffSchema.Constants.Prefix;
				subAccount.SubAccountParentId = ZGuid.NewZGuid();

				AssertHasError(subAccount.SubAccountParentIdInfo, "Enter a valid Sub Account.");

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "TS";
				staff.GS_IsActive = true;

				Factory.Save();

				subAccount.SubAccountParentId = staff.PK;

				AssertNoError(subAccount.SubAccountParentIdInfo, "Enter a valid Sub Account.");

				//GlbGroup
				subAccount.SubAccountTypeParentTableCode = GlbGroupSchema.Constants.Prefix;
				subAccount.SubAccountParentId = ZGuid.NewZGuid();

				AssertHasError(subAccount.SubAccountParentIdInfo, "Enter a valid Sub Account.");

				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.GG_Code = "RANDOM";
				group.GG_IsActive = true;

				Factory.Save();

				subAccount.SubAccountParentId = group.PK;

				AssertNoError(subAccount.SubAccountParentIdInfo, "Enter a valid Sub Account.");

				//Sales gorup
				subAccount.SubAccountTypeParentTableCode = AccGroupsSchema.Constants.Prefix;
				subAccount.SubAccountParentId = ZGuid.NewZGuid();

				AssertHasError(subAccount.SubAccountParentIdInfo, "Enter a valid Sub Account.");

				var salesGroup = Factory.NewWithValidTestData<AccGroups>();
				salesGroup.AR_Code = "Sales";

				Factory.Save();

				subAccount.SubAccountParentId = salesGroup.PK;

				AssertNoError(subAccount.SubAccountParentIdInfo, "Enter a valid Sub Account.");
			}
			else
			{
				AssertEquals(0, bizO.SubAccounts.SubAccountElements.Count());
			}
		}

		protected ISupportSubAccount GetSubAccount(ISupportMultiSubAccounts parent)
		{
			AssertEquals("Precondition", 1, parent.SubAccounts.SubAccountElements.Count());
			return parent.SubAccounts.SubAccountElements.FirstOrDefault();
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}
		TestObjectCreator testObjectCreator;
	}

	#region TransactionLine

	public abstract class DependentTransactionLineMultiSubAccountsImplementationTest<T1, T2> : MultiSubAccountsImplementationTest
		where T1 : TransactionHeaderWithLines
		where T2 : DependentTransactionLine
	{
		protected override ISupportMultiSubAccounts GetSubAccountSupportedBusinessObject()
		{
			var header = Factory.NewWithValidTestData<T1>();
			var line = header.Lines.AddNew();
			line.FillWithValidTestData();

			return (T2)line;
		}

		protected override IReversing ReverseTransaction(ISupportMultiSubAccounts bizO)
		{
			var line = bizO as DependentTransactionLine;
			var transaction = Factory.Load<TransactionHeader>(line.AL_AH);
			transaction.GenerateReverseTransaction(true);

			return transaction.ReverseTransaction;
		}

		protected override bool IsTransactionReversing(ISupportMultiSubAccounts bizO)
		{
			var line = bizO as DependentTransactionLine;
			var transaction = Factory.Load<TransactionHeader>(line.AL_AH);
			return transaction.IsReversing;
		}

		protected override void SetGLHeader(ISupportMultiSubAccounts bizO, ZGuid glHeaderPK)
		{
			var line = bizO as DependentTransactionLine;
			line.AL_AG = glHeaderPK;
		}

		protected override void SetJob(ISupportMultiSubAccounts bizO, ZGuid jobPK)
		{
			var line = bizO as DependentTransactionLine;
			line.AL_JH = jobPK;
		}

		public void TestChangingAL_JH_Resets_SubClassParentId()
		{
			var bizO = GetSubAccountSupportedBusinessObject();
			if (bizO.IsMultiSubAccountsSupported)
			{
				TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

				var subAccount = bizO.SubAccounts.AddNew();
				subAccount.SubAccountTypeParentTableCode = Core.Constants.SubAccountType.Organization;
				subAccount.SubAccountParentId = testObjectCreator.ABIGAS.PK;

				(bizO as DependentTransactionLine).AL_JH = ZGuid.NewZGuid();
				AssertEquals("Changing AL_JH should reset SubAccountTypeParentTableCode", string.Empty, subAccount.SubAccountTypeParentTableCode);
				AssertEquals("Changing AL_JH should reset SubAccountParentId", Guid.Empty, subAccount.SubAccountParentId);
			}
			else
			{
				Assert(true);
			}
		}

		protected override void AssertSubAccountValuesOnReversedTransactions(IReversing reversedTransaction, ZString expectedSubAccountParentTableCode, ZGuid expectedSubAccountParentId)
		{
			var transaction = reversedTransaction as TransactionHeaderWithLines;

			AssertNotNull(transaction);
			AssertNotNull(transaction.Lines);
			Assert(transaction.Lines.Count > 0);

			var line = transaction.Lines[0];
			var subAccount = GetSubAccount(line);
			AssertEquals(expectedSubAccountParentTableCode, subAccount.SubAccountTypeParentTableCode);
			AssertEquals(expectedSubAccountParentId, subAccount.SubAccountParentId);
		}
	}

	[TestedType(typeof(APInvoiceLine))]
	public class APInvoiceLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<APInvoice, APInvoiceLine>
	{
	}

	[TestedType(typeof(ARInvoiceLine))]
	public class ARInvoiceLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<ARInvoice, ARInvoiceLine>
	{
	}

	[TestedType(typeof(UACreditNoteLine))]
	public class UACreditNoteLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<UACreditNote, UACreditNoteLine>
	{
		protected override void AssertSubAccountValuesOnReversedTransactions(IReversing reversedTransaction, ZString expectedSubAccountParentTableCode, ZGuid expectedSubAccountParentId)
		{
			AssertNull("No reverse transaction created for unapproved transactions", reversedTransaction);
		}
	}

	[TestedType(typeof(UAInvoiceLine))]
	public class UAInvoiceLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<UAInvoice, UAInvoiceLine>
	{
		protected override void AssertSubAccountValuesOnReversedTransactions(IReversing reversedTransaction, ZString expectedSubAccountParentTableCode, ZGuid expectedSubAccountParentId)
		{
			AssertNull("No reverse transaction created for unapproved transactions", reversedTransaction);
		}
	}

	[TestedType(typeof(DirectPaymentLine))]
	public class DirectPaymentLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<DirectPayment, DirectPaymentLine>
	{
	}

	[TestedType(typeof(DirectReceiptLine))]
	public class DirectReceiptLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<DirectReceipt, DirectReceiptLine>
	{
	}

	[TestedType(typeof(FCBAdjustmentJournalLine))]
	public class FCBAdjustmentJournalLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<FCBAdjustmentJournal, FCBAdjustmentJournalLine>
	{
		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
		}
	}

	[TestedType(typeof(GLJournalLine))]
	public class GLJournalLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<GLJournal, GLJournalLine>
	{
	}

	[TestedType(typeof(JCJournalLine))]
	public class JCJournalLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<JCJournalHeader, JCJournalLine>
	{
	}

	[TestedType(typeof(JobRevenueJournalLine))]
	public class JobRevenueJournalLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<JobRevenueJournal, JobRevenueJournalLine>
	{
	}

	[TestedType(typeof(APAdjustmentNoteLine))]
	public class APAdjustmentNoteLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<APAdjustmentNote, APAdjustmentNoteLine>
	{
	}

	[TestedType(typeof(ARAdjustmentNoteLine))]
	public class ARAdjustmentNoteLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<ARAdjustmentNote, ARAdjustmentNoteLine>
	{
	}

	[TestedType(typeof(APCreditNoteLine))]
	public class APCreditNoteLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<APCreditNote, APCreditNoteLine>
	{
	}

	[TestedType(typeof(ARCreditNoteLine))]
	public class ARCreditNoteLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<ARCreditNote, ARCreditNoteLine>
	{
	}

	[TestedType(typeof(BankTransferChargeLine))]
	public class BankTransferChargeLineMultiSubAccountsImplementationTest : DependentTransactionLineMultiSubAccountsImplementationTest<BankTransferCharge, BankTransferChargeLine>
	{
		protected override ISupportMultiSubAccounts GetSubAccountSupportedBusinessObject()
		{
			return Factory.NewWithValidTestData<BankTransferChargeLine>();
		}
	}

	#endregion

	#region TransactionHeader for Journal

	public abstract class JournalMultiSubAccountsImplementationTest : MultiSubAccountsImplementationTest
	{
		protected override IReversing ReverseTransaction(ISupportMultiSubAccounts bizO)
		{
			var journal = bizO as Journal;
			journal.GenerateReverseTransaction(true);

			return journal.ReverseTransaction;
		}

		protected override bool IsTransactionReversing(ISupportMultiSubAccounts bizO)
		{
			var journal = bizO as Journal;
			return journal.IsReversing;
		}

		protected override void SetGLHeader(ISupportMultiSubAccounts bizO, ZGuid glHeaderPK)
		{
			var journal = bizO as Journal;
			journal.AH_AG = glHeaderPK;
		}

		protected override void SetJob(ISupportMultiSubAccounts bizO, ZGuid jobPK)
		{
			var journal = bizO as Journal;
			journal.AH_JH = jobPK;
		}

		protected override void AssertSubAccountValuesOnReversedTransactions(IReversing reversedTransaction, ZString expectedSubAccountParentTableCode, ZGuid expectedSubAccountParentId)
		{
			Journal reversedJournal = reversedTransaction as Journal;

			AssertNotNull(reversedJournal);
			var subAccount = GetSubAccount(reversedJournal);
			AssertEquals(expectedSubAccountParentTableCode, subAccount.SubAccountTypeParentTableCode);
			AssertEquals(expectedSubAccountParentId, subAccount.SubAccountParentId);
		}
	}

	[TestedType(typeof(APJournal))]
	public class APJournalMultiSubAccountsImplementationTest : JournalMultiSubAccountsImplementationTest
	{
	}

	[TestedType(typeof(ARJournal))]
	public class ARJournalMultiSubAccountsImplementationTest : JournalMultiSubAccountsImplementationTest
	{
	}

	#endregion
}
