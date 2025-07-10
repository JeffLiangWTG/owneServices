using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	[TestedType(typeof(AccEInvoicingTransactionPivot))]
	class AccEInvoicingTransactionPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestfetchStrategy()
		{
			var transaction = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			var fetchStrategy = transaction.FetchStrategy;
			var errorMessage = "Expected type AccEInvoicingTransactionPivotFetchStrategy";

			Assert(errorMessage, fetchStrategy != null);
			AssertEquals(errorMessage, fetchStrategy.GetType(), typeof(AccEInvoicingTransactionPivotFetchStrategy));
		}

		public void TestSetDefaultValues()
		{
			var pivot = (AccEInvoicingTransactionPivot)GetNewBusinessObject();
			AssertEquals("AIP_GC", GlbCompany.CurrentCompany.PK, pivot.AIP_GC);
			AssertEquals("AIP_RN_NKCountryCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, pivot.AIP_RN_NKCountryCode);
			AssertEquals("AIP_ActionType", EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);
			AssertEquals("AIP_ActionType", EInvoicingPivotState.Queued, pivot.AIP_Status);
		}

		public void TestReadOnlyProperties()
		{
			var pivot = (AccEInvoicingTransactionPivot)GetNewBusinessObject();
			Assert("AIP_ActionType Read Only", pivot.AIP_ActionTypeInfo.ReadOnly);
			Assert("AIP_Status Read Only", pivot.AIP_StatusInfo.ReadOnly);

			Factory.Save();
			Assert("AIP_ActionType Read Only", pivot.AIP_ActionTypeInfo.ReadOnly);
			Assert("AIP_Status Read Only", pivot.AIP_StatusInfo.ReadOnly);
		}

		public void TestAIP_ActionTypeSetterReportsDeveloperExceptionOnSavedRecord()
		{
			var pivot = (AccEInvoicingTransactionPivot)GetNewBusinessObject();
			AssertEquals("AIP_ActionType default value", EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);
			pivot.AIP_ActionType = EInvoicingPivotActionType.StatusCheck;
			Factory.Save();

			AssertEquals("AIP_ActionType saved value", EInvoicingPivotActionType.StatusCheck, pivot.AIP_ActionType);
			pivot.AIP_ActionType = EInvoicingPivotActionType.DocumentAction;
			AssertEquals("AIP_ActionType new value still assigned", EInvoicingPivotActionType.DocumentAction, pivot.AIP_ActionType);

			var expectedErrorMessage = "Assigning a different value to AIP_ActionType on saved AccEInvoicingTransactionPivot record. Current value: STA, new value: DOC.";
			AssertEquals("Should have a developer notification exception.", 1, ExceptionReporterTestListener.Instance.Count);
			Assert("The developer notification exception should be about setting new value to AIP_ActionType on saved record.", ExceptionReporterTestListener.Instance[0].Message.Contains(expectedErrorMessage));
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestAIP_ActionTypeConcurrency()
		{
			var pivot = (AccEInvoicingTransactionPivot)GetNewBusinessObject();
			AssertEquals("AIP_ActionType default value", EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);
			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var pivotInFactory1 = factory1.Load<AccEInvoicingTransactionPivot>(pivot.PK);
			var pivotInFactory2 = factory2.Load<AccEInvoicingTransactionPivot>(pivot.PK);

			pivotInFactory1.AIP_ActionType = EInvoicingPivotActionType.StatusCheck;
			pivotInFactory2.AIP_ActionType = EInvoicingPivotActionType.Cancel;
			ExceptionReporterTestListener.Instance.Clear();

			factory1.Save();
			try
			{
				factory2.Save();
				Fail("Expected a ZSaveConcurrencyException for AIP_ActionType strict concurrency policy.");
			}
			catch (ZSaveConcurrencyException e)
			{
				AssertContains(AutoAccEInvoicingTransactionPivot.Schema.AIP_ActionType, e.Message);
			}
		}

		public void TestAIP_StatusConcurrency()
		{
			var pivot = (AccEInvoicingTransactionPivot)GetNewBusinessObject();
			pivot.AIP_Status = Enterprise.Core.Constants.EInvoicingPivotState.Failed;
			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var pivotInFactory1 = factory1.Load<AccEInvoicingTransactionPivot>(pivot.PK);
			var pivotInFactory2 = factory2.Load<AccEInvoicingTransactionPivot>(pivot.PK);

			pivotInFactory1.AIP_Status = EInvoicingPivotState.Batched;
			pivotInFactory2.AIP_Status = EInvoicingPivotState.Queued;

			factory1.Save();
			try
			{
				factory2.Save();
				Fail("Expected a ZSaveConcurrencyException for AIP_Status strict concurrency policy.");
			}
			catch (ZSaveConcurrencyException e)
			{
				AssertContains(AutoAccEInvoicingTransactionPivot.Schema.AIP_Status, e.Message);
			}
		}

		public void TestSetCompanyAndCountryCode()
		{
			AssertNotEquals("Precondition", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, CountryCodes._TemplateCountryName_);

			var pivot = (AccEInvoicingTransactionPivot)GetNewBusinessObject();
			AssertEquals("AIP_GC uses CurrentCompany by default", GlbCompany.CurrentCompany.PK, pivot.AIP_GC);
			AssertEquals("AIP_RN_NKCountryCode uses CurrentCompany by default", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, pivot.AIP_RN_NKCountryCode);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes._TemplateCountryName_;

			pivot.SetCompanyAndCountryCode(company);
			AssertEquals("AIP_GC is updated", company.PK, pivot.AIP_GC);
			AssertEquals("AIP_RN_NKCountryCode is updated", CountryCodes._TemplateCountryName_, pivot.AIP_RN_NKCountryCode);
		}

		public void TestRequeue()
			=> TestRequeue(registryStatusCode: "", expectedStatus: EInvoicingPivotState.Queued);

		public void TestRequeue_ToQueuedStatus()
			=> TestRequeue(registryStatusCode: EInvoicingPivotState.Queued, expectedStatus: EInvoicingPivotState.Queued);

		public void TestRequeue_ToPendingStatus()
			=> TestRequeue(registryStatusCode: EInvoicingPivotState.Pending, expectedStatus: EInvoicingPivotState.Pending);

		void TestRequeue(string registryStatusCode, ZString expectedStatus)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes._TemplateCountryName_;
			var parentID = new ZGuid("824978bf-51db-4875-8ffb-be0746211cb4");
			var batchPK = new ZGuid("873AA26B-AAA6-4DC0-A646-E4AE2AA39B5F");
			var testDate = ZDateTime.Now;

			var pivot = (AccEInvoicingTransactionPivot)GetNewBusinessObject();
			pivot.AIP_ParentTableCode = "AH";
			pivot.AIP_ParentID = parentID;
			pivot.AIP_AIB = batchPK;
			pivot.SetCompanyAndCountryCode(company);
			pivot.AIP_Status = Enterprise.Core.Constants.EInvoicingPivotState.Failed;
			pivot.AIP_ErrorDescription = "This transaction was rejected by IIS site";
			pivot.AIP_IsNotifiedByEmail = ZBool.True;
			pivot.AIP_LastResponseReceivedUtc = testDate;
			pivot.AIP_LastSentTimeUtc = testDate;

			AssertEquals($"Pre-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_ParentTableCode}", "AH", pivot.AIP_ParentTableCode);
			AssertEquals($"Pre-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_ParentID}", parentID, pivot.AIP_ParentID);
			AssertEquals($"Pre-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_AIB}", batchPK, pivot.AIP_AIB);
			AssertEquals($"Pre-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_GC}", company.PK, pivot.AIP_GC);
			AssertEquals($"Pre-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_RN_NKCountryCode}", company.GC_RN_NKCountryCode, pivot.AIP_RN_NKCountryCode);
			AssertEquals($"Pre-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_Status}", Core.Constants.EInvoicingPivotState.Failed, pivot.AIP_Status);
			AssertEquals($"Pre-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_ErrorDescription}", "This transaction was rejected by IIS site", pivot.AIP_ErrorDescription);
			AssertEquals($"Pre-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_IsNotifiedByEmail}", ZBool.True, pivot.AIP_IsNotifiedByEmail);
			AssertEquals($"Pre-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_LastResponseReceivedUtc}", testDate, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals($"Pre-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_LastSentTimeUtc}", testDate, pivot.AIP_LastSentTimeUtc);

			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, registryStatusCode))
			{
				pivot.Requeue();
			}

			AssertEquals($"Post-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_ParentTableCode}", "AH", pivot.AIP_ParentTableCode);
			AssertEquals($"Post-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_ParentID}", parentID, pivot.AIP_ParentID);
			AssertEquals($"Post-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_AIB}", ZGuid.Empty, pivot.AIP_AIB);
			AssertEquals($"Post-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_GC}", company.PK, pivot.AIP_GC);
			AssertEquals($"Post-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_RN_NKCountryCode}", company.GC_RN_NKCountryCode, pivot.AIP_RN_NKCountryCode);
			AssertEquals($"Post-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_Status}", expectedStatus, pivot.AIP_Status);
			AssertEquals($"Post-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_ErrorDescription}", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals($"Post-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_IsNotifiedByEmail}", ZBool.False, pivot.AIP_IsNotifiedByEmail);
			AssertEquals($"Post-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_LastResponseReceivedUtc}", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals($"Post-Condition : {AccEInvoicingTransactionPivot.Schema.AIP_LastSentTimeUtc}", ZDateTime.Empty, pivot.AIP_LastSentTimeUtc);
		}

		public void TestIsSubmitPivotWithGovernmentNumber()
		{
			var creator = new TestObjectCreator(new BusinessObjectFactory());
			var batch = creator.CreateEInvoicingBatch(101, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			batch.Factory.Save();

			var expectedPreferencesByPivotStatus = new Dictionary<string, (string assertMessage, bool expectedPropertyValue, string expectedGovernmentNumber, ZGuid batchPK)[]>()
			{
				[EInvoicingPivotState.Queued] = new (string, bool, string, ZGuid)[]
				{
					("'Queued' pivot value should be 'false'", false, "", ZGuid.Empty),
				},
				[EInvoicingPivotState.Batched] = new (string, bool, string, ZGuid)[]
				{
					("'Batched' pivot with empty government allocated number value should be 'false'", false, "", batch.PK),
				},
				[EInvoicingPivotState.BatchedWithError] = new (string, bool, string, ZGuid)[]
				{
					("'BatchedWithError' pivot with empty government allocated number value should be 'false'", false, "", batch.PK),
				},
				[EInvoicingPivotState.Sent] = new (string, bool, string, ZGuid)[]
				{
					("'Sent' pivot with empty government allocated number value should be 'false'", false, "", batch.PK),
				},
				[EInvoicingPivotState.Delivered] = new (string, bool, string, ZGuid)[]
				{
					("'Delivered' pivot with empty government allocated number value should be 'false'", false, "", batch.PK),
					("'Delivered' pivot with filled government allocated number value should be 'true'", true, "Government Allocalted Number", batch.PK),
				},
				[EInvoicingPivotState.Succeed] = new (string, bool, string, ZGuid)[]
				{
					("'Succeed' pivot with empty government allocated number value should be 'false'", false, "", batch.PK),
					("'Succeed' pivot with filled government allocated number value should be 'true'", true, "Government Allocalted Number", batch.PK),
				},
				[EInvoicingPivotState.Failed] = new (string, bool, string, ZGuid)[]
				{
					("'Failed' pivot with empty government allocated number value should be 'false'", false, "", batch.PK),
					("'Failed' pivot with filled government allocated number value should be 'false'", false, "Government Allocalted Number", batch.PK),
				},
				[EInvoicingPivotState.Discarded] = new (string, bool, string, ZGuid)[]
				{
					("'Discarded' pivot with empty government allocated number value should be 'false'", false, "", batch.PK),
					("'Discarded' pivot with filled government allocated number value should be 'false'", false, "Government Allocalted Number", batch.PK),
				},
			};

			AssertIsSubmitPivotWithGovernmentNumberMultiple(batch, expectedPreferencesByPivotStatus);
		}

		public void TestIsSubmitPivotInVietnam()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			{
				var creator = new TestObjectCreator(new BusinessObjectFactory());
				var batch = creator.CreateEInvoicingBatch(101, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				batch.Factory.Save();

				var expectedPreferencesByPivotStatus = new Dictionary<string, (string assertMessage, bool expectedPropertyValue, string expectedGovernmentNumber, ZGuid batchPK)[]>()
				{
					[EInvoicingPivotState.Delivered] = new (string, bool, string, ZGuid)[]
					{
						("'Delivered' pivot with empty government allocated number value should be 'true'", true, "", batch.PK),
						("'Delivered' pivot with filled government allocated number value should be 'true'", true, "Government Allocalted Number", batch.PK),
					},
					[EInvoicingPivotState.Succeed] = new (string, bool, string, ZGuid)[]
					{
						("'Succeed' pivot with empty government allocated number value should be 'true'", true, "", batch.PK),
						("'Succeed' pivot with filled government allocated number value should be 'true'", true, "Government Allocalted Number", batch.PK),
					},
				};

				AssertIsSubmitPivotWithGovernmentNumberMultiple(batch, expectedPreferencesByPivotStatus, false);
			}
		}

		void AssertIsSubmitPivotWithGovernmentNumberMultiple(AccEInvoicingBatch batch, Dictionary<string, (string assertMessage, bool expectedPropertyValue, string expectedGovernmentNumber, ZGuid batchPK)[]> expectedPreferencesByPivotStatus, bool isNeedGovernmentNumber = true)
		{
			foreach (var pivotStatus in expectedPreferencesByPivotStatus)
			{
				for (int i = 0; i < pivotStatus.Value.Length; i++)
				{
					batch.AIB_GovernmentAllocatedNumber = pivotStatus.Value[i].expectedGovernmentNumber;
					batch.Factory.Save();
					AssertIsSubmitPivotWithGovernmentNumberSingle(pivotStatus.Value[i].assertMessage, pivotStatus.Key, pivotStatus.Value[i].expectedGovernmentNumber, pivotStatus.Value[i].batchPK, pivotStatus.Value[i].expectedPropertyValue, isNeedGovernmentNumber);
				}
			}
		}

		void AssertIsSubmitPivotWithGovernmentNumberSingle(string assertMessage, string status, string expectedGovernmentNumber, ZGuid batchPK, bool expected, bool isNeedGovernmentNumber)
		{
			var pivot = (AccEInvoicingTransactionPivot)GetNewBusinessObject();
			pivot.FillWithValidTestData();
			pivot.AIP_ParentTableCode = "AH";
			pivot.AIP_ParentID = new ZGuid("824978bf-51db-4875-8ffb-be0746211cb4");
			pivot.AIP_Status = status;
			pivot.AIP_AIB = batchPK;

			var governmentNumber = pivot.Batch?.AIB_GovernmentAllocatedNumber ?? ZString.Empty;
			AssertEquals(expectedGovernmentNumber, governmentNumber);
			if (isNeedGovernmentNumber)
			{
				AssertEquals(assertMessage, expected, pivot.IsSubmitPivotSucceedOrDelivered && !governmentNumber.IsEmpty);
			}
			else
			{
				AssertEquals(assertMessage, expected, pivot.IsSubmitPivotSucceedOrDelivered);
			}
		}

		#region UniqueIndexFailureHandler

		public void TestUniqueIndexFailureHandler()
		{
			var parentID = new ZGuid("824978bf-51db-4875-8ffb-be0746211cb4");
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();

			var pivot1 = (AccEInvoicingTransactionPivot)GetNewBusinessObject();
			pivot1.AIP_ParentID = parentID;
			pivot1.AIP_AIB = batch.PK;
			AssertNotEquals("AIP_ActionType is not Discarded", EInvoicingPivotState.Discarded, pivot1.AIP_Status);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var pivot2 = newFactory.New<AccEInvoicingTransactionPivot>();
			pivot2.AIP_ParentID = parentID;
			AssertNotEquals("AIP_ActionType is not Discarded", EInvoicingPivotState.Discarded, pivot2.AIP_Status);

			var failureHandler = pivot2.UniqueIndexFailureHandler_ForTestOnly;
			AssertNotNull(failureHandler);

			AssertExceptionThrown("ZSaveException should be thrown", typeof(ZSaveException), () =>
			{
				newFactory.Save();
			});

			var notification = new JobInvoicing.Testing.ExchangeRateTest.NotificationHandlerForTest();
			failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single(x => x == AccEInvoicingTransactionPivotSchema.Constants.Indexes.NR_UX__AIP_ParentID_AIP_ActionType));
			AssertContains("There is already an active electronic invoicing pivot for the same action type SUB.", notification.Message);
			Assert("Error should be reported", notification.ReportErrorCount == 1 && notification.ReportInformationCount == 0);

			pivot1.AIP_Status = EInvoicingPivotState.Discarded;
			Factory.Save();

			pivot2.AIP_AIB = batch.PK;
			AssertExceptionThrown("ZSaveException should be thrown", typeof(ZSaveException), () =>
			{
				newFactory.Save();
			});

			notification.Reset();
			failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single(x => x == AccEInvoicingTransactionPivotSchema.Constants.Indexes.FK_UX__AIP_AIB_AIP_ParentID));
			AssertContains("There is already an electronic invoicing pivot for the same parent and same electronic invoicing batch.", notification.Message);
			Assert("Error should be reported", notification.ReportErrorCount == 1 && notification.ReportInformationCount == 0);

			pivot2.AIP_AIB = ZGuid.Empty;
			AssertNoExceptionThrown(() =>
			{
				newFactory.Save();
			});
		}

		#endregion

		[TestDate(2020, 12, 23, 1, 35, 42)]
		public void TestOnSavingReportsWhenStatusIsNotUpdated()
		{
			var creator = new TestObjectCreator(Factory);
			int batchNumber = 101;
			AssertOnSavingReportsWhenStatusIsNotUpdated(CountryCodes.Italy);
			AssertOnSavingReportsWhenStatusIsNotUpdated(CountryCodes.Mexico);
			AssertOnSavingReportsWhenStatusIsNotUpdated(CountryCodes.India, true);
			AssertOnSavingReportsWhenStatusIsNotUpdated(CountryCodes.Hungary);

			void AssertOnSavingReportsWhenStatusIsNotUpdated(string country, bool expectErrorReport = false)
			{
				using (creator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, creator.CreateTestPrefixAndSequenceNumberCustomisation()))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var invoice = creator.CreateARInvoice<ARInvoice>("0001", creator.AUD, 1m, creator.Debtor);
					var batch = creator.CreateEInvoicingBatch(++batchNumber, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);

					var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
					pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
					pivot.AIP_ParentID = invoice.PK;
					pivot.AIP_ActionType = EInvoicingPivotActionType.Submit;
					pivot.AIP_Status = EInvoicingPivotState.Sent;
					pivot.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-5);
					pivot.AIP_AIB = batch.PK;
					pivot.AIP_ErrorDescription = "Some description";
					AssertEquals("Precondition: country code should match", country, pivot.Company.GC_RN_NKCountryCode);

					Factory.Save();

					var newFactory = new BusinessObjectFactory();
					pivot = newFactory.Load<AccEInvoicingTransactionPivot>(pivot.PK);
					var receivedTime = ZDateTime.UtcNow;
					AssertNotEquals("AIP_LastResponseReceivedUtc is going to be updated", receivedTime, pivot.AIP_LastResponseReceivedUtc);
					pivot.AIP_LastResponseReceivedUtc = receivedTime;
					var authorisation = newFactory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
					authorisation.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
					authorisation.AHF_ParentId = invoice.PK;

					newFactory.Save();
					if (expectErrorReport)
					{
						AssertNotNull($"An error should be reported for {country}", ErrorReporter.LastMessageReported);
						var expectedMessage = $@"Submit pivot status for the AR INV {invoice.AH_TransactionNum} transaction was not updated on receiving response.
Transaction company code: EDI, post date: 23-Dec-20 01:35:00.
Pivot last sent time: 23-Dec-20 01:30:00, last response received original value:  and new value: 23-Dec-20 01:35:42, decription: Some description
Authorisation record is in database: false";
						AssertContains("Expected key", "PivotStatusNotUpdatedOnReceivingResponse_IN", ErrorReporter.LastKeyReported);
						AssertContains("Expected message", expectedMessage, ErrorReporter.LastMessageReported);
						ErrorReporter.Clear();
					}
					else
					{
						AssertNullOrEmpty($"An error should not be reported for {country}", ErrorReporter.LastMessageReported);
					}
				}
			}
		}

		public void TestTruncateAndSetErrorDescription()
		{
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();

			var description = "the error description";
			pivot.AIP_ErrorDescription = description;
			AssertEquals("Description is not truncated when less than field max length", "the error description", pivot.AIP_ErrorDescription);

			var longDescription = new string('_', AccEInvoicingTransactionPivotSchema.AIP_ErrorDescription.MaxLength);
			pivot.AIP_ErrorDescription = longDescription;
			AssertEquals("Description is not truncated when equal to field max length", longDescription, pivot.AIP_ErrorDescription);

			var tooLongDescription = new string('_', AccEInvoicingTransactionPivotSchema.AIP_ErrorDescription.MaxLength + 1);
			pivot.AIP_ErrorDescription = tooLongDescription;
			var expectedDecription = new string('_', AccEInvoicingTransactionPivotSchema.AIP_ErrorDescription.MaxLength - 1) + "…";
			AssertEquals("Description is truncated when greater than field max length", expectedDecription, pivot.AIP_ErrorDescription);
		}
	}
}
