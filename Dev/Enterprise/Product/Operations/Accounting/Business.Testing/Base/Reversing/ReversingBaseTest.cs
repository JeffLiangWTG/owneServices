using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	public abstract class ReversingBaseTest : TestCaseWithFactory
	{
		public void TestTransactionAttachedToActiveBatchDoesNotAllowReversing()
		{
			if (TestIReversingInstance is ARInvoice || TestIReversingInstance is ARCreditNote || TestIReversingInstance is ARAdjustmentNote)
			{
				TestIReversingInstance.Factory.Save();

				var bank = Factory.NewWithValidTestData<AccBankAccount>();
				var batch = Factory.New<AccCollectionBatch>();
				batch.ACB_TotalAmount = 100m;
				batch.ACB_AB = bank.PK;
				batch.ACB_GC = GlbCompany.CurrentCompany.PK;
				batch.ACB_BatchNumber = "0001000";

				var order = Factory.New<AccCollectionOrder>();
				order.ACO_ACB = batch.PK;
				order.ACO_CollectionDate = ZDate.Today;
				order.ACO_OH_Debtor = TestObjectCreator.AALSHI.PK;
				order.ACO_OrderNumber = "000001";
				order.IncludeInBatch = true;
				order.ACO_Amount = 100m;

				var orderline = Factory.New<AccCollectionOrderLine>();
				orderline.AOL_ACO = order.PK;
				orderline.AOL_AH = TestIReversingInstance.PK;
				orderline.AOL_IsCancelled = false;
				orderline.IncludeInOrder = true;
				Factory.Save();

				Assert("Should never allow reverse on transaction attached to active batch.", !Reversing.CanReverseTransaction);
				AssertEquals("Can't reverse error", Reversing.TransactionAttachedToCollectionBatchErrorMessage_ForTestOnly, Reversing.GenerateCantReverseErrorMessage_ForTestOnly());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCantReverseAlreadyReversedTransaction()
		{
			TestIReversingInstance.SetIsReversed(true);
			Assert("Should never allow reverse on an already reversed transaction", !Reversing.CanReverseTransaction);
			ZString alreadyReversedErrorMessage = Reversing.AlreadyReversedErrorMessage_ForTestOnly;
			AssertEquals("Can't reverse error", alreadyReversedErrorMessage, Reversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public virtual void TestErrorMessages()
		{
			ZString expectedErrorMessage = "This transaction cannot be reversed because it has already been reversed or is a reversal of another transaction.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, Reversing.AlreadyReversedErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be reversed because it has been matched with other transactions.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, Reversing.MatchedAndCantReverseErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be reversed because it has been cleared in Cashbook. Please unclear this transaction from the cashbook before reversing.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, Reversing.ClearedInCashBookErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be reversed because it is attached to an active collection batch order.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, Reversing.TransactionAttachedToCollectionBatchErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be reversed because related job has Ready For Financial Closure status.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, Reversing.RelatedJobIsReadyForFinancialClosureAndCantReversedErrorMessage_ForTestOnly);
		}

		public void TestReverseTransaction()
		{
			TestIReversingInstance.SetReverseTransactionToBeGenerated(TestReversingIReversingInstance);
			Reversing.Reverse();
			AssertNotNull("If the reversing class is told to reverse, it should call reverse on the interface, " +
				"which should create another IReversing", Reversing.ReverseTransaction);
		}

		public void TestReverseWhenDoReverseTransactionReturnsNull()
		{
			ARInvoice originalInvoice = Factory.New<ARInvoice>();
			var reversingBase = new ReversingBaseForTest(originalInvoice);
			reversingBase.GenerateValidReverseTransaction = false;
			reversingBase.Reverse();
			Assert(!reversingBase.IsReverseTaxTransactionInvoked);

			reversingBase.GenerateValidReverseTransaction = true;
			reversingBase.Reverse();
			Assert(reversingBase.IsReverseTaxTransactionInvoked);
		}

		public void TestReverseTransactionSetSuspendersToFactory()
		{
			TestIReversingInstance.SetReverseTransactionToBeGenerated(TestReversingIReversingInstance);
			Assert(!ServiceContainerSuspenderHelper.FunctionalitySuspender<ChargeWithCost.OnFactorySavingInCompanyContextSuspender>.IsSuspended(Factory));
			Reversing.Reverse();
			AssertEquals(TestIReversingInstance is IPayment || TestIReversingInstance is Receipt, ServiceContainerSuspenderHelper.FunctionalitySuspender<ChargeWithCost.OnFactorySavingInCompanyContextSuspender>.IsSuspended(Factory));
		}

		public virtual void TestTransactionReversedEmailSentOnSaving()
		{
			SetupStaffMemberEmailAddress();
			AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, StaffGroupPK.ToGuid());

			TestIReversingInstance.Factory.Save();

			AssertEquals("Should not have sent email because reversing transactions were not setup correctly",
				0, Env.OutgoingMailManager.EmailsCreated.Count);

			TestIReversingInstance.SetReverseTransactionToBeGenerated(TestReversingIReversingInstance);
			Reversing.FReverseTransaction_ForTestOnly = TestReversingIReversingInstance;
			TestIReversingInstance.Factory.Save();

			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestEmailIsNotSentWhenFactorySavedFails()
		{
			SetupStaffMemberEmailAddress();
			AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, StaffGroupPK.ToGuid());
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			ReversingBase revBase = new ReversingBase(aRInv);
			revBase.Reverse();
			revBase.Factory_Saved_ForTestOnly(Factory, false);

			AssertEquals("Email should not be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestInactiveOrgDoesNotAllowReversing()
		{
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_IsActive = false;
			newOrg.OH_IsDebtor = true;
			newOrg.OH_IsCreditor = true;
			Factory.Save();
			TestIReversingInstance.Organization = newOrg.PK;
			Assert("Should never allow reverse on transaction for inactive organisation.", !Reversing.CanReverseTransaction);
			AssertEquals("Can't reverse error", Reversing.TransactionForInactiveOrganisationErrorMessage_ForTestOnly, Reversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCorrectTestClassInitialisation()
		{
			AssertEquals("Reversig must have the same class Type as testing reversing class.", GetTestingClassType(), Reversing.GetType());
		}

		public void TestCheckpointsToReverse_NoMatchGroup()
		{
			AssertSequencesEqual("When no match groups, there should be no (additional) Checkpoint required to reverse", Array.Empty<SecurityCheckpoint>(), Reversing.CheckpointsToReverse);
		}

		#region Implementation

		protected ReversingFactory ReversingFactory;
		protected ReversingBase Reversing;
		protected IReversingForTests TestIReversingInstance;
		protected IReversingForTests TestReversingIReversingInstance;
		protected abstract void SetupReversingInstance();
		protected abstract void SetupReversingIReversingInstance();

		protected virtual void SetupReversingObject()
		{
			Reversing = ReversingFactory.NewReversing(TestIReversingInstance);
		}

		protected ZGuid StaffGroupPK
		{
			get
			{
				GlbGroup group = Factory.New<GlbGroup>();
				group.Staff.Add(Factory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK)));
				return group.PK;
			}
		}

		protected ZString EmailAddress
		{
			get { return new ZString("blahblah@whatever.example"); }
		}

		protected void SetupStaffMemberEmailAddress()
		{
			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK));
			currentStaffMember.GS_EmailAddress = EmailAddress;
			staffMemberFactory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			ReversingFactory = new ReversingFactory();
			SetupReversingInstance();
			SetupReversingIReversingInstance();
			TestIReversingInstance.SetFactory(Factory);
			TestReversingIReversingInstance.SetFactory(Factory);
			SetupReversingObject();
		}

		protected override void TearDown()
		{
			base.TearDown();

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		protected abstract Type GetTestingClassType();

		#endregion

		class ReversingBaseForTest : ReversingBase
		{
			public ReversingBaseForTest(IReversing originalTransaction)
				: base(originalTransaction)
			{
			}

			public bool GenerateValidReverseTransaction
			{
				get { return generateValidReverseTransaction; }
				set { generateValidReverseTransaction = value; }
			}

			protected bool generateValidReverseTransaction;

			public bool IsReverseTaxTransactionInvoked
			{
				get { return isReverseTaxTransactionInvoked; }
			}

			protected bool isReverseTaxTransactionInvoked;

			protected override void DoReverseTaxTransaction()
			{
				base.DoReverseTaxTransaction();
				isReverseTaxTransactionInvoked = true;
			}

			protected override void DoReverseTransaction()
			{
				if (GenerateValidReverseTransaction)
				{
					base.DoReverseTransaction();
				}
			}
		}
	}
}
