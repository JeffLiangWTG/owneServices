using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(MultipleReversingProviderForHeader))]
	public class MultipleReversingProviderForHeaderTest : MultipleReversingProviderBaseTest
	{
		public void TestGetSecurityOverrideProvider()
		{
			AssertNull(TestObjectForHeader.GetSecurityOverrideProvider(null));

			var securityProvider = new NonInteractiveSecurityOverrideProvider();
			AssertEquals(securityProvider, TestObjectForHeader.GetSecurityOverrideProvider(securityProvider));
			AssertEquals(securityProvider, TestObjectForHeader.GetSecurityOverrideProvider(new NonInteractiveSecurityOverrideProvider()));
			AssertEquals(securityProvider, TestObjectForHeader.GetSecurityOverrideProvider(null));
		}

		public void TestWrappedObjectParentCollection()
		{
			var invoice = Factory.New<APInvoice>();
			var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(invoice);
			AssertEquals("Precondition: Invoice has not parent collections.", 0, ((IBusinessObjectInternals)invoice).ParentCollections.Length);

			TestObjectForHeader.TransactionsAlreadyReversed.Add(wrapper);
			AssertEquals("Precondition: Invoice must have parent collection after adding its wrapper to TransactionsAlreadyReversed collection.", 1, ((IBusinessObjectInternals)invoice).ParentCollections.Length);

			TestObjectForHeader.TransactionsAlreadyReversed.RemoveAll();
			AssertEquals("Precondition: Invoice must not have parent collection after removing its wrapper from TransactionsAlreadyReversed collection.", 0, ((IBusinessObjectInternals)invoice).ParentCollections.Length);
		}

		protected override BusinessObject[] GetBusinessObjectsForIEnumeratorTesting() =>
			new BusinessObject[]
			{
				new IReversingImplicitlyImplementedWrapperForBinding(Factory.New<APInvoice>()),
				new IReversingImplicitlyImplementedWrapperForBinding(Factory.New<ARInvoice>()),
				new IReversingImplicitlyImplementedWrapperForBinding(Factory.New<APCreditNote>() )
			};

		public void TestReversingReason()
		{
			var invoices = GetBusinessObjectsForIEnumeratorTesting();

			TestObjectForHeader.TransactionsAlreadyReversed.AddRange(invoices);
			string expectedReversingReason = "There are no any reasons.";
			TestObjectForHeader.ReversingReason = expectedReversingReason;
			AssertEquals("MultipleReversingProvider.ReversingReason getter must return set value.", expectedReversingReason, TestObjectForHeader.ReversingReason);
			foreach (IReversing reversedObject in TestObjectForHeader.TransactionsAlreadyReversed)
			{
				AssertEquals("All objects in TransactionsAlreadyReversed must have ReversingReason set in MultipleReversingProvider.ReversingReason.",
					expectedReversingReason, reversedObject.ReversingReason);
			}
		}

		#region ReversalStatusCode

		public void TestReversalStatusCode_ShouldBeEmpty()
		{
			AssertEquals(nameof(ITransaction.ReversalStatusCode), ZString.Empty, (new MultipleReversingProviderForHeader() as ITransaction).ReversalStatusCode);
		}

		public void TestReversalStatusCode_ReadOnly_ShouldBeTrue()
		{
			AssertEquals(nameof(ITransaction.ReversalStatusCode_ReadOnly), true, (new MultipleReversingProviderForHeader() as ITransaction).ReversalStatusCode_ReadOnly);
		}

		public void TestReversalStatusCodeList_ShouldBeNull()
		{
			AssertNull(nameof(ITransaction.ReversalStatusCodeList), (new MultipleReversingProviderForHeader() as ITransaction).ReversalStatusCodeList);
		}

		#endregion ReversalStatusCode

		InvoicingBase CreateInvoiceForCompliance(TestObjectCreator creator, Type invType, string invNum, ZDate allocationDate, string subType, AccComplianceSequence sequence, string complNr = null, string complianceNumberAllocationDateOption = null)
		{
			var inv = creator.CreateInvoice(invType);
			inv.AH_TransactionNum = invNum;

			if (complianceNumberAllocationDateOption != null && complianceNumberAllocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				inv.AH_InvoiceDate = allocationDate;
			}
			else
			{
				inv.AH_PostDate = allocationDate;
			}

			inv.AH_ComplianceSubType = subType;
			inv.AH_XD_ComplianceBook = sequence.PK;
			inv.AH_TransactionReference = complNr;
			return inv;
		}

		[TestDate(2021, 1, 15)]
		public void TestCheckIfComplianceErrors_NoPastTransactionsWithEmptyComplNum_PST()
		{
			AssertCheckIfComplianceErrors_NoPastTransactionsWithEmptyComplNum(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2021, 1, 15)]
		public void TestCheckIfComplianceErrors_NoPastTransactionsWithEmptyComplNum_INV()
		{
			AssertCheckIfComplianceErrors_NoPastTransactionsWithEmptyComplNum(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertCheckIfComplianceErrors_NoPastTransactionsWithEmptyComplNum(string dateOption)
		{
			var creator = new TestObjectCreator(Factory);
			var today = ZDate.Today;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var revInvType = typeof(ARInvoice);
				const string subType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				var allocRegistry = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables;

				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ITMIL";
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var sequence = creator.SetupComplianceSequence(menuPK, subType, subType + ".21-", 1, 100, 3);
				sequence.XD_StartDate = new ZDate(today.Year, today.Month, 1);
				sequence.XD_ExpiryDate = sequence.XD_StartDate.AddMonths(1).AddDays(-1);
				sequence.XD_IsActive = true;

				var inv1 = CreateInvoiceForCompliance(creator, revInvType, "INV1", sequence.XD_StartDate, subType, sequence, subType + ".21-0001", complianceNumberAllocationDateOption: dateOption);
				var inv2 = CreateInvoiceForCompliance(creator, revInvType, "INV2", today.AddDays(-5), subType, sequence, complianceNumberAllocationDateOption: dateOption);
				var inv3 = CreateInvoiceForCompliance(creator, revInvType, "INV3", today, subType, sequence, subType + ".21-0002", complianceNumberAllocationDateOption: dateOption);

				Factory.Save();

				var reverseInvoice = CreateInvoiceForCompliance(creator, revInvType, "1000", today.AddDays(5), subType, sequence, complianceNumberAllocationDateOption: dateOption);
				var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(reverseInvoice);

				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (allocRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					Assert("No Compliance errors", !TestObjectForHeader.CheckHasComplianceSubTypeAndNumberingErrors());
					TestObjectForHeader.TransactionsAlreadyReversed.Add(wrapper);
					Assert("Compliance errors", TestObjectForHeader.CheckHasComplianceSubTypeAndNumberingErrors());
					Assert(reverseInvoice.HasErrors);
					var allocationDate = dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code ? reverseInvoice.AH_InvoiceDate : reverseInvoice.AH_PostDate;
					Assert(reverseInvoice.RowErrors.Contains(string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(dateOption),
						subType, allocationDate.ToShortDateString())));
				}
			}
		}

		[TestDate(2021, 1, 10)]
		public void TestCheckIfComplianceErrors_BookIsFullOrExpired_PST()
		{
			var creator = new TestObjectCreator(Factory);
			var today = ZDate.Today;
			var dateOption = AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				var revInvType = typeof(APInvoice);
				const string subType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
				var allocRegistry = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables;

				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ITMIL";
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var alterSeq = creator.SetupComplianceSequence(menuPK, subType, subType + ".21-", 1, 100, 2);
				alterSeq.XD_StartDate = new ZDate(today.Year, today.Month, 1);
				alterSeq.XD_ExpiryDate = alterSeq.XD_StartDate.AddMonths(1).AddDays(-2);
				alterSeq.XD_IsActive = true;

				var fullSeq = creator.SetupComplianceSequence(menuPK, subType, subType + ".21_", 1, 100, 101);
				fullSeq.XD_StartDate = alterSeq.XD_StartDate.AddMonths(1);
				fullSeq.XD_ExpiryDate = fullSeq.XD_StartDate.AddMonths(1).AddDays(-1);
				fullSeq.XD_IsActive = true;

				Factory.Save();

				var revInvAlter = CreateInvoiceForCompliance(creator, revInvType, "1000", alterSeq.XD_ExpiryDate.Date.AddDays(1), subType, alterSeq, complianceNumberAllocationDateOption: dateOption);
				var wrapperAlter = new IReversingImplicitlyImplementedWrapperForBinding(revInvAlter);

				var revInvFull = CreateInvoiceForCompliance(creator, revInvType, "1001", fullSeq.XD_StartDate, subType, alterSeq, complianceNumberAllocationDateOption: dateOption);
				var wrapperFull = new IReversingImplicitlyImplementedWrapperForBinding(revInvFull);

				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (allocRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					Assert("No Compliance errors", !TestObjectForHeader.CheckHasComplianceSubTypeAndNumberingErrors());
					TestObjectForHeader.TransactionsAlreadyReversed.Add(wrapperAlter);
					Assert("Compliance errors", TestObjectForHeader.CheckHasComplianceSubTypeAndNumberingErrors());
					Assert("Compliance Book expired", revInvAlter.HasErrors);
					Assert(revInvAlter.RowErrors.Contains(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage));

					TestObjectForHeader.TransactionsAlreadyReversed.RemoveAndDeleteAll();
					Assert("No Compliance errors", !TestObjectForHeader.CheckHasComplianceSubTypeAndNumberingErrors());
					TestObjectForHeader.TransactionsAlreadyReversed.Add(wrapperFull);
					Assert("Compliance errors", TestObjectForHeader.CheckHasComplianceSubTypeAndNumberingErrors());
					Assert("Compliance Book full", revInvFull.HasErrors);
					Assert(revInvFull.RowErrors.Contains(ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage));
				}
			}
		}

		[TestDate(2021, 1, 15)]
		public void TestCheckIfComplianceErrors_PostDateEarlierThanLastDateUsed_PST()
		{
			AssertCheckIfComplianceErrors_PostDateEarlierThanLastDateUsed(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2021, 1, 15)]
		public void TestCheckIfComplianceErrors_PostDateEarlierThanLastDateUsed_INV()
		{
			AssertCheckIfComplianceErrors_PostDateEarlierThanLastDateUsed(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertCheckIfComplianceErrors_PostDateEarlierThanLastDateUsed(string dateOption)
		{
			var creator = new TestObjectCreator(Factory);
			var today = ZDate.Today;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var revInvType = typeof(ARInvoice);
				const string subType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				var allocRegistry = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables;

				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ITMIL";
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var complSeq = creator.SetupComplianceSequence(menuPK, subType, subType + ".21-", 1, 100, 3);
				complSeq.XD_StartDate = new ZDate(today.Year, today.Month, 1);
				complSeq.XD_ExpiryDate = complSeq.XD_StartDate.AddMonths(1).AddDays(-1);
				complSeq.XD_IsActive = true;

				var lastDateUsed = today.AddDays(-5);
				var inv1AllocationDate = lastDateUsed.AddDays(-5);
				var inv1 = CreateInvoiceForCompliance(creator, revInvType, "INV1", inv1AllocationDate, subType, complSeq, subType + ".21-0001", complianceNumberAllocationDateOption: dateOption);
				var inv2 = CreateInvoiceForCompliance(creator, revInvType, "INV2", lastDateUsed, subType, complSeq, subType + ".21-0002", complianceNumberAllocationDateOption: dateOption);

				Factory.Save();

				var reverseInvoice = CreateInvoiceForCompliance(creator, revInvType, "1000", inv1AllocationDate, subType, complSeq, complianceNumberAllocationDateOption: dateOption);
				var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(reverseInvoice);

				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (allocRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					Assert("No Compliance errors", !TestObjectForHeader.CheckHasComplianceSubTypeAndNumberingErrors());
					TestObjectForHeader.TransactionsAlreadyReversed.Add(wrapper);
					Assert("Compliance errors", TestObjectForHeader.CheckHasComplianceSubTypeAndNumberingErrors());
					Assert(reverseInvoice.HasErrors);
					Assert(reverseInvoice.RowErrors.Contains(string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(dateOption),
						subType, lastDateUsed.ToShortDateString())));
				}
			}
		}

		public void TestHasClosedJobAndReOpenClosedJob()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = JobHeaderStatus.Working.Code;
			job1.JH_JobNum = "TESTJOB1";
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_Status = JobHeaderStatus.Closed.Code;
			job2.JH_JobNum = "TESTJOB2";
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_Status = JobHeaderStatus.Working.Code;
			job3.JH_JobNum = "TESTJOB3";

			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AddRelatedJobsForReversing_ForTestOnly(job1);
			var wrapper1 = new IReversingImplicitlyImplementedWrapperForBinding(invoice1);

			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AddRelatedJobsForReversing_ForTestOnly(job2);
			invoice2.AddRelatedJobsForReversing_ForTestOnly(job3);
			var wrapper2 = new IReversingImplicitlyImplementedWrapperForBinding(invoice2);

			AssertEquals("HasClosedJob", false, TestObjectForHeader.HasClosedJob);

			TestObjectForHeader.TransactionsAlreadyReversed.Add(wrapper1);
			AssertEquals("HasClosedJob", false, TestObjectForHeader.HasClosedJob);

			TestObjectForHeader.TransactionsAlreadyReversed.Add(wrapper2);
			AssertEquals("HasClosedJob", true, TestObjectForHeader.HasClosedJob);

			TestObjectForHeader.ReOpenClosedJob();
			AssertEquals("job2 must be reopen", JobHeaderStatus.Working.Code, job2.JH_Status);
		}

		public void TestNoErrorWhenReversingNonInvoiceTransactions()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var receipt1 = testObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 20M, testObjectCreator.AUDBankAccount.PK);
			var invoice1 = testObjectCreator.CreateARInvoice<ARInvoice>("001", testObjectCreator.AUD, 1M, testObjectCreator.LocalClient);

			var job1 = testObjectCreator.CreateJob("S001", testObjectCreator.LocalClient, 0M, testObjectCreator.Agent, 0M);
			job1.JH_Status = JobHeaderStatus.Closed.Code;

			invoice1.AddRelatedJobsForReversing_ForTestOnly(job1);

			var wrapper1 = new IReversingImplicitlyImplementedWrapperForBinding(invoice1);
			var wrapper2 = new IReversingImplicitlyImplementedWrapperForBinding(receipt1);

			TestObjectForHeader.TransactionsAlreadyReversed.Add(wrapper2);
			TestObjectForHeader.TransactionsAlreadyReversed.Add(wrapper1);

			try
			{
				AssertEquals("The collection of transactions should have a closed job", true, TestObjectForHeader.HasClosedJob);
			}
			catch (InvalidCastException)
			{
				Assert("There should be no InvalidCastExceptions thrown", false);
			}
		}

		MultipleReversingProviderForHeader TestObjectForHeader => (MultipleReversingProviderForHeader)TestObject;

		#region Overriden Members

		protected override MultipleReversingProviderBase GetNewTestObject() => new MultipleReversingProviderForHeader();

		protected override BusinessObject GetCurrentBusinessObjectForIEnumeratorTesting() => TestObjectForHeader.Current;

		#endregion
	}

	[TestedType(typeof(MultipleReversingProviderForHeader))]
	public class MultipleReversingProviderIUnmatchDateSupporterTest : BaseITransactionTestCase
	{
		protected override ITransaction GetNewObject() => new MultipleReversingProviderForHeader();
	}
}
