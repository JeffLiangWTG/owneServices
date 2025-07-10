using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class TransactionLineSubAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAL1_SubClassParentId()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			var glHeaderSubAccount1 = TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);
			var glHeaderSubAccount2 = TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, false);

			var checkedEnterMessage = "Please enter a Sub Account.";
			var subAccount1CheckedEnterMessage = "Please enter a Sub Account 1.";
			var subAccount2CheckedEnterMessage = "Please enter a Sub Account 2.";
			var line = CreateDependentTransactionLine();

			AssertNoErrors("Pre-condition", line.AL_Calc_FirstSubClassParentIdInfo);
			AssertNoErrors("Pre-condition", line.AL_Calc_SecondSubClassParentIdInfo);

			line.AL_AG = glHeader.PK;
			var firstSubAccount = line.SubAccounts.FirstSubAccount;
			var secondSubAccount = line.SubAccounts.SecondSubAccount;

			AssertEquals("first AL1_SubClassParentId", ZGuid.Empty, firstSubAccount.AL1_SubClassParentId);
			AssertEquals("second AL1_SubClassParentId", ZGuid.Empty, secondSubAccount.AL1_SubClassParentId);
			AssertEquals("AL_Calc_FirstSubClassParentId", ZGuid.Empty, line.AL_Calc_FirstSubClassParentId);
			AssertEquals("AL_Calc_SecondSubClassParentId", ZGuid.Empty, line.AL_Calc_SecondSubClassParentId);

			AssertHasError("first AL1_SubClassParentId should have errors when value is empty and relate to ASA_IsSubClassValidationRuleMandatory is true.", firstSubAccount.AL1_SubClassParentIdInfo, checkedEnterMessage);
			AssertHasError("AL_Calc_FirstSubClassParentId should have errors when value is empty and relate to ASA_IsSubClassValidationRuleMandatory is true.", line.AL_Calc_FirstSubClassParentIdInfo, subAccount1CheckedEnterMessage);
			AssertNoErrors("second AL1_SubClassParentId should have no errors when value is empty and relate to ASA_IsSubClassValidationRuleMandatory is false.", secondSubAccount.AL1_SubClassParentIdInfo);
			AssertNoErrors("AL_Calc_SecondSubClassParentId should have no errors when value is empty and relate to ASA_IsSubClassValidationRuleMandatory is false.", line.AL_Calc_SecondSubClassParentIdInfo);

			var organizationPK = TestObjectCreator.Creditor1.PK;
			var staffPK = TestObjectCreator.Staff.PK;
			firstSubAccount.AL1_SubClassParentId = organizationPK;
			secondSubAccount.AL1_SubClassParentId = staffPK;

			AssertEquals("first AL1_SubClassParentId", organizationPK, firstSubAccount.AL1_SubClassParentId);
			AssertEquals("AL_Calc_FirstSubClassParentId", organizationPK, line.AL_Calc_FirstSubClassParentId);
			AssertEquals("secondSubAccount AL1_SubClassParentId", staffPK, secondSubAccount.AL1_SubClassParentId);
			AssertEquals("AL_Calc_SecondSubClassParentId", staffPK, line.AL_Calc_SecondSubClassParentId);

			AssertNoErrors("first AL1_SubClassParentId should have no errors when value is not empty.", firstSubAccount.AL1_SubClassParentIdInfo);
			AssertNoErrors("AL_Calc_FirstSubClassParentId should be no errors when value is not empty.", line.AL_Calc_FirstSubClassParentIdInfo);
			AssertNoErrors("second AL1_SubClassParentId should be no errors when value is not empty.", secondSubAccount.AL1_SubClassParentIdInfo);
			AssertNoErrors("AL_Calc_SecondSubClassParentId should be no errors when value is not empty.", line.AL_Calc_SecondSubClassParentIdInfo);

			glHeaderSubAccount1.ASA_IsSubClassValidationRuleMandatory = false;
			glHeaderSubAccount2.ASA_IsSubClassValidationRuleMandatory = true;
			firstSubAccount.AL1_SubClassParentId = ZGuid.Empty;
			secondSubAccount.AL1_SubClassParentId = ZGuid.Empty;

			AssertNoErrors("first AL1_SubClassParentId should have no errors when value is empty and relate to ASA_IsSubClassValidationRuleMandatory is false.", firstSubAccount.AL1_SubClassParentIdInfo);
			AssertNoErrors("AL_Calc_FirstSubClassParentIdInfo should have no errors when value is empty and relate to ASA_IsSubClassValidationRuleMandatory is false.", line.AL_Calc_FirstSubClassParentIdInfo);
			AssertHasError("second AL1_SubClassParentId should have errors when value is empty and relate to ASA_IsSubClassValidationRuleMandatory is true.", secondSubAccount.AL1_SubClassParentIdInfo, checkedEnterMessage);
			AssertHasError("AL_Calc_SecondSubClassParentIdInfo should have errors when value is empty and relate to ASA_IsSubClassValidationRuleMandatory is true.", line.AL_Calc_SecondSubClassParentIdInfo, subAccount2CheckedEnterMessage);
		}

		protected abstract Type GetExpectedTransactionHeader();

		DependentTransactionLine CreateDependentTransactionLine()
		{
			var header = (TransactionHeaderWithLines)Factory.New(GetExpectedTransactionHeader());
			header.FillWithValidTestData();

			var line = header.Lines.AddNew();
			line.FillWithValidTestData();
			return line;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;

		public class ARInvoiceTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(ARInvoice);
			}
		}

		public class APInvoiceTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(APInvoice);
			}
		}

		public class ARAdjustmentNoteTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(ARAdjustmentNote);
			}
		}

		public class APAdjustmentNoteTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(APAdjustmentNote);
			}
		}

		public class ARCreditNoteTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(ARCreditNote);
			}
		}

		public class APCreditNoteTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(APCreditNote);
			}
		}

		public class UACreditNoteTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(UACreditNote);
			}
		}

		public class UAInvoiceTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(UAInvoice);
			}
		}

		public class DirectReceiptTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(DirectReceipt);
			}
		}

		public class DirectPaymentTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(DirectPayment);
			}
		}

		public class GLJournalTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(GLJournal);
			}
		}

		public class FCBAdjustmentJournalTransactionLineSubAccountValidationTest : TransactionLineSubAccountValidationTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(FCBAdjustmentJournal);
			}
		}
	}
}
