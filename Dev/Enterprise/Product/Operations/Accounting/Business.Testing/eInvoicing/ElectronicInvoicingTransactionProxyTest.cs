using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using AuthRecordConstants = Enterprise.Accounting.Integration.DataTransferConstants.AccTransactionHeaderAuthorisationRecord;

namespace Enterprise.Accounting.Business.eInvoicing.Testing
{
	public class ElectronicInvoicingTransactionProxyTest : TestCaseWithFactory
	{
		public void TestIdentityProperties()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "A1234";
			var proxy = new ElectronicInvoicingTransactionProxy(transaction);
			AssertEquals(transaction.PK, proxy.PK);
			AssertEquals("AR INV A1234", proxy.UniqueIdentifier);

			var transaction2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction2.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction2.AH_TransactionType = TransactionTypes.CreditNote;
			transaction2.AH_TransactionNum = "B6789";
			var proxy2 = new ElectronicInvoicingTransactionProxy(transaction2);
			AssertEquals(transaction2.PK, proxy2.PK);
			AssertEquals("AP CRD B6789", proxy2.UniqueIdentifier);
		}

		public void TestPivotProperties()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			var now = ZDateTime.UtcNow;

			pivot.AIP_LastResponseReceivedUtc = now;
			pivot.AIP_LastSentTimeUtc = now.AddMinutes(-1);
			pivot.AIP_Status = EInvoicingPivotState.Batched;
			pivot.AIP_ErrorDescription = "Some error";
			var proxy = new ElectronicInvoicingTransactionProxy(transaction);
			CombineAssertions(() =>
			{
				AssertEquals(EInvoicingPivotState.Batched, proxy.CurrentStatus);
				AssertEquals("Some error", proxy.CurrentError);
				AssertEquals(now, proxy.LastResponseReceivedUtc);
				AssertEquals(now.AddMinutes(-1), proxy.LastSentTimeUtc);
			});

			pivot.AIP_Status = EInvoicingPivotState.Discarded;
			proxy = new ElectronicInvoicingTransactionProxy(transaction);
			CombineAssertions(() =>
			{
				AssertEquals(EInvoicingPivotState.Discarded, proxy.CurrentStatus);
				AssertEquals("Some error", proxy.CurrentError);
				AssertEquals(now, proxy.LastResponseReceivedUtc);
				AssertEquals(now.AddMinutes(-1), proxy.LastSentTimeUtc);
			});

			pivot.AIP_ParentID = ZGuid.Empty;
			proxy = new ElectronicInvoicingTransactionProxy(transaction);
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, proxy.CurrentStatus);
				AssertEquals(ZString.Empty, proxy.CurrentError);
				AssertEquals(ZDateTime.Empty, proxy.LastResponseReceivedUtc);
				AssertEquals(ZDateTime.Empty, proxy.LastSentTimeUtc);
			});
		}

		public void TestBatchProperties()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();

			MockAndSetICountryComplianceFactory("", AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber);
			using (ObjectFactory.Substitute(CountryComplianceFactoryMock.Object))
			{
				pivot.AIP_Status = EInvoicingPivotState.Batched;
				pivot.AIP_AIB = batch.PK;
				batch.AIB_BatchNumber = 42;
				batch.AIB_EHubAllocatedNumber = "some very unique number";
				batch.AIB_GovernmentAllocatedNumber = "ABC9999";
				var proxy = new ElectronicInvoicingTransactionProxy(transaction);
				CombineAssertions(() =>
				{
					AssertEquals("42", proxy.BatchNumber);
					AssertEquals("some very unique number", proxy.eHubAllocatedNumber);
					AssertEquals("ABC9999", proxy.GovernmentAllocatedNumber);
				});

				pivot.AIP_AIB = ZGuid.Empty;
				proxy = new ElectronicInvoicingTransactionProxy(transaction);
				CombineAssertions(() =>
				{
					AssertEquals(ZString.Empty, proxy.BatchNumber);
					AssertEquals(ZString.Empty, proxy.eHubAllocatedNumber);
					AssertEquals(ZString.Empty, proxy.GovernmentAllocatedNumber);
				});

				pivot.AIP_AIB = batch.PK;
				pivot.AIP_Status = EInvoicingPivotState.Discarded;
				proxy = new ElectronicInvoicingTransactionProxy(transaction);
				CombineAssertions(() =>
				{
					AssertEquals("42", proxy.BatchNumber);
					AssertEquals("some very unique number", proxy.eHubAllocatedNumber);
					AssertEquals("ABC9999", proxy.GovernmentAllocatedNumber);
				});

				pivot.AIP_ParentID = ZGuid.Empty;
				proxy = new ElectronicInvoicingTransactionProxy(transaction);
				CombineAssertions(() =>
				{
					AssertEquals(ZString.Empty, proxy.BatchNumber);
					AssertEquals(ZString.Empty, proxy.eHubAllocatedNumber);
					AssertEquals(ZString.Empty, proxy.GovernmentAllocatedNumber);
				});
			}
		}

		public void TestGovernmentAllocatedNumber()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_GovernmentAllocatedID = "HIJ6666";
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			pivot.AIP_Status = EInvoicingPivotState.Batched;
			pivot.AIP_AIB = batch.PK;

			batch.AIB_GovernmentAllocatedNumber = "ABC9999";
			MockAndSetICountryComplianceFactory("", AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber);
			using (ObjectFactory.Substitute(CountryComplianceFactoryMock.Object))
			{
				var proxy = new ElectronicInvoicingTransactionProxy(transaction);
				AssertEquals("ABC9999", proxy.GovernmentAllocatedNumber);
			}

			MockAndSetICountryComplianceFactory("", AccTransactionHeaderSchema.Constants.AH_GovernmentAllocatedID);
			using (ObjectFactory.Substitute(CountryComplianceFactoryMock.Object))
			{
				var proxy = new ElectronicInvoicingTransactionProxy(transaction);
				AssertEquals("HIJ6666", proxy.GovernmentAllocatedNumber);
			}

			var authRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_RecordType = "XYZ";
			authRecord.AHF_ParentId = transaction.PK;
			authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authRecord.AHF_Number = "EFG7777";
			MockAndSetICountryComplianceFactory("XYZ", AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number);
			using (ObjectFactory.Substitute(CountryComplianceFactoryMock.Object))
			{
				var proxy = new ElectronicInvoicingTransactionProxy(transaction);
				AssertEquals("EFG7777", proxy.GovernmentAllocatedNumber);

				authRecord.AHF_Number = ZString.Empty;
				AssertEquals(ZString.Empty, proxy.GovernmentAllocatedNumber);

				authRecord.AHF_Number = "EFG7777";
				authRecord.AHF_RecordType = "NOT";
				proxy = new ElectronicInvoicingTransactionProxy(transaction);
				AssertEquals(ZString.Empty, proxy.GovernmentAllocatedNumber);
			}

			MockAndSetICountryComplianceFactory("NOT", AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_ITransactionHash);
			using (ObjectFactory.Substitute(CountryComplianceFactoryMock.Object))
			{
				var proxy = new ElectronicInvoicingTransactionProxy(transaction);
				AssertEquals(ZString.Empty, proxy.GovernmentAllocatedNumber);
			}
		}

		public void TestAuthorisationDateTime()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			var testTime = ZDateTimeOffset.Now.AddSeconds(-15);

			var proxy = new ElectronicInvoicingTransactionProxy(transaction);
			Assert("Empty when no Authorisation record", proxy.AuthorisationDateTime.IsEmpty);

			MockAndSetICountryComplianceFactory("XYZ");
			using (ObjectFactory.Substitute(CountryComplianceFactoryMock.Object))
			{
				var authRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
				authRecord.AHF_RecordType = "XYZ";
				authRecord.AHF_ParentId = transaction.PK;
				authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				authRecord.AHF_Number = "KFC123";

				authRecord.AHF_DateTime = ZDateTimeOffset.Empty;
				proxy = new ElectronicInvoicingTransactionProxy(transaction);
				Assert("Empty when Authorisation record has empty value", proxy.AuthorisationDateTime.IsEmpty);

				authRecord.AHF_DateTime = AuthRecordConstants.NullPlaceholderForDateTime;
				proxy = new ElectronicInvoicingTransactionProxy(transaction);
				Assert("Empty when Authorisation record has legacy null placeholder value", proxy.AuthorisationDateTime.IsEmpty);

				authRecord.AHF_DateTime = testTime;
				proxy = new ElectronicInvoicingTransactionProxy(transaction);
				AssertEquals(testTime, proxy.AuthorisationDateTime);

				authRecord.AHF_RecordType = "NOT";
				proxy = new ElectronicInvoicingTransactionProxy(transaction);
				Assert("Empty when Authorisation record has non-compliant type", proxy.AuthorisationDateTime.IsEmpty);
			}
		}

		public void TestDiscardedStatus_IsVisible_UntilAnotherPivotIsCreated()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			var now = ZDateTime.UtcNow;

			pivot.AIP_LastSentTimeUtc = now.AddMinutes(-30);
			pivot.AIP_LastResponseReceivedUtc = now.AddMinutes(-20);
			pivot.AIP_Status = EInvoicingPivotState.Batched;
			pivot.AIP_ErrorDescription = "Some error";
			var proxy = new ElectronicInvoicingTransactionProxy(transaction);
			CombineAssertions(() =>
			{
				AssertEquals(EInvoicingPivotState.Batched, proxy.CurrentStatus);
				AssertEquals("Some error", proxy.CurrentError);
				AssertEquals(now.AddMinutes(-30), proxy.LastSentTimeUtc);
				AssertEquals(now.AddMinutes(-20), proxy.LastResponseReceivedUtc);
			});

			pivot.AIP_Status = EInvoicingPivotState.Discarded;
			proxy = new ElectronicInvoicingTransactionProxy(transaction);
			CombineAssertions(() =>
			{
				AssertEquals(EInvoicingPivotState.Discarded, proxy.CurrentStatus);
				AssertEquals("Some error", proxy.CurrentError);
				AssertEquals(now.AddMinutes(-30), proxy.LastSentTimeUtc);
				AssertEquals(now.AddMinutes(-20), proxy.LastResponseReceivedUtc);
			});

			var secondPivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			secondPivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			secondPivot.AIP_ParentID = transaction.PK;
			secondPivot.AIP_Status = EInvoicingPivotState.Queued;
			secondPivot.AIP_ErrorDescription = "Some different message";
			secondPivot.AIP_LastSentTimeUtc = now.AddMinutes(-10);
			secondPivot.AIP_LastResponseReceivedUtc = now.AddMinutes(-1);
			proxy = new ElectronicInvoicingTransactionProxy(transaction);
			CombineAssertions(() =>
			{
				AssertEquals(EInvoicingPivotState.Queued, proxy.CurrentStatus);
				AssertEquals("Some different message", proxy.CurrentError);
				AssertEquals(now.AddMinutes(-10), proxy.LastSentTimeUtc);
				AssertEquals(now.AddMinutes(-1), proxy.LastResponseReceivedUtc);
			});
		}

		public void TestActionTypeAmend()
		{
			AssertActionTypeAmend(false, EInvoicingPivotActionType.Submit);
			AssertActionTypeAmend(true, EInvoicingPivotActionType.Amend);

			void AssertActionTypeAmend(bool waitForOriginal, string expectedActionType)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.USD, 1m, TestObjectCreator.DebtorTR);
				var arCreditNote = (ARCreditNote)(TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice).amendTransaction);
				var arDebitNote = (ARInvoice)(TestObjectCreator.AmendARTransaction(TransactionTypes.Invoice, arInvoice).amendTransaction);

				var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("0001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
				var apCreditNote = (APCreditNote)(TestObjectCreator.AmendAPTransaction(apInvoice).amendTransaction);

				var batchStrategyMock = new Mock<IBatchCreatorStrategy>();
				batchStrategyMock.Setup(x => x.SupportsWaitForOriginalTransactionForAmending).Returns(waitForOriginal);

				var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
				globalFactoryMock.Setup(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>())).Returns(batchStrategyMock.Object);

				var registryTestDate = ZDateTime.Now.AddDays(-1).ToDateTime();
				var registryInstance = AccountingMasterFilesRegistry.Instance;
				using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
				using (ObjectFactory.Substitute(globalFactoryMock.Object))
				using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
				{
					using (registryInstance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
					using (registryInstance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
					{
						var proxy = new ElectronicInvoicingTransactionProxy(arCreditNote);
						proxy.EvaluateEligibilityAndQueue();
						AssertEquals("ActionType for credit note of receivables transactions", expectedActionType, proxy.MostRecentPivot.AIP_ActionType);

						proxy = new ElectronicInvoicingTransactionProxy(arDebitNote);
						proxy.EvaluateEligibilityAndQueue();
						AssertEquals("ActionType for debit note of receivables transactions", expectedActionType, proxy.MostRecentPivot.AIP_ActionType);
					}
					using (registryInstance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
					using (registryInstance.EReportingComplianceDateForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
					{
						var proxy = new ElectronicInvoicingTransactionProxy(apCreditNote);
						proxy.EvaluateEligibilityAndQueue();
						AssertEquals("ActionType for amending payables transactions", expectedActionType, proxy.MostRecentPivot.AIP_ActionType);
					}
				}
			}
		}

		[TestDate(2022, 03, 03)]
		public void TestActionType_KoreaSouth_AllAreSubmit()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			GlbCompany.CurrentCompany.Factory.Save();

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.USD, 1m, TestObjectCreator.DebtorTR);
			var arCreditNote = (ARCreditNote)(TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice).amendTransaction);
			var arDebitNote = (ARInvoice)(TestObjectCreator.AmendARTransaction(TransactionTypes.Invoice, arInvoice).amendTransaction);

			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.USD, 1m, TestObjectCreator.DebtorTR);
			var arReversal = TestObjectCreator.ReverseTransaction(arInvoice, out var error) as TransactionHeader;

			Factory.Save();

			AssertActionTypeSubmit(arInvoice);
			AssertActionTypeSubmit(arCreditNote);
			AssertActionTypeSubmit(arDebitNote);
			AssertActionTypeSubmit(arInvoice2);
			AssertActionTypeSubmit(arReversal);

			void AssertActionTypeSubmit(TransactionHeader invoice)
			{
				var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
				globalFactoryMock.Setup(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>())).Returns(() => null);

				var registryTestDate = ZDateTime.Now.AddDays(-1).ToDateTime();
				var registryInstance = AccountingMasterFilesRegistry.Instance;
				using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
				using (ObjectFactory.Substitute(globalFactoryMock.Object))
				using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
				{
					using (registryInstance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
					using (registryInstance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
					{
						var proxy = new ElectronicInvoicingTransactionProxy(invoice);
						proxy.EvaluateEligibilityAndQueue();
						AssertEquals($"ActionType for {invoice.AH_TransactionType} of receivables transactions", EInvoicingPivotActionType.Submit, proxy.MostRecentPivot.AIP_ActionType);

						proxy = new ElectronicInvoicingTransactionProxy(arDebitNote);
						proxy.EvaluateEligibilityAndQueue();
						AssertEquals("ActionType for debit note of receivables transactions", EInvoicingPivotActionType.Submit, proxy.MostRecentPivot.AIP_ActionType);
					}
				}
			}
		}

		[TestDate(2022, 03, 03)]
		public void TestTransactionPivotCreated()
		{
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
			{
				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
				{
					var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.USD, 1m, TestObjectCreator.DebtorTR);
					Factory.Save();

					var pivots = Factory.Load<AccEInvoicingTransactionPivot>(GetPivotQueryForTransaction(arInvoice.PK));

					AssertEquals(1, pivots.Length);

					AssertEquals(Env.CurrentCompanyPK, pivots[0].AIP_GC);
					AssertEquals(arInvoice.PK, pivots[0].AIP_ParentID);
					AssertEquals(AccTransactionHeaderSchema.Constants.Prefix, pivots[0].AIP_ParentTableCode);
				}
				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
				{
					var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("0002", TestObjectCreator.USD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.CreditorTR);
					Factory.Save();

					var pivots = Factory.Load<AccEInvoicingTransactionPivot>(GetPivotQueryForTransaction(apInvoice.PK));

					AssertEquals(1, pivots.Length);
					AssertEquals(Env.CurrentCompanyPK, pivots[0].AIP_GC);
					AssertEquals(apInvoice.PK, pivots[0].AIP_ParentID);
					AssertEquals(AccTransactionHeaderSchema.Constants.Prefix, pivots[0].AIP_ParentTableCode);
				}

				var totalPivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()).Length;
				AssertEquals(2, totalPivots);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0003", TestObjectCreator.USD, 1m, TestObjectCreator.DebtorTR);
				var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("0004", TestObjectCreator.USD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.CreditorTR);
				Factory.Save();

				var totalPivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()).Length;
				AssertEquals(2, totalPivots);
			}
		}

		[TestDate(2022, 03, 03)]
		public void TestTransactionPivotCreatedWhenTransactionCreatedForDifferentCompanyThanLoginCompany()
		{
			var currentCompany = Env.CurrentCompany;
			var otherCompany = TestObjectCreator.CreateNewCompany("XYZ");
			var otherBranch = TestObjectCreator.CreateNewBranch(otherCompany, "XYB");

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(currentCompany.PK, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(currentCompany.PK, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
			{
				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
				{
					var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), "0001", TestObjectCreator.USD, 1m);
					arInvoice.AH_OH = TestObjectCreator.DebtorTR.PK;
					arInvoice.AH_GC = otherCompany.PK;
					arInvoice.AH_GB = otherBranch.PK;
					Factory.Save();

					var pivots = Factory.Load<AccEInvoicingTransactionPivot>(GetPivotQueryForTransaction(arInvoice.PK));

					AssertEquals(1, pivots.Length);
					AssertEquals(otherCompany.PK, pivots[0].AIP_GC);
					AssertEquals(arInvoice.PK, pivots[0].AIP_ParentID);
					AssertEquals(AccTransactionHeaderSchema.Constants.Prefix, pivots[0].AIP_ParentTableCode);
				}

				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables.SetTemporaryValue(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
				{
					var apInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.USD, 1m);
					apInvoice.AH_TransactionNum = "0002";
					apInvoice.AH_GC = otherCompany.PK;
					apInvoice.AH_GB = otherBranch.PK;
					Factory.Save();

					var pivots = Factory.Load<AccEInvoicingTransactionPivot>(GetPivotQueryForTransaction(apInvoice.PK));

					AssertEquals(1, pivots.Length);
					AssertEquals(otherCompany.PK, pivots[0].AIP_GC);
					AssertEquals(apInvoice.PK, pivots[0].AIP_ParentID);
					AssertEquals(AccTransactionHeaderSchema.Constants.Prefix, pivots[0].AIP_ParentTableCode);
				}
			}

			var totalPivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()).Length;
			AssertEquals(2, totalPivots);
		}

		[TestDate(2022, 03, 03)]
		public void TestHasEReportingComplianceDateReached()
		{
			TestDateAttribute.UseUNLOCO = false;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.USD, 1m, TestObjectCreator.DebtorTR);
			arInvoice.AH_PostDate = new ZDateTime(2023, 01, 10, 00, 00, 00);
			Factory.Save();

			AssertEquals("PreCondition, AH_PostDate"
				, new ZDateTime(2023, 1, 10, 0, 0, 0)
				, arInvoice.AH_PostDate
			);
			AssertEquals("PreCondition, AH_SystemCreateTimeUtc"
				, new ZDateTime(2022, 03, 03)
				, arInvoice.AH_SystemCreateTimeUtc
			);

			var mockGlobalCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithNullResultFromInstanceProvider_Dangerous<IEInvoicingPreEligibilityProvider>();

			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				var proxy = new ElectronicInvoicingTransactionProxy(arInvoice);

				CombineAssertions("HasEReportingComplianceDateReached, comparing AH_PostDate when IEInvoicingPreEligibilityProvider is not implemented.", () =>
				{
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 01, 09)))
					{
						AssertEquals("When AH_PostDate(2023-01-10) is greater than EReportingComplianceDate(2023-01-09).", true, proxy.HasEReportingComplianceDateReached());
					}
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 01, 10)))
					{
						AssertEquals("When AH_PostDate(2023-01-10) is equaled than EReportingComplianceDate(2023-01-10).", true, proxy.HasEReportingComplianceDateReached());
					}
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 01, 11)))
					{
						AssertEquals("When AH_PostDate(2023-01-10) is less than EReportingComplianceDate(2023-01-11).", false, proxy.HasEReportingComplianceDateReached());
					}
				});
			}
		}

		[TestDate(2022, 03, 03)]
		public void TestCanEvaluateByComplianceDate_PreEligibilityProvider()
		{
			TestDateAttribute.UseUNLOCO = false;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.USD, 1m, TestObjectCreator.DebtorTR);
			arInvoice.AH_PostDate = new ZDateTime(2023, 01, 10, 00, 00, 00);
			Factory.Save();

			AssertEquals("PreCondition, AH_PostDate"
				, new ZDateTime(2023, 1, 10, 0, 0, 0)
				, arInvoice.AH_PostDate
			);
			AssertEquals("PreCondition, AH_SystemCreateTimeUtc"
				, new ZDateTime(2022, 03, 03)
				, arInvoice.AH_SystemCreateTimeUtc
			);

			var mockGlobalCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithEInvoicingPreEligibilityProvider(canEvaluateByComplianceDate: true, canEvaluateByTransaction: true, preEligibilityProvider: out var mockPreEligibilityProvider)
				.ToGlobalFactory();

			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				var proxy = new ElectronicInvoicingTransactionProxy(arInvoice);

				CombineAssertions("CanEvaluateByComplianceDate, comparing via IEInvoicingPreEligibilityProvider when it is implemented.", () =>
				{
					mockPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(arInvoice, It.IsAny<DateTime>()), Times.Exactly(0));

					var complianceDate20230109 = new DateTime(2023, 01, 09);
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, complianceDate20230109))
					{
						AssertEquals(true, proxy.HasEReportingComplianceDateReached());
						mockPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(arInvoice, complianceDate20230109), Times.Exactly(1));
						mockPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(arInvoice, It.IsAny<DateTime>()), Times.Exactly(1));
					}

					var complianceDate20230110 = new DateTime(2023, 01, 10);
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, complianceDate20230110))
					{
						AssertEquals(true, proxy.HasEReportingComplianceDateReached());
						mockPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(arInvoice, complianceDate20230110), Times.Exactly(1));
						mockPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(arInvoice, It.IsAny<DateTime>()), Times.Exactly(2));
					}

					var complianceDate20230111 = new DateTime(2023, 01, 11);
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, complianceDate20230111))
					{
						AssertEquals(true, proxy.HasEReportingComplianceDateReached());
						mockPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(arInvoice, complianceDate20230111), Times.Exactly(1));
						mockPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(arInvoice, It.IsAny<DateTime>()), Times.Exactly(3));
					}
				});
			}
		}

		public void TestCanEvaluateByTransaction_PreEligibilityProvider()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "A1235";
			transaction.AH_PostDate = ZDateTime.Now;

			Factory.Save();

			var mockGlobalCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithEInvoicingPreEligibilityProvider(canEvaluateByComplianceDate: true, canEvaluateByTransaction: true, preEligibilityProvider: out var mockPreEligibilityProvider)
				.ToGlobalFactory();

			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				var proxy = new ElectronicInvoicingTransactionProxy(transaction);

				CombineAssertions("CanEvaluateByTransaction, comparing via IEInvoicingPreEligibilityProvider when it is implemented.", () =>
				{
					mockPreEligibilityProvider.Verify(x => x.CanEvaluateByTransaction(transaction), Times.Exactly(0));
					AssertEquals(true, proxy.CanEvaluateEligibility());
					mockPreEligibilityProvider.Verify(x => x.CanEvaluateByTransaction(transaction), Times.Exactly(1));
				});
			}
		}

		#region TestIsPreEInvoicingTransaction

		public void TestIsPreEInvoicingTransaction_ReturnsFalse_WhenCreatedAfterComplianceDate()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			using (registry.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
				transaction.AH_PostDate = ZDateTime.Now;
				var proxy = new ElectronicInvoicingTransactionProxy(transaction);

				AssertEquals(false, proxy.IsPreEInvoicingTransaction());
			}
		}

		public void TestIsPreEInvoicingTransaction_ReturnsTrue_WhenCreatedBeforeComplianceDate()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			using (registry.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(10).ToDateTime()))
			{
				var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
				transaction.AH_PostDate = ZDateTime.Now;
				var proxy = new ElectronicInvoicingTransactionProxy(transaction);

				AssertEquals(true, proxy.IsPreEInvoicingTransaction());
			}
		}

		public void TestIsPreEInvoicingTransaction_ReturnsTrue_WhenCreatedAfterComplianceDate_AndHasSpecialDCDPivot()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			using (registry.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
				transaction.AH_PostDate = ZDateTime.Now;
				var proxy = new ElectronicInvoicingTransactionProxy(transaction);
				AssertEquals("Without pivot the transaction is not pre-eInvoicing", false, proxy.IsPreEInvoicingTransaction());

				var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
				pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				pivot.AIP_ParentID = transaction.PK;
				pivot.AIP_Status = EInvoicingPivotState.Discarded;
				pivot.AIP_ErrorDescription = "Some other error message";
				AssertEquals("Without special error message the transaction is not pre-eInvoicing", false, proxy.IsPreEInvoicingTransaction());

				pivot.AIP_ErrorDescription = AccEInvoicingTransactionPivot.ErrorDescriptionWhenEInvoicingDisabled();
				AssertEquals("With DCD pivot and special error message, the transaction should be pre-eInvoicing", true, proxy.IsPreEInvoicingTransaction());
			}
		}

		[TestDate(2018, 7, 16, 15, 26, 32)]
		public void TestIsPreEInvoicingTransaction_ForVariousDates()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			AssertEquals("Precondition: when registry EReportingComplianceDate is empty, it returns the DateTime.MinValue", DateTime.MinValue,
						registry.EReportingComplianceDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_PostDate = ZDateTime.UtcNow;
			var proxy = new ElectronicInvoicingTransactionProxy(transaction);
			AssertEquals("if registry is not set, then a transaction is Before EInvoicing", true, proxy.IsPreEInvoicingTransaction());

			using (registry.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2018, 7, 12)))
			{
				transaction.AH_PostDate = new ZDateTime(2018, 7, 11);
				AssertEquals(true, proxy.IsPreEInvoicingTransaction());

				transaction.AH_PostDate = new ZDateTime(2018, 7, 12);
				AssertEquals(false, proxy.IsPreEInvoicingTransaction());

				transaction.AH_PostDate = new ZDateTime(2018, 7, 13);
				AssertEquals(false, proxy.IsPreEInvoicingTransaction());
			}
		}

		#endregion

		#region TestGetInitialPivotStatusForKoreaSouth

		public void TestGetInitialPivotStatusForKoreaSouth_ForARCreditNote_GeneratedByReverse()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			var defaultPivotStatus = EInvoicingPivotState.Queued;

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultPivotStatus))
			{
				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.KRW, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice1.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.KRW, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice2.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				arInvoice1.GetMostRecentEInvoicingTransactionPivot().AIP_Status = EInvoicingPivotState.Delivered;
				arInvoice2.GetMostRecentEInvoicingTransactionPivot().AIP_Status = EInvoicingPivotState.Succeed;
				Factory.Save();

				new ReversingFactory().NewReversing(arInvoice1).Reverse();
				var reverseTransactionForARInvoice1 = arInvoice1.ReverseInvoice;
				new ReversingFactory().NewReversing(arInvoice2).Reverse();
				var reverseTransactionForARInvoice2 = arInvoice2.ReverseInvoice;
				Factory.Save();

				AssertNotNull("Precondition", reverseTransactionForARInvoice1.OriginalTransaction);
				AssertEquals("Precondition", true, reverseTransactionForARInvoice1.IsARCreditNote);
				AssertEquals("Precondition", false, (reverseTransactionForARInvoice1.OriginalTransaction as InvoicingBase).IsApprovedByGovt);
				AssertEquals(EInvoicingPivotState.Pending, reverseTransactionForARInvoice1.GetMostRecentEInvoicingTransactionPivot().AIP_Status);

				AssertNotNull("Precondition", reverseTransactionForARInvoice2.OriginalTransaction);
				AssertEquals("Precondition", true, reverseTransactionForARInvoice2.IsARCreditNote);
				AssertEquals("Precondition", true, (reverseTransactionForARInvoice2.OriginalTransaction as InvoicingBase).IsApprovedByGovt);
				AssertEquals(EInvoicingPivotState.Queued, reverseTransactionForARInvoice2.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
			}
		}

		public void TestGetInitialPivotStatusForKoreaSouth_ForARCreditNote_GeneratedByAmend()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			var defaultPivotStatus = EInvoicingPivotState.Queued;

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultPivotStatus))
			{
				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.KRW, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice1.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.KRW, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice2.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				arInvoice1.GetMostRecentEInvoicingTransactionPivot().AIP_Status = EInvoicingPivotState.Delivered;
				arInvoice2.GetMostRecentEInvoicingTransactionPivot().AIP_Status = EInvoicingPivotState.Succeed;
				Factory.Save();

				var amendTransactionForARInvoice1 = TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice1).amendTransaction as InvoicingBase;
				var amendTransactionForARInvoice2 = TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice2).amendTransaction as InvoicingBase;
				Factory.Save();

				AssertNotNull("Precondition", amendTransactionForARInvoice1.OriginalTransaction);
				AssertEquals("Precondition", true, amendTransactionForARInvoice1.IsARCreditNote);
				AssertEquals("Precondition", false, (amendTransactionForARInvoice1.OriginalTransaction as InvoicingBase).IsApprovedByGovt);
				AssertEquals(EInvoicingPivotState.Pending, amendTransactionForARInvoice1.GetMostRecentEInvoicingTransactionPivot().AIP_Status);

				AssertNotNull("Precondition", amendTransactionForARInvoice2.OriginalTransaction);
				AssertEquals("Precondition", true, amendTransactionForARInvoice2.IsARCreditNote);
				AssertEquals("Precondition", true, (amendTransactionForARInvoice2.OriginalTransaction as InvoicingBase).IsApprovedByGovt);
				AssertEquals(EInvoicingPivotState.Queued, amendTransactionForARInvoice2.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
			}
		}

		#endregion

		#region TestGetInitialPivotStatusForItaly_Payables

		[TestDate(2022, 01, 02)]
		public void TestGetInitialPivotStatusForItaly_Payables()
		{
			AssertGetInitialPivotStatusForItaly_Payables("A1234", EInvoicingPivotState.Pending);
			AssertGetInitialPivotStatusForItaly_Payables("B1234", EInvoicingPivotState.Queued);

			void AssertGetInitialPivotStatusForItaly_Payables(string transNum, string defaultPivotStatus)
			{
				var transaction = GetEligiblePayableTransactionForItalianCompliance(transNum);
				AssertEquals("Precondition: no pivots for transaction", 0, RefetchPivotStatusesForTransaction(transaction.PK).Count);

				var complianceDateForPayable = ZDateTime.Today.AddDays(-1).ToDateTime();
				var currentCompany = GlbCompany.CurrentCompany;
				var currCompanyPK = currentCompany.PK.ToGuid();
				var registryInstance = AccountingMasterFilesRegistry.Instance;

				using (currentCompany.TemporarilySetCountry(CountryCodes.Italy))
				using (registryInstance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(currCompanyPK, Guid.Empty, Guid.Empty, true))
				using (registryInstance.EReportingComplianceDateForPayables.SetTemporaryValue(currCompanyPK, Guid.Empty, Guid.Empty, complianceDateForPayable))
				using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
				using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
				using (registryInstance.EReportingSubmitPivotDefaultStatusForPayables.SetTemporaryValue(currCompanyPK, Guid.Empty, Guid.Empty, defaultPivotStatus))
				{
					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var invoice = newFactory.Load<APInvoice>(transaction.PK);
					invoice.AH_ComplianceSubType = "ZXC";
					invoice.AH_TransactionReference = "AAA9999";
					newFactory.Save();

					var actualPivotStatuses = RefetchPivotStatusesForTransaction(transaction.PK);
					var expectedPivotStatuses = new[] { defaultPivotStatus };
					AssertSequencesEqual(string.Format("One {0} pivot should be created when invoice is eligible and EReportingSubmitPivotDefaultStatusForPayables is set to {0}", defaultPivotStatus), expectedPivotStatuses, actualPivotStatuses);

					newFactory.ClearCachedValue<ZBool>(transaction.PK.ToStringKey());
				}
			}
		}

		#endregion

		#region TestEvaluateEligibilityAndQueue

		[TestDate(2023, 06, 20)]
		public void TestEvaluateEligibilityAndQueue_ComparingComplianceDate_Default()
		{
			AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued);
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 06, 10));
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 05, 10));

			var mockGlobalCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithEInvoicingEligibilityDecider(isEligible: true)
				.WithNullResultFromInstanceProvider_Dangerous<IEInvoicingPreEligibilityProvider>()
				.ToGlobalFactory();

			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				var arInvoice20230609 = CreateTransaction<ARInvoice>("AR20230609");
				arInvoice20230609.AH_PostDate = new ZDateTime(2023, 06, 09);
				Factory.Save();
				AssertPivotCreation("[AR]When AH_PostDate is less than EReportingComplianceDate(2023-06-10)", arInvoice20230609, Array.Empty<string>());

				var arInvoice20230610 = CreateTransaction<ARInvoice>("AR20230610");
				arInvoice20230610.AH_PostDate = new ZDateTime(2023, 06, 10);
				Factory.Save();
				AssertPivotCreation("[AR]When AH_PostDate equal to EReportingComplianceDate(2023-06-10)", arInvoice20230610, new[] { EInvoicingPivotState.Queued });

				var arInvoice20230611 = CreateTransaction<ARInvoice>("AR20230611");
				arInvoice20230611.AH_PostDate = new ZDateTime(2023, 06, 11);
				Factory.Save();
				AssertPivotCreation("[AR]When AH_PostDate is greater than EReportingComplianceDate(2023-06-10)", arInvoice20230611, new[] { EInvoicingPivotState.Queued });

				var apInvoice20230509 = CreateTransaction<APInvoice>("AP20230509");
				apInvoice20230509.AH_PostDate = new ZDateTime(2023, 05, 09);
				Factory.Save();
				AssertPivotCreation("[AP]When AH_PostDate is less than EReportingComplianceDateForPayables(2023-05-10)", apInvoice20230509, Array.Empty<string>());

				var apInvoice20230510 = CreateTransaction<APInvoice>("AP20230510");
				apInvoice20230510.AH_PostDate = new ZDateTime(2023, 05, 10);
				Factory.Save();
				AssertPivotCreation("[AP]When AH_PostDate equal to EReportingComplianceDateForPayables(2023-05-10)", apInvoice20230510, new[] { EInvoicingPivotState.Queued });

				var apInvoice20230511 = CreateTransaction<APInvoice>("AP20230511");
				apInvoice20230511.AH_PostDate = new ZDateTime(2023, 05, 11);
				Factory.Save();
				AssertPivotCreation("[AP]When AH_PostDate is greater than EReportingComplianceDateForPayables(2023-05-10)", apInvoice20230511, new[] { EInvoicingPivotState.Queued });
			}
		}

		[TestDate(2023, 06, 20)]
		public void TestEvaluateEligibilityAndQueue_ComparingComplianceDate_EInvoicingPreEligibilityProvider()
		{
			AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued);
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 06, 10));
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 05, 10));

			var mockGlobalCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithEInvoicingPreEligibilityProvider(preEligibilityProvider: out var mockEPreEligibilityProvider, canEvaluateByComplianceDate: true, canEvaluateByTransaction: true)
				.WithEInvoicingEligibilityDecider(isEligible: true)
				.ToGlobalFactory();

			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				mockEPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(It.IsAny<AccTransactionHeader>(), It.IsAny<DateTime>()), Times.Exactly(0));

				var arInvoice20230609 = CreateTransaction<ARInvoice>("AR20230609");
				arInvoice20230609.AH_PostDate = new ZDateTime(2023, 06, 09);
				Factory.Save();
				AssertPivotCreation("[AR]When AH_PostDate is less than EReportingComplianceDate(2023-06-10)", arInvoice20230609, new[] { EInvoicingPivotState.Queued });
				mockEPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(arInvoice20230609, new DateTime(2023, 06, 10)), Times.Exactly(1));
				mockEPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(It.IsAny<AccTransactionHeader>(), It.IsAny<DateTime>()), Times.Exactly(1));

				var arInvoice20230610 = CreateTransaction<ARInvoice>("AR20230610");
				arInvoice20230610.AH_PostDate = new ZDateTime(2023, 06, 10);
				Factory.Save();
				AssertPivotCreation("[AR]When AH_PostDate equal to EReportingComplianceDate(2023-06-10)", arInvoice20230610, new[] { EInvoicingPivotState.Queued });
				mockEPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(arInvoice20230610, new DateTime(2023, 06, 10)), Times.Exactly(1));
				mockEPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(It.IsAny<AccTransactionHeader>(), It.IsAny<DateTime>()), Times.Exactly(2));

				var apInvoice20230509 = CreateTransaction<APInvoice>("AP20230509");
				apInvoice20230509.AH_PostDate = new ZDateTime(2023, 05, 09);
				Factory.Save();
				AssertPivotCreation("[AP]When AH_PostDate is less than EReportingComplianceDateForPayables(2023-05-10)", apInvoice20230509, new[] { EInvoicingPivotState.Queued });
				mockEPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(apInvoice20230509, new DateTime(2023, 05, 10)), Times.Exactly(1));
				mockEPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(It.IsAny<AccTransactionHeader>(), It.IsAny<DateTime>()), Times.Exactly(3));

				var apInvoice20230510 = CreateTransaction<APInvoice>("AP20230510");
				apInvoice20230510.AH_PostDate = new ZDateTime(2023, 05, 10);
				Factory.Save();
				AssertPivotCreation("[AP]When AH_PostDate equal to EReportingComplianceDateForPayables(2023-05-10)", apInvoice20230510, new[] { EInvoicingPivotState.Queued });
				mockEPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(apInvoice20230510, new DateTime(2023, 05, 10)), Times.Exactly(1));
				mockEPreEligibilityProvider.Verify(x => x.CanEvaluateByComplianceDate(It.IsAny<AccTransactionHeader>(), It.IsAny<DateTime>()), Times.Exactly(4));
			}
		}

		void AssertPivotCreation(string comment, AccTransactionHeader transactionHeader, IEnumerable<string> expectedPivotStatuses)
		{
			var actualPivotStatuses = RefetchPivotStatusesForTransaction(transactionHeader.PK);
			CombineAssertions(comment, () =>
			{
				AssertEquals("Count", expectedPivotStatuses.Count(), actualPivotStatuses.Count);
				AssertSequencesEqual("actualPivotStatuses", expectedPivotStatuses, actualPivotStatuses);
			});
		}

		public void TestEvaluateEligibilityAndQueue_Eligible_QueuesNewPivotAndDiscard()
		{
			var today = ZDateTime.Today;
			var complianceDate = today.AddDays(-1).ToDateTime();
			var registry = AccountingMasterFilesRegistry.Instance;
			var expectedPivotStatuses = new[] { EInvoicingPivotState.Discarded };

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				using (registry.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				using (registry.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, complianceDate))
				using (registry.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
				{
					var transRec = Factory.NewWithValidTestData<ARInvoice>();
					transRec.AH_TransactionNum = "00001000";
					transRec.AH_PostDate = today;
					var proxy = new ElectronicInvoicingTransactionProxy(transRec);
					proxy.EvaluateEligibilityAndQueue();
					Factory.Save();

					var actualPivotStatuses = RefetchPivotStatusesForTransaction(transRec.PK);
					AssertEquals("New pivot should be created for AR transaction", 1, actualPivotStatuses.Count);
					AssertSequencesEqual("Queued pivot should be discarded when AR invoice is eligible but e-Invoicing not enabled", expectedPivotStatuses, actualPivotStatuses);
					AssertEquals("No billing record should be created for AR transaction", 0, CountBillingTransactionsFromDatabase(transRec));
				}

				using (registry.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				using (registry.EReportingComplianceDateForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, complianceDate))
				using (registry.EReportingSubmitPivotDefaultStatusForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
				{
					var transPay = Factory.NewWithValidTestData<APInvoice>();
					transPay.AH_TransactionNum = "P1234";
					transPay.AH_PostDate = today;
					var proxy = new ElectronicInvoicingTransactionProxy(transPay);
					proxy.EvaluateEligibilityAndQueue();
					Factory.Save();
					AssertEquals("No pivot should be created for AP transaction not eligible", 0, RefetchPivotStatusesForTransaction(transPay.PK).Count);
					AssertEquals("No Billing record should be created for AP transaction", 0, CountBillingTransactionsFromDatabase(transPay));

					var transPay2 = GetEligiblePayableTransactionForItalianCompliance("A1234");
					proxy = new ElectronicInvoicingTransactionProxy(transPay2);
					proxy.EvaluateEligibilityAndQueue();
					Factory.Save();
					var actualPivotStatuses = RefetchPivotStatusesForTransaction(transPay2.PK);
					AssertEquals("New pivot should be created for AP transaction eligible", 1, actualPivotStatuses.Count);
					AssertSequencesEqual("Queued pivot should be discarded when AP invoice is eligible but e-Invoicing not enabled", expectedPivotStatuses, actualPivotStatuses);
					AssertEquals("No Billing record should be created for AP transaction", 0, CountBillingTransactionsFromDatabase(transPay2));
				}
			}
		}

		public void TestEvaluateEligibilityAndQueue_ShouldQueueDueToAdditionalCondition_TransitioningTransactions() =>
			AssertEvaluateEligibilityAndQueue_ForShouldQueueDueToAdditionalCondition_TransitioningTransactions(true);

		public void TestEvaluateEligibilityAndQueue_ShouldNotQueueDueToAdditionalCondition_TransitioningTransactions() =>
			AssertEvaluateEligibilityAndQueue_ForShouldQueueDueToAdditionalCondition_TransitioningTransactions(false);

		void AssertEvaluateEligibilityAndQueue_ForShouldQueueDueToAdditionalCondition_TransitioningTransactions(bool expected)
		{
			var registryTestDate = ZDateTime.Now.AddDays(-1).ToDateTime();
			var registryInstance = AccountingMasterFilesRegistry.Instance;
			using (registryInstance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (registryInstance.EReportingComplianceDateForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
			{
				TransactionPendingAllocation pendingTrans;
				InvoicingBase incompleteTrans;

				using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: false).Object))
				{
					pendingTrans = Factory.NewWithValidTestData<TransactionPendingAllocation>();
					pendingTrans.AH_TransactionNum = "PA_123";
					Factory.Save();

					incompleteTrans = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "IN_TR_123", TestObjectCreator.EUR, 1, 10, 0, 10, 0);
					incompleteTrans.SaveAsIncomplete();
				}

				using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: expected, canEvaluateByTransaction: expected).Object))
				{
					var apInvoice1 = TransactionAllocationConverter.ConvertUnallocatedToAP(pendingTrans).Invoice;
					var line = apInvoice1.Lines.AddNew();
					line.AL_OSExTaxAmount = apInvoice1.AH_OSExTaxAmount;
					line.AL_AG = TestObjectCreator.GLHeader1.PK;

					var newFactory = apInvoice1.Factory;
					var apInvoice2 = newFactory.Load<APInvoice>(incompleteTrans.PK);
					apInvoice2.RestoreSavedData();
					apInvoice2.MoveFromIncompleteToPayableLedger();
					newFactory.Save();

					var pivot1 = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, apInvoice1.PK));
					var pivot2 = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, apInvoice2.PK));

					AssertEquals(expected, pivot1 != null);
					AssertEquals(expected, pivot2 != null);

					if (expected)
					{
						AssertEquals(EInvoicingPivotActionType.Submit, pivot1.AIP_ActionType);
						AssertEquals(EInvoicingPivotActionType.Submit, pivot2.AIP_ActionType);

						AssertEquals(EInvoicingPivotState.Queued, pivot1.AIP_Status);
						AssertEquals(EInvoicingPivotState.Queued, pivot2.AIP_Status);
					}
				}
			}
		}

		public void TestEvaluateEligibilityAndQueue_WasIneligible_NowEligible_ForItaly()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			using (registry.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (registry.EReportingComplianceDateForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-1).ToDateTime()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var incompleteTrans = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "IN123", TestObjectCreator.EUR, 1, 10, 0, 10, 0);
				incompleteTrans.AH_Ledger = LedgerTypes.IncompleteTransactions;
				incompleteTrans.AH_TransactionType = TransactionTypes.IncompleteInvoice;

				var unapprovedTrans = TestObjectCreator.CreateInvoiceWithLine(typeof(UAInvoice), "UA123", TestObjectCreator.EUR, 1, 10, 0, 10, 0);

				var pendingTrans = Factory.NewWithValidTestData<TransactionPendingAllocation>();
				pendingTrans.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
				pendingTrans.AH_TransactionNum = "PA123";

				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test";
				taxRate.AT_Type = AccTaxRate.Types.ReverseRated;

				Factory.Save();
				AssertEquals("Precondition: no pivots for Incomplete transaction", 0, RefetchPivotStatusesForTransaction(incompleteTrans.PK).Count);
				AssertEquals("Precondition: no pivots for Unapproved transaction", 0, RefetchPivotStatusesForTransaction(unapprovedTrans.PK).Count);
				AssertEquals("Precondition: no pivots for Pending Allocation transaction", 0, RefetchPivotStatusesForTransaction(pendingTrans.PK).Count);

				incompleteTrans.AH_Ledger = LedgerTypes.AccountsPayable;
				incompleteTrans.AH_TransactionType = TransactionTypes.Invoice;
				incompleteTrans.Lines[0].AL_AT = taxRate.PK;
				Factory.Save();
				AssertNotNull("Postcondition: one pivot for Incomplete transaction", RefetchPivotStatusesForTransaction(incompleteTrans.PK).FirstOrDefault());

				var approvedTrans = new UnapprovedTransactionConverter(Factory).ConvertToAP(unapprovedTrans, true);
				approvedTrans.Lines[0].AL_AT = taxRate.PK;
				approvedTrans.Factory.Save();
				AssertNotNull("Postcondition: one pivot for Unapproved transaction", RefetchPivotStatusesForTransaction(approvedTrans.PK).FirstOrDefault());

				var allocatedTrans = TransactionAllocationConverter.ConvertUnallocatedToAP(pendingTrans).Invoice;
				var line = (InvoicingLineBase)allocatedTrans.Lines.AddNew();
				line.AL_OSExTaxAmount = allocatedTrans.AH_OSExTaxAmount;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_AT = taxRate.PK;
				allocatedTrans.Factory.Save();
				AssertNotNull("Postcondition: one pivot for Pending Allocation transaction", RefetchPivotStatusesForTransaction(allocatedTrans.PK).FirstOrDefault());
			}
		}

		public void TestEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot()
		{
			AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsReceivable, Array.Empty<string>(), "A1233", true);
			AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsPayable, Array.Empty<string>(), "A1234", true);

			var expectedStatus = EInvoicingPivotState.Queued;
			var expectedStatusArray = new[] { expectedStatus };
			var registryTestDate = ZDateTime.Now.AddDays(-1).ToDateTime();
			var registryInstance = AccountingMasterFilesRegistry.Instance;
			using (registryInstance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (registryInstance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
			using (registryInstance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedStatus))
			{
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsReceivable, expectedStatusArray, "A1235", true);
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsPayable, Array.Empty<string>(), "A1236", true);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			using (registryInstance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (registryInstance.EReportingComplianceDateForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
			using (registryInstance.EReportingSubmitPivotDefaultStatusForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedStatus))
			{
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsPayable, expectedStatusArray, "A1237", true);
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsReceivable, Array.Empty<string>(), "A1238", true);
			}

			using (registryInstance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (registryInstance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
			using (registryInstance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedStatus))
			using (registryInstance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (registryInstance.EReportingComplianceDateForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
			using (registryInstance.EReportingSubmitPivotDefaultStatusForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedStatus))
			{
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsReceivable, expectedStatusArray, "A1239", true);
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsPayable, expectedStatusArray, "B1235", true);
			}
		}

		public void TestEvaluateEligibilityAndQueue_WasIneligible_StillIneligible_HasNoPivot()
		{
			AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsReceivable, Array.Empty<string>());
			AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsPayable, Array.Empty<string>());

			var expectedStatus = EInvoicingPivotState.Queued;
			var registryTestDate = ZDateTime.Now.AddDays(-1).ToDateTime();
			var registryInstance = AccountingMasterFilesRegistry.Instance;
			using (registryInstance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (registryInstance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
			using (registryInstance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedStatus))
			{
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsReceivable, Array.Empty<string>(), "A1235");
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsPayable, Array.Empty<string>(), "B1236");
			}

			using (registryInstance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (registryInstance.EReportingComplianceDateForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
			using (registryInstance.EReportingSubmitPivotDefaultStatusForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedStatus))
			{
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsPayable, Array.Empty<string>(), "A1237");
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsReceivable, Array.Empty<string>(), "A1238");
			}

			using (registryInstance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (registryInstance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
			using (registryInstance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedStatus))
			using (registryInstance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (registryInstance.EReportingComplianceDateForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTestDate))
			using (registryInstance.EReportingSubmitPivotDefaultStatusForPayables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedStatus))
			{
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsReceivable, Array.Empty<string>(), "A1239");
				AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(LedgerTypes.AccountsPayable, Array.Empty<string>(), "B1235");
			}
		}

		void AssertEvaluateEligibilityAndQueue_WasIneligible_NowEligible_CreatesPivot_Or_StillIneligible_HasNoPivot(string ledgerType, string[] expectedPivotStatuses, string transactionNumber = "A1234", bool isEligible_ValueToTest = false)
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = ledgerType;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = transactionNumber;
			transaction.AH_PostDate = ZDateTime.Now;

			var mockGlobalCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithEInvoicingEligibilityDecider(isEligible_ValueToTest)
				.WithEInvoicingPreEligibilityProvider(canEvaluateByTransaction: true)
				.ToGlobalFactory();
			Factory.Save();

			AssertEquals("Precondition: no pivots for transaction", 0, RefetchPivotStatusesForTransaction(transaction.PK).Count);
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoice = newFactory.Load<GovernmentInvoice>(transaction.PK);
				invoice.AH_ComplianceSubType = "ZXC";
				invoice.AH_TransactionReference = "AAA9999";
				newFactory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transaction.PK);
				if (isEligible_ValueToTest)
				{
					AssertSequencesEqual("One queued pivot should be created when invoice is eligible", expectedPivotStatuses, actualPivotStatuses);
				}
				else
				{
					AssertEquals("No pivots should be created when invoice is ineligible", 0, actualPivotStatuses.Count);
				}
			}
		}

		public void TestEvaluateEligibilityAndQueue_WasEligible_StillEligible_QueuesNewPivot()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "A1234";
			transaction.AH_PostDate = ZDateTime.Now;
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			pivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
			pivot.AIP_ErrorDescription = "Some error";
			Factory.Save();
			AssertEquals("Precondition: one pivot for transaction", 1, RefetchPivotStatusesForTransaction(transaction.PK).Count);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoice = newFactory.Load<GovernmentInvoice>(transaction.PK);
				invoice.AH_ComplianceSubType = "ZXC";
				invoice.AH_TransactionReference = "AAA9999";
				newFactory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transaction.PK);
				var expectedPivotStatuses = new[] { EInvoicingPivotState.Discarded, EInvoicingPivotState.Queued };
				AssertSequencesEqual("Previous pivot should be discarded; queued pivot should be created when invoice is eligible", expectedPivotStatuses, actualPivotStatuses);
			}
		}

		public void TestEvaluateEligibilityAndQueue_WasEligibleAndDiscarded_StillEligible_QueuesNewPivot()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "A1234";
			transaction.AH_PostDate = ZDateTime.Now;
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			pivot.AIP_Status = EInvoicingPivotState.Discarded;
			pivot.AIP_ErrorDescription = "Some error";
			Factory.Save();
			AssertEquals("Precondition: one pivot for transaction", 1, RefetchPivotStatusesForTransaction(transaction.PK).Count);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoice = newFactory.Load<GovernmentInvoice>(transaction.PK);
				invoice.AH_ComplianceSubType = "ZXC";
				invoice.AH_TransactionReference = "AAA9999";
				newFactory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transaction.PK);
				var expectedPivotStatuses = new[] { EInvoicingPivotState.Discarded, EInvoicingPivotState.Queued };
				AssertSequencesEqual("Previous pivot should remain discarded; queued pivot should be created when invoice is eligible", expectedPivotStatuses, actualPivotStatuses);
			}
		}

		public void TestEvaluateEligibilityAndQueue_WasEligible_NowIneligible_DiscardsPivot()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "A1234";
			transaction.AH_PostDate = ZDateTime.Now;
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			pivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
			pivot.AIP_ErrorDescription = "Some error";
			Factory.Save();
			AssertEquals("Precondition: one pivot for transaction", 1, RefetchPivotStatusesForTransaction(transaction.PK).Count);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: false).Object))
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoice = newFactory.Load<GovernmentInvoice>(transaction.PK);
				invoice.AH_ComplianceSubType = "ZXC";
				invoice.AH_TransactionReference = "AAA9999";
				newFactory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transaction.PK);
				var expectedPivotStatuses = new[] { EInvoicingPivotState.Discarded };
				AssertSequencesEqual("Previous pivot should be discarded when invoice is ineligible", expectedPivotStatuses, actualPivotStatuses);
			}
		}

		public void TestEvaluateEligibilityAndQueue_WasEligibleAndDiscarded_NowIneligible_DiscardsPivot()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "A1234";
			transaction.AH_PostDate = ZDateTime.Now;
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			pivot.AIP_Status = EInvoicingPivotState.Discarded;
			pivot.AIP_ErrorDescription = "Some error";
			Factory.Save();
			AssertEquals("Precondition: one pivot for transaction", 1, RefetchPivotStatusesForTransaction(transaction.PK).Count);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: false).Object))
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoice = newFactory.Load<GovernmentInvoice>(transaction.PK);
				invoice.AH_ComplianceSubType = "ZXC";
				invoice.AH_TransactionReference = "AAA9999";
				newFactory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transaction.PK);
				var expectedPivotStatuses = new[] { EInvoicingPivotState.Discarded };
				AssertSequencesEqual("Previous pivot should still be discarded when invoice is ineligible", expectedPivotStatuses, actualPivotStatuses);
			}
		}

		public void TestEvaluateEligibilityAndQueue_RequeuePivot_WhenSentStatus_DoesNothing()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "A1234";
			transaction.AH_PostDate = ZDateTime.Now;
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			pivot.AIP_Status = EInvoicingPivotState.Sent;
			Factory.Save();
			AssertEquals("Precondition: one pivot for transaction", 1, RefetchPivotStatusesForTransaction(transaction.PK).Count);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoice = newFactory.Load<GovernmentInvoice>(transaction.PK);
				invoice.AH_ComplianceSubType = "ZXC";
				invoice.AH_TransactionReference = "AAA9999";
				newFactory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transaction.PK);
				var expectedPivotStatuses = new[] { EInvoicingPivotState.Sent };
				AssertSequencesEqual("Pivot status should remain Sent as re-queuing is dangerous after Batched status", expectedPivotStatuses, actualPivotStatuses);
			}
		}

		public void TestEvaluateEligibilityAndQueue_ChangeNonComplianceFields_WhenEligible_DoesNothing()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "A1234";
			transaction.AH_PostDate = ZDateTime.Now;
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			pivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
			Factory.Save();
			AssertEquals("Precondition: one pivot for transaction", 1, RefetchPivotStatusesForTransaction(transaction.PK).Count);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoice = newFactory.Load<ARInvoice>(transaction.PK);
				invoice.AH_Desc = "I changed description";
				newFactory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transaction.PK);
				var expectedPivotStatuses = new[] { EInvoicingPivotState.BatchedWithError };
				AssertSequencesEqual("Pivot status should remain unchanged as compliance fields were not changed", expectedPivotStatuses, actualPivotStatuses);
			}
		}

		public void TestEvaluateEligibilityAndQueue_WhenServiceTaskRunsInBackground_ThrowsConcurrencyException()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "A1234";
			transaction.AH_PostDate = ZDateTime.Now;
			transaction.AH_ComplianceSubType = "ABC";
			transaction.AH_TransactionReference = "DEF1234";
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_ParentID = transaction.PK;
			pivot.AIP_Status = EInvoicingPivotState.Queued;
			Factory.Save();
			AssertEquals("Precondition: one pivot for transaction", 1, RefetchPivotStatusesForTransaction(transaction.PK).Count);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoice = newFactory.Load<GovernmentInvoice>(transaction.PK);
				invoice.AH_ComplianceSubType = "ZXC";
				invoice.AH_TransactionReference = "AAA9999";
				invoice.RunPreSaveValidation();
				Assert("Precondition: No validation errors", !invoice.Notifications.Any(x => x.Type.IsFatal));

				// Simulate service task running and changing pivot status
				var serviceTaskFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var pivotQuery = GetPivotQueryForTransaction(transaction.PK);
				var serviceTaskPivot = serviceTaskFactory.Load<AccEInvoicingTransactionPivot>(pivotQuery).FirstOrDefault();
				serviceTaskPivot.AIP_Status = EInvoicingPivotState.Sent;
				serviceTaskFactory.Save();

				invoice.RunPreSaveValidation();
				Assert("Still no validation errors after service task runs", !invoice.Notifications.Any(x => x.Type.IsFatal));
				AssertExceptionThrown<ZSaveConcurrencyException>("A concurrency exception should be thrown as the pivot status has changed", () => newFactory.Save());

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transaction.PK);
				var expectedPivotStatuses = new[] { EInvoicingPivotState.Sent };
				AssertSequencesEqual("Pivot status should remain unchanged as concurrency policy should prevent save", expectedPivotStatuses, actualPivotStatuses);
			}
		}

		public void TestEvaluateEligibilityAndQueue_PendingAllocationTurkey()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			{
				var transactionPA1 = CreateTransaction<TransactionPendingAllocation>("A1231", "2F3661E7-A17F-4197-8CE0-C727ADB487F2") as TransactionPendingAllocation;
				var transactionPA2 = CreateTransaction<TransactionPendingAllocation>("A1232") as TransactionPendingAllocation;
				var transactionAP1 = CreateTransaction<APInvoice>("A1235", "2F3661E7-A17F-4197-8CE0-C727ADB487F2") as APInvoice;
				var transactionAP2 = CreateTransaction<APInvoice>("A1236") as APInvoice;
				Factory.Save();

				var pivotQuery = GetPivotQueryForTransaction(transactionPA1.PK);
				var pivot = Factory.Load<AccEInvoicingTransactionPivot>(pivotQuery).FirstOrDefault();
				AssertNotNull(pivot);
				AssertEquals(EInvoicingPivotActionType.ConfirmTransactionReceived, pivot.AIP_ActionType);
				AssertEquals(EInvoicingPivotState.Queued, pivot.AIP_Status);

				AssertPivotIsNotCreated(transactionPA2.PK);
				AssertPivotIsNotCreated(transactionAP1.PK);
				AssertPivotIsNotCreated(transactionAP2.PK);
			}

			void AssertPivotIsNotCreated(ZGuid id, string actionType = null) =>
				Assert(!Factory.Load<AccEInvoicingTransactionPivot>(GetPivotQueryForTransaction(id, actionType)).Any());
		}

		public void TestEvaluateEligibilityAndQueue_ForApproval_PendingAllocationTransaction_ApprovalRequestStatusIsNotAsApplicable()
		{
			var testObjects = EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<TransactionPendingAllocation>("PIC");
			AssertInitialDataSetupOfEvaluateEligibilityAndQueueForApprovalAndRejection(testObjects);

			EvaluateERequestingEligibilityAndQueue_InlineDataSetup(testObjects.transaction, GenApprovalRequestApprovalStatus.Requested);

			AssertNull(GetPivot(testObjects.transaction.PK, EInvoicingPivotActionType.Approve));
		}

		public void TestEvaluateEligibilityAndQueue_ForApproval_PendingAllocationTransaction_ApprovalRequestStatusDoesNotHaveChanges()
		{
			var testObjects = EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<TransactionPendingAllocation>("PIC");
			AssertInitialDataSetupOfEvaluateEligibilityAndQueueForApprovalAndRejection(testObjects);

			EvaluateERequestingEligibilityAndQueue_InlineDataSetup(testObjects.transaction, GenApprovalRequestApprovalStatus.ApprovalRequested, setApprovalStatusAsNotChanged: true);

			AssertNull(GetPivot(testObjects.transaction.PK, EInvoicingPivotActionType.Approve));
		}

		public void TestEvaluateEligibilityAndQueue_ForApproval_PendingAllocationTransaction_TransactionIsNotEligibleToCreateApprovalRequest()
		{
			var testObjects = EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<TransactionPendingAllocation>("PIC");
			AssertInitialDataSetupOfEvaluateEligibilityAndQueueForApprovalAndRejection(testObjects);

			testObjects.rejectionProviderMock.Setup(x => x.IsTransactionEligibleToCreateApprovalRequest(It.IsAny<AccTransactionHeader>())).Returns(false);
			EvaluateERequestingEligibilityAndQueue_InlineDataSetup(testObjects.transaction, GenApprovalRequestApprovalStatus.ApprovalRequested);

			AssertNull(GetPivot(testObjects.transaction.PK, EInvoicingPivotActionType.Approve));
		}

		public void TestEvaluateEligibilityAndQueue_ForApproval_PendingAllocationTransaction_ApprovalRequestIsSucceeded()
		{
			var testObjects = EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<TransactionPendingAllocation>("PIC");
			AssertInitialDataSetupOfEvaluateEligibilityAndQueueForApprovalAndRejection(testObjects);

			testObjects.rejectionProviderMock.Setup(x => x.IsTransactionEligibleToCreateApprovalRequest(It.IsAny<AccTransactionHeader>())).Returns(true);
			EvaluateERequestingEligibilityAndQueue_InlineDataSetup(testObjects.transaction, GenApprovalRequestApprovalStatus.ApprovalRequested);

			var pivot = GetPivot(testObjects.transaction.PK, EInvoicingPivotActionType.Approve);
			AssertNotNull(pivot);
			AssertEquals(Env.CurrentCompanyPK, pivot.AIP_GC);
			AssertEquals(Env.CurrentCompany.Country.Code, pivot.AIP_RN_NKCountryCode);
			AssertEquals(EInvoicingPivotState.Queued, pivot.AIP_Status);
			AssertEquals(EInvoicingPivotActionType.Approve, pivot.AIP_ActionType);
			testObjects.rejectionProviderMock.Verify(x => x.IsTransactionEligibleToCreateApprovalRequest(It.IsAny<AccTransactionHeader>()), Times.Once);
		}

		public void TestEvaluateEligibilityAndQueue_ForApproval_PayablesInvoice_DoesNotCreateEInvoicingPivot()
		{
			var testObjects = EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<APInvoice>("PIC");
			var transaction = testObjects.transaction;

			var mostRecentPivot = transaction.GetMostRecentEInvoicingTransactionPivot();
			AssertNull(mostRecentPivot);
			AssertNull(GetPivot(transaction.PK, EInvoicingPivotActionType.ConfirmTransactionReceived));
			AssertNull(GetPivot(transaction.PK, EInvoicingPivotActionType.Approve));
			testObjects.rejectionProviderMock.Verify(x => x.IsTransactionEligibleToCreateApprovalRequest(It.IsAny<AccTransactionHeader>()), Times.Never);
		}

		public void TestEvaluateEligibilityAndQueue_ForRejection_PendingAllocationTransaction_ApprovalRequestStatusIsNotAsApplicable()
		{
			var testObjects = EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<TransactionPendingAllocation>("PIC");
			AssertInitialDataSetupOfEvaluateEligibilityAndQueueForApprovalAndRejection(testObjects);

			EvaluateERequestingEligibilityAndQueue_InlineDataSetup(testObjects.transaction, GenApprovalRequestApprovalStatus.Requested);

			AssertNull(GetPivot(testObjects.transaction.PK, EInvoicingPivotActionType.Approve));
		}

		public void TestEvaluateEligibilityAndQueue_ForRejection_PendingAllocationTransaction_ApprovalRequestStatusDoesNotHaveChanges()
		{
			var testObjects = EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<TransactionPendingAllocation>("PIC");
			AssertInitialDataSetupOfEvaluateEligibilityAndQueueForApprovalAndRejection(testObjects);

			EvaluateERequestingEligibilityAndQueue_InlineDataSetup(testObjects.transaction, GenApprovalRequestApprovalStatus.RejectionRequested, setApprovalStatusAsNotChanged: true);

			AssertNull(GetPivot(testObjects.transaction.PK, EInvoicingPivotActionType.Reject));
		}

		public void TestEvaluateEligibilityAndQueue_ForRejection_PendingAllocationTransaction_TransactionIsNotEligibleToCreateRejectionRequest()
		{
			var testObjects = EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<TransactionPendingAllocation>("PIC");
			AssertInitialDataSetupOfEvaluateEligibilityAndQueueForApprovalAndRejection(testObjects);

			testObjects.rejectionProviderMock.Setup(x => x.IsTransactionEligibleToCreateRejectionRequest(It.IsAny<AccTransactionHeader>())).Returns(false);
			EvaluateERequestingEligibilityAndQueue_InlineDataSetup(testObjects.transaction, GenApprovalRequestApprovalStatus.RejectionRequested);

			AssertNull(GetPivot(testObjects.transaction.PK, EInvoicingPivotActionType.Reject));
		}

		public void TestEvaluateEligibilityAndQueue_ForRejection_PendingAllocationTransaction_RejectionRequestIsSucceeded()
		{
			var testObjects = EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<TransactionPendingAllocation>("PIC");
			AssertInitialDataSetupOfEvaluateEligibilityAndQueueForApprovalAndRejection(testObjects);

			testObjects.rejectionProviderMock.Setup(x => x.IsTransactionEligibleToCreateRejectionRequest(It.IsAny<AccTransactionHeader>())).Returns(true);
			EvaluateERequestingEligibilityAndQueue_InlineDataSetup(testObjects.transaction, GenApprovalRequestApprovalStatus.RejectionRequested);

			var pivot = GetPivot(testObjects.transaction.PK, EInvoicingPivotActionType.Reject);
			AssertNotNull(pivot);
			AssertEquals(Env.CurrentCompanyPK, pivot.AIP_GC);
			AssertEquals(Env.CurrentCompany.Country.Code, pivot.AIP_RN_NKCountryCode);
			AssertEquals(EInvoicingPivotState.Queued, pivot.AIP_Status);
			AssertEquals(EInvoicingPivotActionType.Reject, pivot.AIP_ActionType);
			testObjects.rejectionProviderMock.Verify(x => x.IsTransactionEligibleToCreateRejectionRequest(It.IsAny<AccTransactionHeader>()), Times.Once);
		}

		public void TestEvaluateEligibilityAndQueue_ForRejection_PayablesInvoice_DoesNotCreateEInvoicingPivot()
		{
			var testObjects = EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<APInvoice>("PIC");
			var transaction = testObjects.transaction;

			var mostRecentPivot = transaction.GetMostRecentEInvoicingTransactionPivot();
			AssertNull(mostRecentPivot);
			AssertNull(GetPivot(transaction.PK, EInvoicingPivotActionType.ConfirmTransactionReceived));
			AssertNull(GetPivot(transaction.PK, EInvoicingPivotActionType.Reject));
			testObjects.rejectionProviderMock.Verify(x => x.IsTransactionEligibleToCreateRejectionRequest(It.IsAny<AccTransactionHeader>()), Times.Never);
		}

		void AssertInitialDataSetupOfEvaluateEligibilityAndQueueForApprovalAndRejection((TransactionPendingAllocation transaction, Mock<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider> rejectionProviderMock, Mock<IEInvoicingPivotActionTypeProvider> pivotActionTypeProviderMock) testObjects)
		{
			var transaction = testObjects.transaction;
			var mostRecentPivot = transaction.GetMostRecentEInvoicingTransactionPivot();

			AssertNotNull(mostRecentPivot);
			AssertEquals(EInvoicingPivotActionType.ConfirmTransactionReceived, mostRecentPivot.AIP_ActionType);
			AssertNull(GetPivot(transaction.PK, EInvoicingPivotActionType.Approve));
			AssertNull(GetPivot(transaction.PK, EInvoicingPivotActionType.Reject));
			TestDateAttribute.AddMinutes(1);
		}

		AccEInvoicingTransactionPivot GetPivot(ZGuid transactionPK, string actionType = null) => Factory.Load<AccEInvoicingTransactionPivot>(GetPivotQueryForTransaction(transactionPK, actionType)).FirstOrDefault();

		void EvaluateERequestingEligibilityAndQueue_InlineDataSetup(InvoicingBase transaction, string status, bool setApprovalStatusAsNotChanged = false)
		{
			transaction.TransactionRelatedApprovalRequest.XP_ApprovalStatus = status;
			if (setApprovalStatusAsNotChanged)
			{
				((INeedRow)transaction.TransactionRelatedApprovalRequest).Row.AcceptChanges();
			}
			Factory.Save();
		}

		(T transaction, Mock<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider> rejectionProviderMock, Mock<IEInvoicingPivotActionTypeProvider> pivotActionTypeProviderMock)
		EvaluateEligibilityAndQueue_InitialDataSetupForERequesting<T>(string complianceSubType) where T : InvoicingBase
		{
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime());

			var (eRequestProviderMock, pivotActionTypeProviderMock) = TestObjectCreator.MockCountryFactoryForTPAAeInvoicing();

			var transaction = CreateTransaction<T>("TRN1", "2F3661E7-A17F-4197-8CE0-C727ADB487F2", complianceSubType) as T;
			Factory.Save();

			return (transaction, eRequestProviderMock, pivotActionTypeProviderMock);
		}

		TransactionHeader CreateTransaction<T>(string transactionNumber, string governmentAllocatedID = "", string complianceSubType = "") where T : TransactionHeader
		{
			var transaction = Factory.NewWithValidTestData<T>();
			transaction.AH_TransactionNum = transactionNumber;
			transaction.AH_PostDate = ZDateTime.Now;
			transaction.AH_GovernmentAllocatedID = governmentAllocatedID;
			transaction.AH_ComplianceSubType = complianceSubType;
			return transaction;
		}

		public void TestEvaluateEligibilityAndQueue_ReceivableInvoice_IfComplianceFieldsChanged()
		{
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime());
			using (ObjectFactory.Substitute(MockAndSetICountryComplianceFactory().Object))
			using (ObjectFactory.Substitute(GetIGlobalAccountingCountryFactory(isEligible: true).Object))
			{
				var transaction = Factory.NewWithValidTestData<ARInvoice>();
				transaction.AH_ComplianceSubType = "EIN";
				transaction.AH_TransactionReference = "TRNREF1";
				Factory.Save();

				var mostRecentPivot = transaction.GetMostRecentEInvoicingTransactionPivot();
				AssertNotNull(mostRecentPivot);

				transaction.AH_ComplianceSubType = "EIC";
				Factory.Save();

				var reQueuedMostRecentPivot = transaction.GetMostRecentEInvoicingTransactionPivot();
				AssertNotNull(reQueuedMostRecentPivot);
				AssertEquals(EInvoicingPivotState.Discarded, mostRecentPivot.AIP_Status);
				AssertNotEquals(mostRecentPivot.PK, reQueuedMostRecentPivot.PK);
				AssertEquals(EInvoicingPivotState.Queued, reQueuedMostRecentPivot.AIP_Status);

				transaction.AH_TransactionReference = "TRNREF2";
				Factory.Save();

				var secondReQueuedMostRecentPivot = transaction.GetMostRecentEInvoicingTransactionPivot();
				AssertNotNull(secondReQueuedMostRecentPivot);
				AssertEquals(EInvoicingPivotState.Discarded, reQueuedMostRecentPivot.AIP_Status);
				AssertNotEquals(reQueuedMostRecentPivot.PK, secondReQueuedMostRecentPivot.PK);
				AssertEquals(EInvoicingPivotState.Queued, secondReQueuedMostRecentPivot.AIP_Status);
			}
		}

		public void TestEvaluateEligibilityAndQueue_ForSaudiArabia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SaudiArabia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			{
				var queuedStatus = true;
				var discardedStatus = true;
				var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate1.AT_Code = "CAPRATED";
				taxRate1.AT_Type = AccTaxRate.Types.CapitalRated;
				var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate2.AT_Code = "NOTREPORT";
				taxRate2.AT_Type = AccTaxRate.Types.NotReportable;

				if (queuedStatus)
				{
					var expectedPivotStatusesQueued = new[] { EInvoicingPivotState.Queued };
					var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123");
					var line1 = TestObjectCreator.CreateInvoiceLine(transaction1, TestObjectCreator.LocalCurrency, transaction1.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
					line1.AL_AT = taxRate1.PK;
					var line2 = TestObjectCreator.CreateInvoiceLine(transaction1, TestObjectCreator.LocalCurrency, transaction1.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
					line2.AL_AT = taxRate2.PK;
					var proxy = new ElectronicInvoicingTransactionProxy(transaction1);
					proxy.EvaluateEligibilityAndQueue();
					Factory.Save();

					var actualPivotStatusesQueued = RefetchPivotStatusesForTransaction(transaction1.PK);
					AssertEquals("New pivot should be created for AR transaction", 1, actualPivotStatusesQueued.Count);
					AssertEquals("Pivot created with Queued status", EInvoicingPivotState.Queued, actualPivotStatusesQueued.FirstOrDefault());
					AssertSequencesEqual("AR Invoice with atleast one of the eligible tax types should be in Queued status", expectedPivotStatusesQueued, actualPivotStatusesQueued);
				}
				if (discardedStatus)
				{
					var expectedPivotStatusesDiscarded = new[] { EInvoicingPivotState.Discarded };
					var transaction2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123");
					var line1 = TestObjectCreator.CreateInvoiceLine(transaction2, TestObjectCreator.LocalCurrency, transaction2.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
					line1.AL_AT = taxRate2.PK;
					var proxy2 = new ElectronicInvoicingTransactionProxy(transaction2);
					proxy2.EvaluateEligibilityAndQueue();
					Factory.Save();

					var actualPivotStatusesDiscarded = RefetchPivotStatusesForTransaction(transaction2.PK);
					AssertEquals("New pivot should be created for AR transaction", 1, actualPivotStatusesDiscarded.Count);
					AssertEquals("Pivot created with Discarded status", EInvoicingPivotState.Discarded, actualPivotStatusesDiscarded.FirstOrDefault());
					AssertSequencesEqual("AR Invoice with only Not Reportable tax type should be in Discarded (DCD) status", expectedPivotStatusesDiscarded, actualPivotStatusesDiscarded);
				}
			}
		}

		public void TestEvaluateEligibilityAndQueue_WhenAPTransaction_NoPivotCreatedForSaudiArabia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SaudiArabia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			{
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "CAPRATED";
				taxRate.AT_Type = AccTaxRate.Types.CapitalRated;

				var transRec = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "IN123", TestObjectCreator.LocalCurrency, 1, 10, 0, 10, 0);
				transRec.AH_Ledger = LedgerTypes.AccountsPayable;
				transRec.AH_TransactionNum = "00001000";
				transRec.AH_PostDate = ZDateTime.Today;
				transRec.Lines[0].AL_AT = taxRate.PK;
				var proxy = new ElectronicInvoicingTransactionProxy(transRec);
				proxy.EvaluateEligibilityAndQueue();
				Factory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transRec.PK);
				AssertEquals("New pivot should not be created for AP transaction", 0, actualPivotStatuses.Count);
			}
		}

		public void TestEvaluateEligibilityAndQueue_WhenRegistrySettingFalse_PivotCreatedWithDCDForSaudiArabia()
		{
			var expectedPivotStatuses = new[] { EInvoicingPivotState.Discarded };

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SaudiArabia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			{
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "CAPRATED";
				taxRate.AT_Type = AccTaxRate.Types.CapitalRated;

				var transRec = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "IN123", TestObjectCreator.LocalCurrency, 1, 10, 0, 10, 0);
				transRec.AH_Ledger = LedgerTypes.AccountsReceivable;
				transRec.AH_TransactionNum = "00001000";
				transRec.AH_PostDate = ZDateTime.Today;
				transRec.Lines[0].AL_AT = taxRate.PK;
				var proxy = new ElectronicInvoicingTransactionProxy(transRec);
				proxy.EvaluateEligibilityAndQueue();
				Factory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transRec.PK);
				AssertEquals("New pivot should be created with DCD when Enable eInvoicing Functionality registry setting set to false", 1, actualPivotStatuses.Count);
				AssertSequencesEqual("When registry setting is Off the pivot is in Discarded (DCD) status", expectedPivotStatuses, actualPivotStatuses);
			}
		}

		public void TestEvaluateEligibilityAndQueue_WhenComplianceDateIsFuture_NoPivotCreatedForSaudiArabia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SaudiArabia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(10).ToDateTime()))
			{
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "CAPRATED";
				taxRate.AT_Type = AccTaxRate.Types.CapitalRated;

				var transRec = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "IN123", TestObjectCreator.LocalCurrency, 1, 10, 0, 10, 0);
				transRec.AH_Ledger = LedgerTypes.AccountsReceivable;
				transRec.AH_TransactionNum = "00001000";
				transRec.AH_PostDate = ZDateTime.Today;
				transRec.Lines[0].AL_AT = taxRate.PK;
				var proxy = new ElectronicInvoicingTransactionProxy(transRec);
				proxy.EvaluateEligibilityAndQueue();
				Factory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transRec.PK);
				AssertEquals("New pivot should not be created when enable eReporting Compliance Date is in future", 0, actualPivotStatuses.Count);
			}
		}

		public void TestEvaluateEligibilityAndQueue_WhenRegistrySettingFalse_PivotCreatedWithDCDForIsrael()
		{
			var expectedPivotStatuses = new[] { EInvoicingPivotState.Discarded };

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			{
				var iLOrgAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS, CountryCodes.Israel, SharedConstants.Languages.Hebrew, "Test IL Address", "ABIGAS Address", OrgAddressType.Receivables, OrgAddressType.Receivables);

				var transRec = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "IN123", TestObjectCreator.LocalCurrency, 1, 26000, 0, 26000, 0);
				transRec.AH_OH = TestObjectCreator.ABIGAS.PK;
				transRec.AH_OA_InvoiceAddressOverride = iLOrgAddress.PK;
				transRec.AH_Ledger = LedgerTypes.AccountsReceivable;
				transRec.AH_TransactionNum = "00001000";
				transRec.AH_PostDate = ZDateTime.Today;
				transRec.Lines[0].AL_AT = TestObjectCreator.GST2.PK;
				transRec.Lines[0].AL_LineAmount = 26000;

				var proxy = new ElectronicInvoicingTransactionProxy(transRec);
				proxy.EvaluateEligibilityAndQueue();
				Factory.Save();

				var actualPivotStatuses = RefetchPivotStatusesForTransaction(transRec.PK);
				AssertEquals("New pivot should be created with DCD when Enable eInvoicing Functionality registry setting set to false", 1, actualPivotStatuses.Count);
				AssertSequencesEqual("When registry setting is Off the pivot is in Discarded (DCD) status", expectedPivotStatuses, actualPivotStatuses);
			}
		}

		public void TestEvaluateEligibilityAndQueue_IsEligible_OnEvaluateEligibilityAndQueue()
		{
			//Arrange
			var mockGlobalCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithEInvoicingEligibilityDecider(isEligible: true)
				.WithEInvoicingActionProvider_OnEvaluateEligibilityAndQueue(out var mockEInvoicingActionProvider)
				.ToGlobalFactory();

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				var transaction = Factory.NewWithValidTestData<ARInvoice>();
				transaction.AH_TransactionNum = "00001000";
				transaction.AH_PostDate = ZDateTime.Now;

				//Act
				Factory.Save();

				//Assert
				var pivotStatus = RefetchPivotStatusesForTransaction(transaction.PK);
				AssertEquals("New pivot should be created", 1, pivotStatus.Count);
				AssertNoExceptionThrown("OnEvaluateEligibilityAndQueue should called once", () => mockEInvoicingActionProvider.Verify(x => x.OnEvaluateEligibilityAndQueue(It.IsAny<AccTransactionHeader>()), Times.Once));
			}
		}

		public void TestEvaluateEligibilityAndQueue_IsEligible_WithoutEInvoicingActionProvider_OnEvaluateEligibilityAndQueue()
		{
			//Arrange
			var mockEInvoicingActionProvider = new Mock<IEInvoicingActionProvider>();
			var mockCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithEInvoicingEligibilityDecider(isEligible: true);
			mockCountryFactory.As<IInstanceProvider<IEInvoicingActionProvider>>().Setup(x => x.Get()).Returns((IEInvoicingActionProvider)null);
			var mockGlobalCountryFactory = mockCountryFactory.ToGlobalFactory();

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				var transaction = Factory.NewWithValidTestData<ARInvoice>();
				transaction.AH_TransactionNum = "00001000";
				transaction.AH_PostDate = ZDateTime.Now;

				//Act
				Factory.Save();

				//Assert
				var pivotStatus = RefetchPivotStatusesForTransaction(transaction.PK);
				AssertEquals("New pivot should be created", 1, pivotStatus.Count);
				AssertNoExceptionThrown("OnEvaluateEligibilityAndQueue should not be called", () => mockEInvoicingActionProvider.Verify(x => x.OnEvaluateEligibilityAndQueue(It.IsAny<AccTransactionHeader>()), Times.Never));
			}
		}

		public void TestEvaluateEligibilityAndQueue_IsNotEligible_OnEvaluateEligibilityAndQueue()
		{
			//Arrange
			var mockGlobalCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithEInvoicingEligibilityDecider(isEligible: false)
				.WithEInvoicingActionProvider_OnEvaluateEligibilityAndQueue(out var mockEInvoicingActionProvider)
				.ToGlobalFactory();

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				var transaction = Factory.NewWithValidTestData<ARInvoice>();
				transaction.AH_TransactionNum = "00001000";
				transaction.AH_PostDate = ZDateTime.Now;

				//Act
				Factory.Save();

				//Assert
				var pivotStatus = RefetchPivotStatusesForTransaction(transaction.PK);
				AssertEquals("New pivot should not be created", 0, pivotStatus.Count);
				AssertNoExceptionThrown("OnEvaluateEligibilityAndQueue should not be called", () => mockEInvoicingActionProvider.Verify(x => x.OnEvaluateEligibilityAndQueue(It.IsAny<AccTransactionHeader>()), Times.Never));
			}
		}

		public void TestEvaluateEligibilityAndQueue_CreatesPivotWithNotEligibleStatus_WhenProviderSupports()
		{
			var mockGlobalCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithEInvoicingPivotStatusProvider_OnEvaluateEligibilityAndQueue(out var mockPivotStatusProvider)
				.WithEInvoicingEligibilityDecider(isEligible: false)
				.ToGlobalFactory();

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				var transaction = Factory.NewWithValidTestData<ARInvoice>();

				mockPivotStatusProvider.Setup(x => x.CanCreateNotEligibleForEInvoicingPivot(It.IsAny<AccTransactionHeader>())).Returns(true);
				var proxy = new ElectronicInvoicingTransactionProxy(transaction);
				proxy.EvaluateEligibilityAndQueue();

				mockPivotStatusProvider.Verify(x => x.CanCreateNotEligibleForEInvoicingPivot(It.Is<AccTransactionHeader>(t => t == transaction)), Times.Once);
				AssertEquals("Not Eligible Status", EInvoicingPivotState.NotEligible, proxy.CurrentStatus);
			}
		}

		public void TestEvaluateEligibilityAndQueue_DoesNotCreateDuplicatePivots_WhenCreatingDepositBatches()
		{
			var mockGlobalCountryFactory = new Mock<IAccountingCountryFactory>()
				.WithEInvoicingPivotStatusProvider_OnEvaluateEligibilityAndQueue(out var mockPivotStatusProvider)
				.WithEInvoicingEligibilityDecider(isEligible: false)
				.ToGlobalFactory();

			mockPivotStatusProvider.Setup(x => x.CanCreateNotEligibleForEInvoicingPivot(It.IsAny<AccTransactionHeader>())).Returns(true);

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute(mockGlobalCountryFactory.Object))
			{
				var bank = Factory.NewWithValidTestData<AccBankAccount>();
				var org = Factory.NewWithValidTestData<OrgHeader>();

				var receipt = Factory.NewWithValidTestData<ARReceipt>();
				receipt.AH_AB = bank.PK;
				receipt.AH_OH = org.PK;
				receipt.AH_InvoiceAmount = -40M;
				receipt.AH_OutstandingAmount = -40M;
				receipt.AH_OSTotal = -40M;
				receipt.AH_ReceiptType = ReceiptTypes.DirectCredit;
				receipt.SkipCreateDepositBatch = false;

				AssertNoExceptionThrown(Factory.Save);

				mockPivotStatusProvider.Verify(x => x.CanCreateNotEligibleForEInvoicingPivot(It.Is<AccTransactionHeader>(t => t == receipt)), Times.Once);

				var proxy = new ElectronicInvoicingTransactionProxy(receipt);
				AssertEquals("Not Eligible Status", EInvoicingPivotState.NotEligible, proxy.CurrentStatus);
			}
		}

		#endregion

		public void TestPivotActionTypeWhenIsAmendingInvoiceInVietnam_VietnamIssuePositiveAdjustmentViaAmendWithInvoiceEnable()
		{
			TestPivotActionTypeWhenIsAmendingInvoiceInVietnam(true, EInvoicingPivotActionType.Adjustment);
		}

		public void TestPivotActionTypeWhenIsAmendingInvoiceInVietnam_VietnamIssuePositiveAdjustmentViaAmendWithInvoiceDisable()
		{
			TestPivotActionTypeWhenIsAmendingInvoiceInVietnam(false, EInvoicingPivotActionType.Submit);
		}

		void TestPivotActionTypeWhenIsAmendingInvoiceInVietnam(bool registryValue, string pivotActionType)
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("VNHOA");
			Factory.Save();
			var branch = company.FirstActiveBranch;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				GlbBranch.CurrentBranch.SetCountry("VN");

				var shipment = TestObjectCreator.CreateShipment("S00001007");
				var job = TestObjectCreator.CreateJob(shipment);

				var arInvoiceOriginal = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateARInvoiceLineWithJobCharge(arInvoiceOriginal, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc1", 100m, TestObjectCreator.GST1.PK);
				var arInvoice = (ARInvoice)(testObjectCreator.AmendARTransaction(TransactionTypes.Invoice, arInvoiceOriginal)).amendTransaction;
				Factory.Save();

				var proxy = new ElectronicInvoicingTransactionProxy(arInvoice);
				var eInvoicingTransactionPivot = proxy.CreateNewPivot();
				AssertEquals(pivotActionType, eInvoicingTransactionPivot.AIP_ActionType);
			}
		}

		#region Helpers

		Mock<ICountryComplianceFactory> MockAndSetICountryComplianceFactory(string authRecordType = "", string governmentNumberColumnName = "")
		{
			EInvoicingInfoMock.Setup(x => x.GetAccTransactionHeaderAuthorisationRecordType()).Returns(authRecordType);
			EInvoicingInfoMock.Setup(x => x.GetGovernmentAllocatedNumberColumnName()).Returns(governmentNumberColumnName);

			CountryComplianceFactoryMock.Setup(c => c.GetIComplianceInfoElectronicInvoicing(It.IsAny<ZString>())).Returns(EInvoicingInfoMock.Object);
			CountryComplianceFactoryMock.Setup(c => c.GetIComplianceInfoElectronicInvoicingEligibleSubType(It.IsAny<ZString>())).Returns(EInvoicingSubTypeMock.Object);
			return CountryComplianceFactoryMock;
		}

		Mock<ICountryComplianceFactory> CountryComplianceFactoryMock => countryComplianceFactoryMock ?? (countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>());
		Mock<ICountryComplianceFactory> countryComplianceFactoryMock;

		Mock<IComplianceInfoElectronicInvoicing> EInvoicingInfoMock => eInvoicingInfoMock ?? (eInvoicingInfoMock = new Mock<IComplianceInfoElectronicInvoicing>());
		Mock<IComplianceInfoElectronicInvoicing> eInvoicingInfoMock;

		Mock<IComplianceInfoElectronicInvoicingEligibleSubType> EInvoicingSubTypeMock => eInvoicingSubTypeMock ?? (eInvoicingSubTypeMock = new Mock<IComplianceInfoElectronicInvoicingEligibleSubType>());
		Mock<IComplianceInfoElectronicInvoicingEligibleSubType> eInvoicingSubTypeMock;

		static Mock<IGlobalAccountingCountryFactory> GetIGlobalAccountingCountryFactory(bool isEligible = false, bool canEvaluateByTransaction = true, bool canEvaluateByComplianceDate = true)
			=> new Mock<IAccountingCountryFactory>()
				.WithEInvoicingEligibilityDecider(isEligible)
				.WithEInvoicingPreEligibilityProvider(canEvaluateByTransaction: canEvaluateByTransaction, canEvaluateByComplianceDate: canEvaluateByComplianceDate)
				.ToGlobalFactory();

		static IReadOnlyCollection<string> RefetchPivotStatusesForTransaction(ZGuid transactionPk)
		{
			var pivotQuery = GetPivotQueryForTransaction(transactionPk);
			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pivots = anotherFactory.Load<AccEInvoicingTransactionPivot>(pivotQuery);
			return pivots
					.OrderBy(p => p.AIP_SystemCreateTimeUtc)
					.Select(p => p.AIP_Status.ToString())
					.ToArray();
		}

		static ZQuery GetPivotQueryForTransaction(ZGuid transactionPk, string actionType = null)
		{
			var query = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionPk)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			if (actionType != null)
			{
				query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType);
			}
			return query;
		}

		int CountBillingTransactionsFromDatabase(InvoicingBase trans)
		{
			int numOfBills = 0;
			using (var cmd = TestConnection.Command($"SELECT SUD_Data FROM dbo.StmUsageData WHERE SUD_Code ='{GlbCompany.CurrentCompany.Country.Code}1' AND SUD_Category ='{ReferenceTypes.Accounting}'"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var bill = BillingManager.DecryptTransaction(reader[0].ToString(), BillingManager.CurrentSchemaVersion);
					if (bill.Reference1 == trans.AH_TransactionNum && bill.Reference4 == trans.AH_Ledger + trans.AH_TransactionType)
					{
						numOfBills++;
					}
				}
			}
			return numOfBills;
		}

		InvoicingBase GetEligiblePayableTransactionForItalianCompliance(string transNum)
		{
			var transaction = TestObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.EUR, 1, testObjectCreator.ABIGAS);
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = transNum;
			transaction.AH_PostDate = ZDateTime.Today;

			var line1 = testObjectCreator.CreateInvoiceLine(transaction, testObjectCreator.EUR, 1M, 35M);
			line1.AL_AT = testObjectCreator.RVS1.PK;

			var line2 = testObjectCreator.CreateInvoiceLine(transaction, testObjectCreator.EUR, 1M, 35M);
			line2.AL_AT = testObjectCreator.GST1.PK;

			Factory.Save();
			return transaction;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
