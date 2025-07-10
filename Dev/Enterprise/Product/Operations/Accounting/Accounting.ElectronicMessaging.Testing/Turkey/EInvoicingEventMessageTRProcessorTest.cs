using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Accounting.ElectronicMessaging.Turkey.EInvoicingEventMessageTRProcessor;
using static Enterprise.Core.Constants;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class EInvoicingEventMessageTRProcessorTest : TestCaseWithFactory
	{
		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestEInvoiceResponse_MissingContextCollection()
		{
			Assert("PreCondition:AIB_GovernmentAllocatedNumber", Batch.AIB_GovernmentAllocatedNumber.IsEmpty);
			Assert("PreCondition:AIP_LastResponseReceivedUtc", Pivot.AIP_LastResponseReceivedUtc.IsEmpty);
			AssertEquals("PreCondition:AIB_Status", EInvoicingBatchState.Sent, Batch.AIB_Status);

			AssertGovernmentAllocatedNumberContextCollection();
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestEInvoiceResponse_MissingGovernmentAllocatedNumberContext()
		{
			Assert("PreCondition:AIB_GovernmentAllocatedNumber", Batch.AIB_GovernmentAllocatedNumber.IsEmpty);
			Assert("PreCondition:AIP_LastResponseReceivedUtc", Pivot.AIP_LastResponseReceivedUtc.IsEmpty);
			AssertEquals("PreCondition:AIB_Status", EInvoicingBatchState.Sent, Batch.AIB_Status);

			AssertGovernmentAllocatedNumberContextCollection(includeContextCollection: true);
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestEInvoiceResponse_MissingGovernmentAllocatedNumberValue()
		{
			Assert("PreCondition:AIB_GovernmentAllocatedNumber", Batch.AIB_GovernmentAllocatedNumber.IsEmpty);
			Assert("PreCondition:AIP_LastResponseReceivedUtc", Pivot.AIP_LastResponseReceivedUtc.IsEmpty);
			AssertEquals("PreCondition:AIB_Status", EInvoicingBatchState.Sent, Batch.AIB_Status);

			AssertGovernmentAllocatedNumberContextCollection(includeContextCollection: true, includeAllocatedNumberContext: true);
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestSubmitPivot_SuccessfulTransmission()
		{
			Assert("PreCondition:AIB_GovernmentAllocatedNumber", Batch.AIB_GovernmentAllocatedNumber.IsEmpty);
			Assert("PreCondition:AIP_LastResponseReceivedUtc", Pivot.AIP_LastResponseReceivedUtc.IsEmpty);
			AssertEquals("PreCondition:AIB_Status", EInvoicingBatchState.Sent, Batch.AIB_Status);

			AssertGovernmentAllocatedNumberContextCollection(includeContextCollection: true, includeAllocatedNumberContext: true, allocatedNumber: "112233445566");
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestProcessFailedMessageForaSucceededPivot()
		{
			Pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			Pivot.AIP_Status = EInvoicingPivotState.Succeed;
			Batch.AIB_GovernmentAllocatedNumber = "112233445566";
			Factory.Save();

			AssertGovernmentAllocatedNumberContextCollection(includeContextCollection: true, includeAllocatedNumberContext: true, allocatedNumber: "112233445566");
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestEInvoiceBatchHasNoTransactionPivots()
		{
			Batch.AIB_Status = EInvoicingBatchState.Sent;
			Pivot.Delete();
			Factory.Save();

			AssertEquals("PreCondition:TransactionPivot_Count", 0, Batch.TransactionPivots.Count);
			AssertEquals("PreCondition:AIB_Status", EInvoicingBatchState.Sent, Batch.AIB_Status);

			AssertGovernmentAllocatedNumberContextCollection(includeContextCollection: true, includeAllocatedNumberContext: true, allocatedNumber: "112233445566");
		}

		void AssertGovernmentAllocatedNumberContextCollection(bool includeContextCollection = false, bool includeAllocatedNumberContext = false, string allocatedNumber = "")
		{
			var governmentAllocatedNumberContext = string.Format(EmptyGovernmentAllocatedNumberContext, allocatedNumber);
			var contextCollection = string.Format(EmptyContextCollection, includeAllocatedNumberContext ? governmentAllocatedNumberContext : "");
			var messageText = string.Format(InvoiceReceiveEventMessage, includeContextCollection ? contextCollection : "");
			var inMessage = CreateEDIMessage(UOFactory.BOFactory, messageText);
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, Batch);

			UOFactory.SaveForTesting();

			AssertEquals("PreCondition:Log_Count", 0, logger.Logs.Count());
			Assert("PreCondition:AIP_ErrorDescription", Pivot.AIP_ErrorDescription.IsEmpty);
			ErrorReporter.Clear();
			Assert("PreCondition:ErrorReporter_LastKeyReported", ErrorReporter.LastKeyReported.IsNullOrEmpty());

			processor.Process();

			if (Batch.TransactionPivots.Count == 0)
			{
				AssertEquals("PostCondition:ErrorReporter_LastKeyReported", "EInvoicingEventMessageTRProcessor_PivotNotFound", ErrorReporter.LastKeyReported);
				AssertEquals("PostCondition:Log_Count", 1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "No update performed due to transaction pivot not found for invoice batch 1 in Company."));
				Assert("PostCondition:AIB_GovernmentAllocatedNumber", Batch.AIB_GovernmentAllocatedNumber.IsEmpty);
			}
			else if (!includeAllocatedNumberContext || allocatedNumber.IsNullOrEmpty() || !includeContextCollection)
			{
				if (!includeContextCollection)
				{
					AssertEquals("PostCondition:ErrorReporter_LastKeyReported", "EInvoicingEventMessageTRProcessor_MissingContextCollection", ErrorReporter.LastKeyReported);
					AssertEquals("PostCondition:Log_Count", 1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "E-Invoice response was not processed due to universal event message was not having context collection for invoice batch 1 in Company."));
				}
				else
				{
					AssertEquals("PostCondition:ErrorReporter_LastKeyReported", "EInvoicingEventMessageTRProcessor_GovernmentAllocatedNumberContextHasNoValue", ErrorReporter.LastKeyReported);
					AssertEquals("PostCondition:Log_Count", 1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "Government Allocated Number was not updated due to Government Allocated Number Context value was empty for invoice batch 1 in Company."));
				}
				AssertEquals("PostCondition:AIB_GovernmentAllocatedNumber", allocatedNumber, Batch.AIB_GovernmentAllocatedNumber);
				AssertEquals("PostCondition:AIP_LastResponseReceivedUtc", TestDateAttribute.Date, Pivot.AIP_LastResponseReceivedUtc);
				AssertEquals("Postcondition:AIP_Status", EInvoicingPivotState.Failed, Pivot.AIP_Status);
				AssertNull("Status action pivot is NOT created after unsuccessful process.", GetPivotCreatedAfterSubmitActionProcess(Pivot, EInvoicingPivotActionType.StatusCheck));
			}
			else if ((Pivot?.AIP_Status ?? "") == EInvoicingPivotState.Succeed)
			{
				AssertEquals("PostCondition:Log_Count", 1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Warning && x.Message == "No update performed due to transaction pivot having 'SUC' status for invoice batch 1 in Company."));
				AssertEquals("PostCondition:AIP_LastResponseReceivedUtc", ZDateTime.BrettsBirthday, Pivot.AIP_LastResponseReceivedUtc);
				AssertEquals("PostCondition:AIB_GovernmentAllocatedNumber", allocatedNumber, Batch.AIB_GovernmentAllocatedNumber);
				AssertEquals("PostCondition:AIP_Status", EInvoicingPivotState.Succeed, Pivot.AIP_Status);
				Assert("PostCondition:AIP_ErrorDescription", Pivot.AIP_ErrorDescription.IsEmpty);
			}
			else
			{
				AssertEquals("PostCondition:Log_Count", 0, logger.Logs.Count());
				AssertEquals("PostCondition:AIB_GovernmentAllocatedNumber", allocatedNumber, Batch.AIB_GovernmentAllocatedNumber);
				AssertEquals("PostCondition:AIP_LastResponseReceivedUtc", TestDateAttribute.Date, Pivot.AIP_LastResponseReceivedUtc);
				AssertEquals("PostCondition:AIP_Status", EInvoicingPivotState.Delivered, Pivot.AIP_Status);
				Assert("PostCondition:AIP_ErrorDescription", Pivot.AIP_ErrorDescription.IsEmpty);
				AssertNotNull("Status action pivot is created after successful process.", GetPivotCreatedAfterSubmitActionProcess(Pivot, EInvoicingPivotActionType.StatusCheck));
			}

			ErrorReporter.Clear();
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestEArchiveInvoiceCancel_MissingContextCollection() =>
			AssertEArchiceInvoiceCancelMessageProcessing(includeContextCollection: false);

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestEArchiveInvoiceCancel_CannotCancelIvoice() =>
			AssertEArchiceInvoiceCancelMessageProcessing(includeContextCollection: true, isCancelledContext: "false", messageContext: "Cannot Cancel due to testing.");

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestEArchiveInvoiceCancel_MissingTransaction() =>
			AssertEArchiceInvoiceCancelMessageProcessing(includeContextCollection: true, isCancelledContext: "true", messageContext: "Cancelled", missingPivot: true);

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestEArchiveInvoiceCancel_CancelSuccessful() =>
			AssertEArchiceInvoiceCancelMessageProcessing(includeContextCollection: true, isCancelledContext: "true", messageContext: "Cancelled");

		void AssertEArchiceInvoiceCancelMessageProcessing(bool includeContextCollection = false, string isCancelledContext = "", string messageContext = "", bool missingPivot = false)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			{
				Pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
				Pivot.AIP_Status = EInvoicingPivotState.Delivered;
				Batch.AIB_GovernmentAllocatedNumber = "112233445566";
				Factory.Save();

				var reverseTransaction = HelperTR.CreateReverseTransaction(Invoice);
				var cancelBatch = HelperTR.TestObjectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, HelperTR.TurkeyBranch.Company);
				var cancelPivot = missingPivot ? null : HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(cancelBatch, reverseTransaction, EInvoicingPivotState.Delivered, EInvoicingPivotActionType.Cancel);

				var cancelledContext = string.Format(EmptyIsCancelledContext, isCancelledContext, messageContext);
				var contextCollection = string.Format(EmptyContextCollection, cancelledContext);
				var messageText = string.Format(InvoiceReceiveEventMessage, includeContextCollection ? contextCollection : "");
				var inMessage = CreateEDIMessage(UOFactory.BOFactory, messageText);
				var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
				IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
				var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, cancelBatch);
				var expectedFileName = TurkeyEInvoiceAPICommandList.Codes.CancelReceivablesInvoice + "_IAK_" + ZDateTime.UtcNow.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture) + ".xml";

				AssertEquals("PreCondition:Log_Count", 0, logger.Logs.Count());
				Assert("PreCondition:AIP_ErrorDescription", cancelPivot?.AIP_ErrorDescription.IsEmpty ?? true);
				ErrorReporter.Clear();
				Assert("PreCondition:ErrorReporter_LastKeyReported", ErrorReporter.LastKeyReported.IsNullOrEmpty());

				processor.Process();

				if (cancelBatch.TransactionPivots.Count == 0)
				{
					AssertEquals("PostCondition:ErrorReporter_LastKeyReported", "EInvoicingEventMessageTRProcessor_PivotNotFound", ErrorReporter.LastKeyReported);
					AssertEquals("PostCondition:Log_Count", 1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "No update performed due to transaction pivot not found for invoice batch 1 in Company."));
				}
				else if (!includeContextCollection)
				{
					AssertEquals("PostCondition:ErrorReporter_LastKeyReported", "EInvoicingEventMessageTRProcessor_MissingContextCollectionOfCancellation", ErrorReporter.LastKeyReported);
					AssertEquals("PostCondition:Log_Count", 1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "E-Archive invoice cancel response was not processed due to universal event message was not having context collection for invoice batch 1 in Company."));
					AssertEquals("PostCondition:AIP_LastResponseReceivedUtc", ZDateTime.BrettsBirthday, Pivot.AIP_LastResponseReceivedUtc);
					AssertEquals("Postcondition:AIP_Status", EInvoicingPivotState.Failed, cancelPivot.AIP_Status);
				}
				else if ((cancelPivot?.AIP_Status ?? "") == EInvoicingPivotState.Succeed)
				{
					AssertEquals("PostCondition:AIP_LastResponseReceivedUtc", TestDateAttribute.Date, cancelPivot.AIP_LastResponseReceivedUtc);
					AssertEquals("PostCondition:AIP_Status", EInvoicingPivotState.Failed, Pivot.AIP_Status);
					Assert("PostCondition:AIP_ErrorDescription", !cancelPivot.AIP_ErrorDescription.IsEmpty);
					AssertEquals("No records in cancelled invoice eDocs", 0, Invoice.DocManagerInfo.AllEDocs.Count);
					AssertEquals("RCN XML added to Credit Note eDocs", 1, reverseTransaction.DocManagerInfo.AllEDocs.Count);
					AssertEquals(expectedFileName, reverseTransaction.DocManagerInfo.AllEDocs[0].FileName);
				}
				else
				{
					AssertEquals("PostCondition:Log_Count", 0, logger.Logs.Count());
					AssertEquals("PostCondition:AIP_LastResponseReceivedUtc", TestDateAttribute.Date, cancelPivot.AIP_LastResponseReceivedUtc);
					AssertEquals("PostCondition:AIP_Status", EInvoicingPivotState.Failed, cancelPivot.AIP_Status);
					Assert("PostCondition:AIP_ErrorDescription", !cancelPivot.AIP_ErrorDescription.IsEmpty);
					AssertEquals("No records in cancelled invoice eDocs", 0, Invoice.DocManagerInfo.AllEDocs.Count);
					AssertEquals("RCN XML added to Credit Note eDocs", 1, reverseTransaction.DocManagerInfo.AllEDocs.Count);
					AssertEquals(expectedFileName, reverseTransaction.DocManagerInfo.AllEDocs[0].FileName);
					var statusPivot = Invoice.Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(
												new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, Invoice.PK),
												new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck))).FirstOrDefault();
					AssertNotNull(statusPivot);
				}

				ErrorReporter.Clear();
			}
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIRJEventMessageProcessingForSubmit()
		{
			var inMessage = CreateEDIMessage(UOFactory.BOFactory, IRJReasonMessageBuilder("false"));
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, Batch);

			AssertEquals("PreCondition:Log_Count", 0, logger.Logs.Count());
			Assert("PreCondition:AIB_GovernmentAllocatedNumber", Batch.AIB_GovernmentAllocatedNumber.IsEmpty);
			Assert("PreCondition:AIP_LastResponseReceivedUtc", Pivot.AIP_LastResponseReceivedUtc.IsEmpty);
			AssertEquals("PreCondition:AIP_Status", EInvoicingPivotState.Sent, Pivot.AIP_Status);
			Assert("PreCondition:AIP_ErrorDescription", Pivot.AIP_ErrorDescription.IsEmpty);

			processor.Process();

			AssertEquals("PostCondition:Log_Count", 1, logger.Logs.Count());
			Assert("PostCondition:AIB_GovernmentAllocatedNumber", Batch.AIB_GovernmentAllocatedNumber.IsEmpty);
			AssertEquals("PostCondition:AIP_LastResponseReceivedUtc", TestDateAttribute.Date, Pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("PostCondition:AIP_Status", EInvoicingPivotState.Failed, Pivot.AIP_Status);
			AssertEquals("PostCondition:AIP_ErrorDescription", testResponseMessage, Pivot.AIP_ErrorDescription);
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestFlaggedForRetryMessagesDoesntWorkWhenRegistryValueIsZero()
		{
			var pivotForAutomatedRetry = SetUpAndGetRetryPivot(maxRetry: 0, expectRetry: false);
			AssertNull("There shouldn't be a pivot for automatic retries when registry is set to 0", pivotForAutomatedRetry);
			AssertEquals("Pivot should be in failed status.", EInvoicingPivotState.Failed, Pivot.AIP_Status);
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestFlaggedForRetryMessagesWorkOnlyWhenRegistryValueIsNotZero()
		{
			var pivotForAutomatedRetry = SetUpAndGetRetryPivot(ZString.Empty, maxRetry: 5);
			AssertSuccessfullRetryPivot(pivotForAutomatedRetry, "1");
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestRetryCounterIncrementsForFlaggedTrasactions()
		{
			var pivotForAutomatedRetry = SetUpAndGetRetryPivot(HelperTR.RetryIdentifierForTest + "2", maxRetry: 5);
			AssertSuccessfullRetryPivot(pivotForAutomatedRetry, "3");
		}

		void AssertSuccessfullRetryPivot(AccEInvoicingTransactionPivot pivotForAutomatedRetry, string incrementedValue)
		{
			AssertNotNull("There must be a pivot for automated retries", pivotForAutomatedRetry);
			var expectedErrorDescriptionForFirstRetry = HelperTR.RetryIdentifierForTest + incrementedValue;
			AssertEquals("Automated retry pivot must have incremented counter", expectedErrorDescriptionForFirstRetry, pivotForAutomatedRetry.AIP_ErrorDescription);
			AssertEquals("Automated retry pivot must be queued", EInvoicingPivotState.Queued, pivotForAutomatedRetry.AIP_Status);
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestRetryDoesntWorkWhenLimitExceeded()
		{
			var pivotForAutomatedRetry = SetUpAndGetRetryPivot(HelperTR.RetryIdentifierForTest + "5", maxRetry: 5, expectRetry: false);
			AssertNull("There shouldn't be a pivot for automatic retries when registry value is exceeded", pivotForAutomatedRetry);
		}

		AccEInvoicingTransactionPivot SetUpAndGetRetryPivot(ZString? originalDescription = null, int maxRetry = 5, bool expectRetry = true)
		{
			var dataSetup = SetupDataForRetryTest();
			Pivot.AIP_ErrorDescription = originalDescription ?? ZString.Empty;

			using (Registry.AccountingElectronicMessagingRegistry.Instance.EReportingAutomaticRetryLimit.SetTemporaryValue(dataSetup.inMessage.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, maxRetry))
			{
				dataSetup.processor.Process();
				Batch.Factory.Save();

				//Asserting old pivot and batch after process
				AssertEquals("PostProcessCondition:AIP_LastResponseReceivedUtc", TestDateAttribute.Date, Pivot.AIP_LastResponseReceivedUtc);
				var expectedProcessedPivotStatus = expectRetry ? EInvoicingPivotState.Discarded : EInvoicingPivotState.Failed;
				AssertEquals("PostProcessCondition:AIP_Status", expectedProcessedPivotStatus, Pivot.AIP_Status);

				if (originalDescription.HasValue && expectRetry)
				{
					AssertEquals("PostCondition:AIP_ErrorDescription", originalDescription.Value, Pivot.AIP_ErrorDescription);
				}

				AssertEquals("PostProcessCondition:AIP_LastResponseReceivedUtc", TestDateAttribute.Date, Pivot.AIP_LastResponseReceivedUtc);

				var result = GetPivotByParentIdTypeAndStatus(Pivot.AIP_ParentID, Pivot.AIP_ActionType, EInvoicingPivotState.Queued);
				AssertEquals("Expect retry pivot created", expectRetry, result != null);

				return result;
			}
		}

		public void TestRetryCounterNotBeingANumberCausesError()
		{
			var dataSetup = SetupDataForRetryTest();

			// Editing the pivot error description to have a non number value
			Pivot.AIP_ErrorDescription = HelperTR.RetryIdentifierForTest + "test value";
			dataSetup.processor.Process();

			AssertEquals(dataSetup.logger.Logs.First().ToString(), "Error - Retry counter value is not a number. Value: Unexpected Error - Attempting Retry #test value");
			ErrorReporter.Clear();
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIRJEventMessageProcessingForStatusRequestWithErrorStatus() => TestIRJEventMessageProcessingForStatusRequest();

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIRJEventMessageProcessingForStatusRequestWithoutErrorStatus() => TestIRJEventMessageProcessingForStatusRequest(false);

		void TestIRJEventMessageProcessingForStatusRequest(bool hasErrorStatus = true)
		{
			var submitPivot = Pivot;
			Batch.AIB_GovernmentAllocatedNumber = "Government Allocated Number";
			submitPivot.AIP_ErrorDescription = "";
			submitPivot.AIP_Status = EInvoicingPivotState.Delivered;
			HelperTR.Factory.Save();

			var statusBatch = HelperTR.TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany, true);
			var statusPivot = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(statusBatch, Invoice, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
			HelperTR.Factory.Save();

			var message = InvoiceReceiveEventFailedMessageForStatusRequest.Replace("<Value>StatusCodeValue</Value>", hasErrorStatus ? "<Value>2000</Value>" : "<Value></Value>");
			var inMessage = CreateEDIMessage(UOFactory.BOFactory, message);
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, statusBatch);

			AssertEquals("PreCondition:Log_Count", 0, logger.Logs.Count());
			Assert("Status Check Pivot AIP_LastResponseReceivedUtc", statusPivot.AIP_LastResponseReceivedUtc.IsEmpty);
			AssertEquals("Status Check Pivot AIP_Status", EInvoicingPivotState.Sent, statusPivot.AIP_Status);
			AssertEquals("Submit Pivot AIP_Status", EInvoicingPivotState.Delivered, submitPivot.AIP_Status);
			Assert("Status Check Pivot AIP_ErrorDescription", statusPivot.AIP_ErrorDescription.IsEmpty);
			Assert("Submit Pivot AIP_ErrorDescription", submitPivot.AIP_ErrorDescription.IsEmpty);

			processor.Process();

			AssertEquals("PostCondition:Log_Count", 1, logger.Logs.Count());
			AssertEquals("Status Check Pivot AIP_LastResponseReceivedUtc", TestDateAttribute.Date, statusPivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Status Check Pivot AIP_Status", EInvoicingPivotState.Failed, statusPivot.AIP_Status);
			var expectedMessage = "Refer to the Uyumsoft Portal for more information";
			AssertEquals("Status Check Pivot AIP_ErrorDescription", expectedMessage, statusPivot.AIP_ErrorDescription);
			AssertEquals("Submit Pivot AIP_ErrorDescription", "STA:" + expectedMessage, submitPivot.AIP_ErrorDescription);
			AssertEquals("Submit Pivot AIP_Status", (hasErrorStatus ? EInvoicingPivotState.Failed : EInvoicingPivotState.Delivered), submitPivot.AIP_Status);
		}

		public void TestStatusCheckPutsUyumsoftStatusDescriptionOnSubmitPivot()
		{
			var submitPivot = Pivot;
			Batch.AIB_GovernmentAllocatedNumber = "Government Allocated Number";
			submitPivot.AIP_Status = EInvoicingPivotState.Delivered;
			HelperTR.Factory.Save();

			var eInvoicingStatuses = new EInvoicingStatuses();
			foreach (CodeDescriptionPair item in eInvoicingStatuses)
			{
				var statusBatch = HelperTR.TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany, true);
				var statusPivot = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(statusBatch, Invoice, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
				submitPivot.AIP_ErrorDescription = "";
				submitPivot.AIP_Status = EInvoicingPivotState.Delivered;
				statusPivot.AIP_LastResponseReceivedUtc = DateTime.UtcNow.AddSeconds(-1);
				HelperTR.Factory.Save();

				var messageText = string.Format(StatusRequestResponseMessage, "100", item.Code);
				var responseMessage = CreateEDIMessage(UOFactory.BOFactory, messageText);
				var responseEvent = responseMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
				IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
				var processor = new EInvoicingEventMessageTRProcessor(logger, responseMessage, responseEvent, statusBatch);

				AssertEquals(item.Description + " Status Check Pivot AIP_Status", EInvoicingPivotState.Sent, statusPivot.AIP_Status);
				AssertEquals(item.Description + " Submit Pivot AIP_Status", EInvoicingPivotState.Delivered, submitPivot.AIP_Status);
				Assert(item.Description + " Status Check Pivot AIP_ErrorDescription", statusPivot.AIP_ErrorDescription.IsEmpty);
				Assert(item.Description + " Submit Pivot AIP_ErrorDescription", submitPivot.AIP_ErrorDescription.IsEmpty);

				var responseTimeBeforeServiceTask = statusPivot.AIP_LastResponseReceivedUtc;
				processor.Process();

				if (item.Code == EInvoicingStatuses.Codes.Approved)
				{
					AssertEquals(item.Description + " Status Check Pivot AIP_Status", EInvoicingPivotState.Succeed, statusPivot.AIP_Status);
					AssertEquals(item.Description + " Submit Pivot AIP_Status", EInvoicingPivotState.Succeed, submitPivot.AIP_Status);
				}
				else if (item.Code == EInvoicingStatuses.Codes.Cancelled
					|| item.Code == EInvoicingStatuses.Codes.Declined
					|| item.Code == EInvoicingStatuses.Codes.Return
					|| item.Code == EInvoicingStatuses.Codes.EArchiveCancelled)
				{
					AssertEquals(item.Description + " Status Check Pivot AIP_Status", EInvoicingPivotState.Succeed, statusPivot.AIP_Status);
					AssertEquals(item.Description + " Submit Pivot AIP_Status", EInvoicingPivotState.Failed, submitPivot.AIP_Status);
				}
				else
				{
					AssertEquals(item.Description + " Status Check Pivot AIP_Status", EInvoicingPivotState.Sent, statusPivot.AIP_Status);
					AssertEquals(item.Description + " Submit Pivot AIP_Status", EInvoicingPivotState.Delivered, submitPivot.AIP_Status);
					Assert(!statusPivot.AIP_LastResponseReceivedUtc.IsEmpty);
					AssertNotEquals(responseTimeBeforeServiceTask, statusPivot.AIP_LastResponseReceivedUtc);
				}
				AssertEquals(item.Description + " Uymsoft Status Description set on AIP_ErrorDescription", item.Description, submitPivot.AIP_ErrorDescription);
				AssertEquals(item.Description + " Status Check Pivot AIP_ErrorDescription is not set", "", statusPivot.AIP_ErrorDescription);
			}
		}

		protected override void SetUp()
		{
			HelperTR = new TurkeyEInvoiceTestHelper();
			uoFactory = new UniversalObjectFactory();

			using (HelperTR.TestObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(HelperTR.TurkeyBranch.PK.ToGuid(), DateTime.Now.AddDays(-10)))
			{
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(HelperTR.TurkeyBranch.Company.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				base.SetUp();

				Invoice = HelperTR.CreateARINVTransactions(HelperTR.TurkeyBranch, HelperTR.TestObjectCreator.KDV18);
				Batch = HelperTR.TestObjectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				Pivot = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(Batch, Invoice, EInvoicingPivotState.Sent);
				HelperTR.Factory.Save();
			}
		}

		public void TestDocumentActionPivot_MissingAttachedDocumentCollection()
		{
			AssertAttachedDocumentCollection("00001000", 100);
		}

		public void TestDocumentActionPivot_EmptyAttachedDocumentCollection()
		{
			AssertAttachedDocumentCollection("00001001", 200, false);
		}

		void AssertAttachedDocumentCollection(string transactionNumber, ZInt batchNumber, bool isMissing = true)
		{
			(var invoice, var batch, var pivot) = CreateInvoiceBatchAndPivot(transactionNumber, batchNumber);
			var messageText = string.Format(EventMessage, batchNumber, isMissing ? "" : EmptyAttachedDocumentCollection);
			var inMessage = CreateEDIMessage(UOFactory.BOFactory, messageText);
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, batch);

			UOFactory.SaveForTesting();

			AssertEquals("Precondition", 0, logger.Logs.Count());
			ErrorReporter.Clear();
			AssertEquals("Precondition", ZString.Empty, ErrorReporter.LastKeyReported);
			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", EInvoicingPivotState.Delivered, pivot.AIP_Status);

			processor.Process();

			universalEvent.Dispose();

			AssertEquals("PostCondition", "EInvoicingEventMessageTRProcessor_AttachmentNotFound", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == string.Format("Response Message does not have attached document. Response Message Context for invoice batch {0} in Eagle Datamation International.", batchNumber)));
			AssertEquals("Postcondition", EInvoicingPivotState.Failed, pivot.AIP_Status);
			ErrorReporter.Clear();
		}

		public void TestIsPDFRequestPivotCreated()
		{
			(var invoice, var batch, var pivot) = CreateInvoiceBatchAndPivot("00001000", 100, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Sent);
			var governmentAllocatedNumberContext = string.Format(EmptyGovernmentAllocatedNumberContext, "Government Allocated Number");
			var contextCollection = string.Format(EmptyContextCollection, governmentAllocatedNumberContext);
			var messageText = string.Format(EventMessage, 100, contextCollection);
			CreateEDIMessage(UOFactory.BOFactory, messageText);

			UOFactory.SaveForTesting();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };

			var postFactory = new BusinessObjectFactory();
			var pivotDocumentAction = postFactory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued));
			AssertNull(pivotDocumentAction);

			serviceTask.RunTask();

			pivotDocumentAction = postFactory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued));
			AssertNotNull(pivotDocumentAction);

			ErrorReporter.Clear();
		}

		[TestDate(2018, 01, 17, 20, 35, 00)]
		public void TestDocumentActionPivot_SuccessfulTransmission()
		{
			(var invoice, var batch, var pivot) = CreateInvoiceBatchAndPivot("00001000", 100);
			var messageText = string.Format(EventMessage, 100, AttachedDocumentCollectionWithDocument);
			CreateEDIMessage(UOFactory.BOFactory, messageText);
			UOFactory.SaveForTesting();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			TestServiceLogger logger = (TestServiceLogger)serviceTask.ServiceLogger;

			AssertEquals("Postcondition", 0, logger.Count);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Delivered, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			var postServicePivot = newFactory.Load<AccEInvoicingTransactionPivot>(pivot.PK);
			var postServiceInvoice = newFactory.Load<AccTransactionHeader>(invoice.PK);

			AssertEquals("PostCondition", "", ErrorReporter.LastKeyReported);
			AssertEquals("Postcondition", 14, logger.Count);
			AssertEquals("Postcondition", TestDateAttribute.Date, postServicePivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", EInvoicingPivotState.Succeed, postServicePivot.AIP_Status);
			AssertEquals("Postcondition", "", postServicePivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 1, postServiceInvoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		[TestDate(2018, 01, 17, 20, 35, 00)]
		public void TestDocumentActionPivot_DocumentTypeIsSetSuccessfulAccordingToRegistry()
		{
			var (invoice, _, _) = CreateInvoiceBatchAndPivot("00001000", 100);
			var expectedDocType = RefDocTypes.Cotton;

			AccountingConfigurationRegistry.Instance.ThirdPartyEInvoiceDocType.SetTemporaryValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, expectedDocType);
			var messageText = string.Format(EventMessage, 100, AttachedDocumentCollectionWithDocument);
			var inMessage = CreateEDIMessage(UOFactory.BOFactory, messageText);
			using (var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>())
			{
				UOFactory.SaveForTesting();

				var receivedDocType = universalEvent.AttachedDocumentCollection.FirstOrDefault(x => x.FileName.Value.ToUpper().Contains("PDF")).Type;
				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };

				AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Precondition", RefDocTypes.MiscellaneousDocument, receivedDocType.Code);

				serviceTask.RunTask();

				var newFactory = new BusinessObjectFactory();
				var postServiceInvoice = newFactory.Load<AccTransactionHeader>(invoice.PK);

				var doc = postServiceInvoice.DocManagerInfo.AllEDocs
					.Cast<DocumentScanning.Business.StorageFile>()
					.FirstOrDefault(y => Path.GetExtension(y.SC_FileNameWithExtension).ToUpper().Contains("PDF"));
				AssertEquals("Postcondition", 1, postServiceInvoice.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Postcondition", expectedDocType, doc.SC_DocType);
			}
			ErrorReporter.Clear();
		}

		public void TestDocumentActionProcessFailedMessageForASucceededPivot()
		{
			(var invoice, var batch, var pivot) = CreateInvoiceBatchAndPivot("00001000", 100);
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			pivot.AIP_Status = EInvoicingPivotState.Succeed;
			var messageText = string.Format(EventMessage, 100, AttachedDocumentCollectionWithDocument);
			var inMessage = CreateEDIMessage(UOFactory.BOFactory, messageText);
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, batch);

			UOFactory.SaveForTesting();

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Succeed, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);

			processor.Process();

			universalEvent.Dispose();

			AssertEquals("PostCondition", "", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Warning && x.Message == "No update performed due to transaction pivot having 'SUC' status for invoice batch 100 in Eagle Datamation International."));
			AssertEquals("Postcondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", EInvoicingPivotState.Succeed, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			ErrorReporter.Clear();
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestPILResponseHandling_IfIAK() => AssertPILResponseHandling("IAK");

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestPILResponseHandling_IfIRJ() => AssertPILResponseHandling("IRJ");

		public void AssertPILResponseHandling(string eventType)
		{
			var batch = HelperTR.TestObjectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany, "TR:AP:GetInboxInvoiceListBatch");
			batch.Factory.Save();
			var inMessage = CreateEDIMessage(UOFactory.BOFactory, PILResponseEventMessage.Replace("{eventType}", eventType));
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, batch);

			UOFactory.SaveForTesting();

			AssertEquals("Precondition", 0, logger.Logs.Count());

			TestDateAttribute.AddMinutes(5);

			processor.Process();

			universalEvent.Dispose();

			AssertEquals("PostCondition", "", ErrorReporter.LastKeyReported);
			AssertEquals("PostCondition", 0, logger.Logs.Count());
			AssertNotEquals("Postcondition", batch.AIB_SystemCreateTimeUtc, batch.AIB_SystemLastEditTimeUtc);
			AssertEquals("Postcondition", EInvoicingBatchState.Discarded, batch.AIB_Status);
			ErrorReporter.Clear();
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIAKEventMessageProcessingForConfirmTransactionReceivedRequest() =>
			AssertEventMessageProcessingForConfirmTransactionReceivedRequest(SetRequestSuccessMessage, 0, EInvoicingPivotState.Succeed, "");

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIRJEventMessageProcessingForConfirmTransactionReceivedRequestRetriable()
		{
			var invoice = AssertEventMessageProcessingForConfirmTransactionReceivedRequest(IRJReasonMessageBuilder("true"), 0, EInvoicingPivotState.Discarded, "");

			var newFactory = new BusinessObjectFactory();
			var newPivot = newFactory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded))
				.FirstOrDefault();
			AssertNotNull(newPivot);
			AssertEquals(EInvoicingPivotState.Queued, newPivot.AIP_Status);
			AssertEquals(EInvoicingPivotActionType.ConfirmTransactionReceived, newPivot.AIP_ActionType);
			AssertEquals("Unexpected Error - Attempting Retry #1", newPivot.AIP_ErrorDescription);
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIRJEventMessageProcessingForConfirmTransactionReceivedRequestNoRetry()
		{
			var invoice = AssertEventMessageProcessingForConfirmTransactionReceivedRequest(IRJReasonMessageBuilder(), 1, EInvoicingPivotState.Failed, "This is my test response message");

			var newFactory = new BusinessObjectFactory();
			var failedPivot = newFactory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded))
				.FirstOrDefault();
			AssertNotNull(failedPivot);
			AssertEquals(EInvoicingPivotState.Failed, failedPivot.AIP_Status);
			AssertEquals(EInvoicingPivotActionType.ConfirmTransactionReceived, failedPivot.AIP_ActionType);
			AssertEquals("This is my test response message", failedPivot.AIP_ErrorDescription);
		}

		TransactionPendingAllocation AssertEventMessageProcessingForConfirmTransactionReceivedRequest(string eventMessage, int expectedLogCount, string expectedPivotState, string expectedErrorMessage)
		{
			var invoice = HelperTR.Factory.NewWithValidTestData<TransactionPendingAllocation>();
			invoice.AH_TransactionNum = "INV2022000000001";
			invoice.AH_GovernmentAllocatedID = HelperTR.GovermentAllocatedNumberForTest;
			var pivot = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, EInvoicingPivotActionType.ConfirmTransactionReceived, EInvoicingPivotState.Sent);
			var batch = HelperTR.TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1000, EInvoicingBatchState.Sent);
			batch.AIB_GovernmentAllocatedNumber = HelperTR.GovermentAllocatedNumberForTest;
			HelperTR.Factory.Save();

			var inMessage = CreateEDIMessage(UOFactory.BOFactory, eventMessage);
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, batch);

			AssertEquals("PreCondition:Log_Count", 0, logger.Logs.Count());
			Assert("PreCondition:AIB_GovernmentAllocatedNumber", !batch.AIB_GovernmentAllocatedNumber.IsEmpty);
			Assert("PreCondition:AIP_LastResponseReceivedUtc", pivot.AIP_LastResponseReceivedUtc.IsEmpty);
			AssertEquals("PreCondition:AIP_Status", EInvoicingPivotState.Sent, pivot.AIP_Status);
			Assert("PreCondition:AIP_ErrorDescription", pivot.AIP_ErrorDescription.IsEmpty);

			processor.Process();

			HelperTR.Factory.Save();

			universalEvent.Dispose();

			AssertEquals("PostCondition:Log_Count", expectedLogCount, logger.Logs.Count());
			Assert("PostCondition:AIB_GovernmentAllocatedNumber", !batch.AIB_GovernmentAllocatedNumber.IsEmpty);
			AssertEquals("PostCondition:AIP_LastResponseReceivedUtc", TestDateAttribute.Date, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("PostCondition:AIP_Status", expectedPivotState, pivot.AIP_Status);
			AssertEquals("PostCondition:AIP_ErrorDescription", expectedErrorMessage, pivot.AIP_ErrorDescription);

			return invoice;
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIAKEventMessageProcessingForApproveRequest() =>
			AssertEventMessageProcessingForApproveRequest(
				eventMessage: SendDocumentResponse_ApprovalAccepted,
				expectedLogCount: 1,
				expectedLogMessages: new string[] { "The invoice is approved successfully. Invoice Number: INV2022000000001" },
				expectedPivotState: EInvoicingPivotState.Succeed,
				expectedApprovalStatus: GenApprovalRequestApprovalStatus.Approved,
				pivotActionType: EInvoicingPivotActionType.Approve,
				isInvoiceCancelled: false,
				expectedErrorDescription: "");

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIAKEventMessageProcessingForApproveRequestWithAutoApprovedResult() =>
			AssertEventMessageProcessingForApproveRequest(
				eventMessage: SendDocumentResponse_ApprovalDeclinedAsAlreadyApproved,
				expectedLogCount: 1,
				expectedLogMessages: new string[] { "The invoice is already 'Approved' by the government. Invoice Number: INV2022000000001" },
				expectedPivotState: EInvoicingPivotState.Succeed,
				expectedApprovalStatus: GenApprovalRequestApprovalStatus.Approved,
				isInvoiceCancelled: false,
				pivotActionType: EInvoicingPivotActionType.Approve,
				expectedErrorDescription: "Faturanın şuanki durumu yanıt vermek için uygun değil, Durum: Approved.EFT-TESTPORTAL");

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIAKEventMessageProcessingForApproveRequestWithRejectedResult() =>
			AssertEventMessageProcessingForApproveRequest(
				eventMessage: SendDocumentResponse_ApprovalDeclinedAsAlreadyRejected,
				expectedLogCount: 1,
				expectedLogMessages: new string[] { "The invoice is already 'Rejected'. You cannot approve it. Invoice Number: INV2022000000001" },
				expectedPivotState: EInvoicingPivotState.Succeed,
				expectedApprovalStatus: GenApprovalRequestApprovalStatus.Rejected,
				isInvoiceCancelled: true,
				pivotActionType: EInvoicingPivotActionType.Approve,
				expectedErrorDescription: "Faturanın şuanki durumu yanıt vermek için uygun değil, Durum: Declined.EFT-TESTPORTAL");

		void AssertEventMessageProcessingForApproveRequest(string eventMessage, int expectedLogCount, string[] expectedLogMessages, string expectedPivotState, string expectedErrorDescription, string pivotActionType = EInvoicingPivotActionType.Reject, bool isInvoiceCancelled = true, string expectedApprovalStatus = GenApprovalRequestApprovalStatus.Rejected, bool assertIsRetried = false)
		{
			var invoice = HelperTR.Factory.NewWithValidTestData<TransactionPendingAllocation>();
			invoice.AH_TransactionNum = "INV2022000000001";
			invoice.AH_GovernmentAllocatedID = HelperTR.GovermentAllocatedNumberForTest;
			var pivotCRX = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, EInvoicingPivotActionType.ConfirmTransactionReceived, EInvoicingPivotState.Succeed);
			var batchCRX = HelperTR.TestObjectCreator.CreateEInvoicingBatchForPivot(pivotCRX, 1000, EInvoicingBatchState.Sent);
			batchCRX.AIB_GovernmentAllocatedNumber = HelperTR.GovermentAllocatedNumberForTest;
			HelperTR.Factory.Save();

			invoice.TransactionRelatedApprovalRequest.XP_ApprovalStatus = pivotActionType == EInvoicingPivotActionType.Reject ? GenApprovalRequestApprovalStatus.RejectionRequested : GenApprovalRequestApprovalStatus.ApprovalRequested;

			var pivotREQ = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, pivotActionType, EInvoicingPivotState.Sent);
			var batchREQ = HelperTR.TestObjectCreator.CreateEInvoicingBatchForPivot(pivotREQ, 1001, EInvoicingBatchState.Sent);
			batchREQ.AIB_GovernmentAllocatedNumber = HelperTR.GovermentAllocatedNumberForTest;
			HelperTR.Factory.Save();

			var inMessage = CreateEDIMessage(UOFactory.BOFactory, eventMessage);
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, batchREQ);

			AssertEquals("PreCondition:Log_Count", 0, logger.Logs.Count());
			Assert("PreCondition:AIB_GovernmentAllocatedNumber", !batchREQ.AIB_GovernmentAllocatedNumber.IsEmpty);
			Assert("PreCondition:AIP_LastResponseReceivedUtc", pivotREQ.AIP_LastResponseReceivedUtc.IsEmpty);
			AssertEquals("PreCondition:AIP_Status", EInvoicingPivotState.Sent, pivotREQ.AIP_Status);
			Assert("PreCondition:AIP_ErrorDescription", pivotREQ.AIP_ErrorDescription.IsEmpty);

			processor.Process();

			HelperTR.Factory.Save();

			universalEvent.Dispose();

			AssertEquals("PostCondition:Log_Count", expectedLogCount, logger.Logs.Count());
			foreach (var log in expectedLogMessages)
			{
				Assert("PostCondition:Log Message", logger.Logs.ToList().First(x => x.Message.Contains(log)) != null);
			}
			Assert("PostCondition:AIB_GovernmentAllocatedNumber", !batchREQ.AIB_GovernmentAllocatedNumber.IsEmpty);
			AssertEquals("PostCondition:AIP_LastResponseReceivedUtc", TestDateAttribute.Date, pivotREQ.AIP_LastResponseReceivedUtc);
			AssertEquals("PostCondition:AIP_Status", expectedPivotState, pivotREQ.AIP_Status);
			AssertEquals("PostCondition:AIP_ErrorDescription", expectedErrorDescription, pivotREQ.AIP_ErrorDescription);

			AssertEquals("Is Invoice Cancelled", isInvoiceCancelled, invoice.IsCancelled);
			AssertEquals("PostCondition: XP_ApprovalStatus", expectedApprovalStatus, invoice.TransactionRelatedApprovalRequest.XP_ApprovalStatus);

			if (assertIsRetried)
			{
				var newFactory = new BusinessObjectFactory();
				var newPivot = newFactory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, pivotActionType)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded))
					.FirstOrDefault();
				AssertNotNull(newPivot);
				AssertEquals(EInvoicingPivotState.Queued, newPivot.AIP_Status);
				AssertEquals(pivotActionType, newPivot.AIP_ActionType);
				AssertEquals("REJ:Unexpected Error - Attempting Retry #1", newPivot.AIP_ErrorDescription);
			}
		}

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIAKEventMessageProcessingForRejectRequest() =>
			AssertEventMessageProcessingForRejectRequest(
				eventMessage: SendDocumentResponse_RejectionAccepted,
				expectedLogCount: 1,
				expectedLogMessages: new string[] { "The invoice is rejected successfully. Invoice Number: INV2022000000001" },
				expectedPivotState: EInvoicingPivotState.Succeed,
				expectedErrorDescription: "",
				expectedApprovalStatus: GenApprovalRequestApprovalStatus.Rejected);

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIAKEventMessageProcessingForRejectRequestWithAutoApprovedResult() =>
			AssertEventMessageProcessingForRejectRequest(
				eventMessage: SendDocumentResponse_RejectionDeclinedAsAlreadyApproved,
				expectedLogCount: 1,
				expectedLogMessages: new string[] { "The invoice is already 'Approved' by the government. You cannot reject it. Invoice Number: INV2022000000001" },
				expectedPivotState: EInvoicingPivotState.Succeed,
				expectedErrorDescription: "Faturanın şuanki durumu yanıt vermek için uygun değil, Durum: Approved.EFT-TESTPORTAL",
				isInvoiceCancelled: false,
				expectedApprovalStatus: GenApprovalRequestApprovalStatus.Requested);

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIAKEventMessageProcessingForRejectRequestWithAutoApprovedResult_InvoiceHasErrors() =>
			AssertEventMessageProcessingForRejectRequest(
				eventMessage: SendDocumentResponse_RejectionDeclinedAsAlreadyApproved,
				expectedLogCount: 1,
				expectedLogMessages: new string[] { "The invoice is already 'Approved' by the government. You cannot reject it. Invoice Number: INV2022000000001" },
				expectedPivotState: EInvoicingPivotState.Succeed,
				expectedErrorDescription: "Faturanın şuanki durumu yanıt vermek için uygun değil, Durum: Approved.EFT-TESTPORTAL",
				isInvoiceCancelled: false,
				hasErrors: true,
				expectedApprovalStatus: GenApprovalRequestApprovalStatus.Error);

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIAKEventMessageProcessingForRejectRequestWithAlreadyRejectedResult() =>
			AssertEventMessageProcessingForRejectRequest(
				eventMessage: SendDocumentResponse_RejectionDeclinedAsAlreadyRejected,
				expectedLogCount: 1,
				expectedLogMessages: new string[] { "The invoice is already 'Rejected'. Invoice Number: INV2022000000001" },
				expectedPivotState: EInvoicingPivotState.Succeed,
				expectedErrorDescription: "Faturanın şuanki durumu yanıt vermek için uygun değil, Durum: Declined.EFT-TESTPORTAL",
				expectedApprovalStatus: GenApprovalRequestApprovalStatus.Rejected);

		[TestDate(2018, 01, 17, 20, 35, 10)]
		public void TestIRJEventMessageProcessingForRejectRequestApplicableForRetry() =>
			AssertEventMessageProcessingForRejectRequest(
				eventMessage: SendDocumentResponseFailureMessageApplicableForRetry,
				expectedLogCount: 0,
				expectedLogMessages: Array.Empty<string>(),
				expectedPivotState: EInvoicingPivotState.Discarded,
				expectedErrorDescription: "",
				isInvoiceCancelled: false,
				expectedApprovalStatus: GenApprovalRequestApprovalStatus.RejectionRequested,
				assertIsRetried: true);

		void AssertEventMessageProcessingForRejectRequest(string eventMessage, int expectedLogCount, string[] expectedLogMessages, string expectedPivotState, string expectedErrorDescription, string expectedApprovalStatus, bool isInvoiceCancelled = true, bool assertIsRetried = false, bool hasErrors = false)
		{
			HelperTR.TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
			HelperTR.Factory.SuspendValidation();
			var invoice = HelperTR.Factory.NewWithValidTestData<TransactionPendingAllocation>();
			if (!hasErrors)
			{
				invoice.AH_OH = TestObjectCreator.CreditorTR.PK;
			}
			invoice.AH_TransactionNum = "INV2022000000001";
			invoice.AH_OSExTaxAmount = 18m;
			invoice.AH_PostDate = ZDateTime.Now;
			invoice.AH_GovernmentAllocatedID = HelperTR.GovermentAllocatedNumberForTest;
			var pivotCRX = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, EInvoicingPivotActionType.ConfirmTransactionReceived, EInvoicingPivotState.Succeed);
			var batchCRX = HelperTR.TestObjectCreator.CreateEInvoicingBatchForPivot(pivotCRX, 1000, EInvoicingBatchState.Sent);
			batchCRX.AIB_GovernmentAllocatedNumber = HelperTR.GovermentAllocatedNumberForTest;
			HelperTR.Factory.Save();

			invoice.TransactionRelatedApprovalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.RejectionRequested;

			var pivotREJ = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, EInvoicingPivotActionType.Reject, EInvoicingPivotState.Sent);
			var batchREJ = HelperTR.TestObjectCreator.CreateEInvoicingBatchForPivot(pivotREJ, 1001, EInvoicingBatchState.Sent);
			batchREJ.AIB_GovernmentAllocatedNumber = HelperTR.GovermentAllocatedNumberForTest;
			HelperTR.Factory.Save();

			var inMessage = CreateEDIMessage(UOFactory.BOFactory, eventMessage);
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, batchREJ);

			AssertEquals("PreCondition:Log_Count", 0, logger.Logs.Count());
			Assert("PreCondition:AIB_GovernmentAllocatedNumber", !batchREJ.AIB_GovernmentAllocatedNumber.IsEmpty);
			Assert("PreCondition:AIP_LastResponseReceivedUtc", pivotREJ.AIP_LastResponseReceivedUtc.IsEmpty);
			AssertEquals("PreCondition:AIP_Status", EInvoicingPivotState.Sent, pivotREJ.AIP_Status);
			Assert("PreCondition:AIP_ErrorDescription", pivotREJ.AIP_ErrorDescription.IsEmpty);

			processor.Process();

			HelperTR.Factory.Save();

			universalEvent.Dispose();

			AssertEquals("PostCondition:Log_Count", expectedLogCount, logger.Logs.Count());
			foreach (var log in expectedLogMessages)
			{
				Assert("PostCondition:Log Message", logger.Logs.ToList().First(x => x.Message.Contains(log)) != null);
			}
			Assert("PostCondition:AIB_GovernmentAllocatedNumber", !batchREJ.AIB_GovernmentAllocatedNumber.IsEmpty);
			AssertEquals("PostCondition:AIP_LastResponseReceivedUtc", TestDateAttribute.Date, pivotREJ.AIP_LastResponseReceivedUtc);
			AssertEquals("PostCondition:AIP_Status", expectedPivotState, pivotREJ.AIP_Status);
			AssertEquals("PostCondition:AIP_ErrorDescription", expectedErrorDescription, pivotREJ.AIP_ErrorDescription);

			AssertEquals("Is Invoice Cancelled", isInvoiceCancelled, invoice.IsCancelled);
			AssertEquals("PostCondition: XP_ApprovalStatus", expectedApprovalStatus, invoice.TransactionRelatedApprovalRequest.XP_ApprovalStatus);

			if (assertIsRetried)
			{
				var newFactory = new BusinessObjectFactory();
				var newPivot = newFactory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.Reject)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded))
					.FirstOrDefault();
				AssertNotNull(newPivot);
				AssertEquals(EInvoicingPivotState.Queued, newPivot.AIP_Status);
				AssertEquals(EInvoicingPivotActionType.Reject, newPivot.AIP_ActionType);
				AssertEquals("Unexpected Error - Attempting Retry #1", newPivot.AIP_ErrorDescription);
			}
		}

		#region Processing Error Message Consolidation

		public void TestSubmitPivot_MessageConsolidation()
		{
			var batchNumber = 900;

			const string docErrorMessage = "IO hatası oluştu. Hata türü: FileLoadException";
			const string staErrorMessage = "Refer to the Uyumsoft Portal for more information";

			InvoicingBase invoice;
			using (var uoFactory = new UniversalObjectFactory())
			{
				const string transactionNumber = "00006000";
				var creator = new TestObjectCreator(uoFactory.BOFactory);
				invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), transactionNumber, creator.EUR, 1m, 100m, 0, 100m, 0m);
				uoFactory.SaveForTesting();
			}

			batchNumber++;
			var subFailPivotPK = ProcessSubmitFailure(invoice, batchNumber);

			batchNumber++;
			DiscardFailedPivot(subFailPivotPK, testResponseMessage);
			var submitPivotPK = ProcessSubmitSuccess(invoice, batchNumber);
			AssertFollowUpPivotsCreated(invoice, submitPivotPK);

			batchNumber++;
			var docFailPivotPK = ProcessDocumentFailure(invoice, batchNumber, submitPivotPK, docErrorMessage);

			batchNumber++;
			var staFailPivotPK = ProcessStatusCheckFailure(invoice, batchNumber, submitPivotPK, docErrorMessage, staErrorMessage);

			batchNumber++;
			DiscardFailedPivot(docFailPivotPK, docErrorMessage);
			ProcessDocumentSuccess(invoice, batchNumber, submitPivotPK, docFailPivotPK, docErrorMessage, staErrorMessage);

			batchNumber++;
			ProcessStatusCheckProgress(invoice, batchNumber, submitPivotPK, staFailPivotPK, staErrorMessage);

			batchNumber++;
			ProcessStatusCheckSuccess(invoice, batchNumber);

			// Ensure that the final status reflects a transaction without (fatal) errors.
			using (var uoFactory = new UniversalObjectFactory())
			{
				var postServiceInvoice = uoFactory.BOFactory.Load<AccTransactionHeader>(invoice.PK);
				AssertNotNull(nameof(postServiceInvoice), postServiceInvoice);
				AssertEquals("AllEDocs.Count", 1, postServiceInvoice.DocManagerInfo.AllEDocs.Count);

				var pivotSubmit = uoFactory.BOFactory.Load<AccEInvoicingTransactionPivot>(submitPivotPK);
				AssertNotNull(nameof(pivotSubmit), pivotSubmit);
				AssertEquals("SUB.AIP_Status", EInvoicingPivotState.Succeed, pivotSubmit.AIP_Status);
				AssertEquals("SUB.AIP_ErrorDescription", EInvoicingStatuses.Descriptions.Approved, pivotSubmit.AIP_ErrorDescription);
			}

			ErrorReporter.Clear();
		}

		ZGuid ProcessSubmitFailure(InvoicingBase invoice, int batchNumber)
		{
			// SUB message with error.
			using (var uoFactory = new UniversalObjectFactory())
			{
				var creator = new TestObjectCreator(uoFactory.BOFactory);

				var batch = creator.CreateEInvoicingBatch(batchNumber, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivot = creator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Sent, EInvoicingPivotActionType.Submit);

				var message = InvoiceReceiveEventFailedMessageForSubmit.Replace("<Key>1</Key>", $"<Key>{batchNumber}</Key>");
				CreateEDIMessage(uoFactory.BOFactory, message);
				uoFactory.SaveForTesting();
				new UMIServiceTask().RunTask();

				return pivot.PK;
			}
		}

		ZGuid ProcessSubmitSuccess(InvoicingBase invoice, int batchNumber)
		{
			// SUB message without error. Should update invoice with government number and result in creation of two more pivots for document action and status request.
			using (var uoFactory = new UniversalObjectFactory())
			{
				var creator = new TestObjectCreator(uoFactory.BOFactory);

				var batch = creator.CreateEInvoicingBatch(batchNumber, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivot = creator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Sent, EInvoicingPivotActionType.Submit);

				const string governmentAllocatedNumber = "987654321";

				var governmentAllocatedNumberContext = string.Format(EmptyGovernmentAllocatedNumberContext, governmentAllocatedNumber);
				var contextCollection = string.Format(EmptyContextCollection, governmentAllocatedNumberContext);
				var messageText = string.Format(EventMessage, batchNumber, contextCollection);

				CreateEDIMessage(uoFactory.BOFactory, messageText);
				uoFactory.SaveForTesting();
				new UMIServiceTask().RunTask();

				return pivot.PK;
			}
		}

		void DiscardFailedPivot(ZGuid pivotPK, string expectedMessage)
		{
			using (var uoFactory = new UniversalObjectFactory())
			{
				if (!pivotPK.IsEmpty)
				{
					var failedPivot = uoFactory.BOFactory.Load<AccEInvoicingTransactionPivot>(pivotPK);

					AssertNotNull(nameof(failedPivot), failedPivot);
					AssertEquals("SUB.AIP_Status", EInvoicingPivotState.Failed, failedPivot.AIP_Status);
					AssertEquals("SUB.AIP_ErrorDescription", expectedMessage, failedPivot.AIP_ErrorDescription);

					failedPivot.AIP_Status = EInvoicingPivotState.Discarded;

					uoFactory.SaveForTesting();
				}
			}
		}

		void AssertFollowUpPivotsCreated(InvoicingBase invoice, ZGuid submitPivotPK)
		{
			// Ensure that the additional pivots are created.
			using (var uoFactory = new UniversalObjectFactory())
			{
				var pivotSubmit = uoFactory.BOFactory.Load<AccEInvoicingTransactionPivot>(submitPivotPK);

				AssertNotNull(nameof(pivotSubmit), pivotSubmit);
				AssertEquals("SUB.AIP_Status", EInvoicingPivotState.Delivered, pivotSubmit.AIP_Status);
				AssertEquals("SUB.AIP_ErrorDescription", string.Empty, pivotSubmit.AIP_ErrorDescription);

				var pivotStatusCheck = uoFactory.BOFactory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck));

				AssertNotNull(nameof(pivotStatusCheck), pivotStatusCheck);
				AssertEquals("STA.AIP_Status", EInvoicingPivotState.Queued, pivotStatusCheck.AIP_Status);
				AssertEquals("STA.AIP_ErrorDescription", string.Empty, pivotStatusCheck.AIP_ErrorDescription);

				var pivotDocumentAction = uoFactory.BOFactory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction));

				AssertNotNull(nameof(pivotDocumentAction), pivotDocumentAction);
				AssertEquals("DOC.AIP_Status", EInvoicingPivotState.Queued, pivotDocumentAction.AIP_Status);
				AssertEquals("DOC.AIP_ErrorDescription", string.Empty, pivotDocumentAction.AIP_ErrorDescription);
			}
		}

		ZGuid ProcessDocumentFailure(InvoicingBase invoice, int batchNumber, ZGuid submitPivotPK, string docErrorMessage)
		{
			// RCP message with transient error. Should end up as FAL and copy the error message to the SUB pivot.
			using (var uoFactory = new UniversalObjectFactory())
			{
				var pivotDocumentAction = uoFactory.BOFactory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction));

				AssertNotNull(nameof(pivotDocumentAction), pivotDocumentAction);

				var creator = new TestObjectCreator(uoFactory.BOFactory);
				creator.CreateEInvoicingBatchForPivot(pivotDocumentAction, batchNumber, EInvoicingBatchState.Ready);
				var messageText = string.Format(DocumentActionErrorMessage, batchNumber, docErrorMessage);

				CreateEDIMessage(uoFactory.BOFactory, messageText);
				uoFactory.SaveForTesting();
				new UMIServiceTask().RunTask();
			}

			// Ensure that DOC pivot failed with the expected error message. The SUB pivot should also reflect the same error message (but not the status).
			using (var uoFactory = new UniversalObjectFactory())
			{
				var pivotDocumentAction = uoFactory.BOFactory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction));

				AssertNotNull(nameof(pivotDocumentAction), pivotDocumentAction);
				AssertEquals("DOC.AIP_Status", EInvoicingPivotState.Failed, pivotDocumentAction.AIP_Status);
				AssertEquals("DOC.AIP_ErrorDescription", docErrorMessage, pivotDocumentAction.AIP_ErrorDescription);

				var postFailureInvoice = uoFactory.BOFactory.Load<AccTransactionHeader>(invoice.PK);
				AssertEquals("AllEDocs.Count", 0, postFailureInvoice.DocManagerInfo.AllEDocs.Count);

				var pivotSubmit = uoFactory.BOFactory.Load<AccEInvoicingTransactionPivot>(submitPivotPK);
				AssertNotNull(nameof(pivotSubmit), pivotSubmit);
				AssertEquals(EInvoicingPivotState.Delivered, pivotSubmit.AIP_Status);
				AssertEquals($"{EInvoicingPivotActionType.DocumentAction}:{docErrorMessage}", pivotSubmit.AIP_ErrorDescription);

				return pivotDocumentAction.PK;
			}
		}

		ZGuid ProcessDocumentSuccess(InvoicingBase invoice, int batchNumber, ZGuid submitPivotPK, ZGuid docFailPivotPK, string docErrorMessage, string staErrorMessage)
		{
			// RCP message without error. Should end up SUC. Error message needs to be removed from SUB pivot after this.
			using (var uoFactory = new UniversalObjectFactory())
			{
				var creator = new TestObjectCreator(uoFactory.BOFactory);
				var docSucceedBatch = creator.CreateEInvoicingBatch(batchNumber, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var docSucceedpivot = creator.CreateEInvoicingTransactionPivot(docSucceedBatch, invoice, EInvoicingPivotState.Sent, EInvoicingPivotActionType.DocumentAction, overridePivot: false);

				var messageText = string.Format(EventMessage, batchNumber, AttachedDocumentCollectionWithDocument);

				CreateEDIMessage(uoFactory.BOFactory, messageText);
				uoFactory.SaveForTesting();
				new UMIServiceTask().RunTask();

				var pivotSubmit = uoFactory.BOFactory.Load<AccEInvoicingTransactionPivot>(submitPivotPK);
				AssertEquals(EInvoicingPivotState.Delivered, pivotSubmit.AIP_Status);
				AssertEquals($"{EInvoicingPivotActionType.StatusCheck}:{staErrorMessage}", pivotSubmit.AIP_ErrorDescription);

				return docSucceedpivot.PK;
			}
		}

		ZGuid ProcessStatusCheckFailure(InvoicingBase invoice, int batchNumber, ZGuid submitPivotPK, string ioErrorMessage, string statusErrorMessage)
		{
			// STA with error. Should update the SUB with all error messages.
			using (var uoFactory = new UniversalObjectFactory())
			{
				var pivotStatusCheck = uoFactory.BOFactory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck));

				AssertNotNull(nameof(pivotStatusCheck), pivotStatusCheck);

				var creator = new TestObjectCreator(uoFactory.BOFactory);
				creator.CreateEInvoicingBatchForPivot(pivotStatusCheck, batchNumber, EInvoicingBatchState.Ready);
				var messageText = InvoiceReceiveEventFailedMessageForStatusRequest.Replace("<Value>StatusCodeValue</Value>", "<Value></Value>");
				messageText = messageText.Replace("<Key>1</Key>", $"<Key>{batchNumber}</Key>");

				CreateEDIMessage(uoFactory.BOFactory, messageText);
				uoFactory.SaveForTesting();
				new UMIServiceTask().RunTask();

				var consolidatedMessage = $"{EInvoicingPivotActionType.StatusCheck}:{statusErrorMessage}; {EInvoicingPivotActionType.DocumentAction}:{ioErrorMessage}";

				var pivotSubmit = uoFactory.BOFactory.Load<AccEInvoicingTransactionPivot>(submitPivotPK);
				AssertEquals(EInvoicingPivotState.Delivered, pivotSubmit.AIP_Status);
				AssertEquals(consolidatedMessage, pivotSubmit.AIP_ErrorDescription);

				return pivotStatusCheck.PK;
			}
		}

		ZGuid ProcessStatusCheckProgress(InvoicingBase invoice, int batchNumber, ZGuid submitPivotPK, ZGuid staFailPivotPK, string statusErrorMessage)
		{
			// STA returns non-final status. Should also update SUB message as "Processing"
			using (var uoFactory = new UniversalObjectFactory())
			{
				var staFailPivot = uoFactory.BOFactory.Load<AccEInvoicingTransactionPivot>(staFailPivotPK);
				AssertNotNull(nameof(staFailPivot), staFailPivot);
				AssertEquals("STA.AIP_Status", EInvoicingPivotState.Failed, staFailPivot.AIP_Status);
				AssertEquals("STA.AIP_ErrorDescription", statusErrorMessage, staFailPivot.AIP_ErrorDescription);

				staFailPivot.AIP_Status = EInvoicingPivotState.Discarded;

				var creator = new TestObjectCreator(uoFactory.BOFactory);
				var pivotStatusBatch = creator.CreateEInvoicingBatch(batchNumber, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivotStatusCheck = creator.CreateEInvoicingTransactionPivot(pivotStatusBatch, invoice, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck, overridePivot: false);

				var messageText = string.Format(StatusRequestSuccessMessage, batchNumber, EInvoicingStatuses.Codes.Processing);

				CreateEDIMessage(uoFactory.BOFactory, messageText);
				uoFactory.SaveForTesting();
				new UMIServiceTask().RunTask();

				var pivotSubmit = uoFactory.BOFactory.Load<AccEInvoicingTransactionPivot>(submitPivotPK);
				AssertEquals(EInvoicingPivotState.Delivered, pivotSubmit.AIP_Status);
				AssertEquals(EInvoicingStatuses.Descriptions.Processing, pivotSubmit.AIP_ErrorDescription);

				return pivotStatusCheck.PK;
			}
		}

		void ProcessStatusCheckSuccess(InvoicingBase invoice, int batchNumber)
		{
			// STA message. Should finalize the invoice and update the SUB as SUC.
			using (var uoFactory = new UniversalObjectFactory())
			{
				var pivotStatusCheck = uoFactory.BOFactory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck));

				AssertNotNull(nameof(pivotStatusCheck), pivotStatusCheck);

				var creator = new TestObjectCreator(uoFactory.BOFactory);
				creator.CreateEInvoicingBatchForPivot(pivotStatusCheck, batchNumber, EInvoicingBatchState.Ready);
				var messageText = string.Format(StatusRequestSuccessMessage, batchNumber, EInvoicingStatuses.Codes.Approved);

				CreateEDIMessage(uoFactory.BOFactory, messageText);
				uoFactory.SaveForTesting();
				new UMIServiceTask().RunTask();
			}
		}

		#endregion Processing Error Message Consolidation

		#region HelperMethods

		EDIMessage CreateEDIMessage(BusinessObjectFactory factory, string messageContent)
		{
			var message = factory.New<EDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageContent;
			return message;
		}

		(InvoicingBase invoice, AccEInvoicingBatch batch, AccEInvoicingTransactionPivot pivot) CreateInvoiceBatchAndPivot(string transactionNumber, ZInt batchNumber, string actionType = EInvoicingPivotActionType.DocumentAction, string status = EInvoicingPivotState.Delivered)
		{
			var creator = new TestObjectCreator(UOFactory.BOFactory);
			var invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), transactionNumber, creator.EUR, 1m, 100m, 0, 100m, 0m);
			var batch = creator.CreateEInvoicingBatch(batchNumber, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = creator.CreateEInvoicingTransactionPivot(batch, invoice, status, actionType);
			return (invoice, batch, pivot);
		}

		AccEInvoicingTransactionPivot GetPivotCreatedAfterSubmitActionProcess(AccEInvoicingTransactionPivot submitPivot, ZString actionType)
			=> submitPivot.Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, submitPivot.AIP_ParentID)
			.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType)
			.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued));

		AccEInvoicingTransactionPivot GetPivotByParentIdTypeAndStatus(ZGuid parentId, ZString actionType, string pivotStatus)
		{
			var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentId)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, pivotStatus));

			return pivot;
		}

		(EDIMessage inMessage, IXmlSessionTracker logger, EInvoicingEventMessageTRProcessor processor) SetupDataForRetryTest()
		{
			var inMessage = CreateEDIMessage(UOFactory.BOFactory, IRJReasonMessageBuilder(retryApplicableValue: "true"));
			Pivot.SetCompanyAndCountryCode(inMessage.Company);
			Batch.AIB_GC = inMessage.Company.PK;
			Pivot.Factory.Save();

			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEventDataObject>();
			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new EInvoicingEventMessageTRProcessor(logger, inMessage, universalEvent, Batch);

			return (inMessage, logger, processor);
		}

		string IRJReasonMessageBuilder(string retryApplicableValue = null)
		{
			var builder = new StringBuilder(InvoiceReceiveEventFailedMessageForSubmit);
			builder.Replace("testResponseMessage", testResponseMessage);
			var retryApplicableContext = string.Empty;

			if (retryApplicableValue != null)
			{
				retryApplicableContext = IsApplicableForRetryContextValue.Replace("{Value}", retryApplicableValue);
			}

			builder.Replace("isApplicableForRetryContext", retryApplicableContext);
			return builder.ToString();
		}

		#endregion

		#region MessageStrings

		readonly string InvoiceReceiveEventMessage = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>EInvoiceResponse</MessageSubType>
		</EventParameters>
		{0}
	</Event>
</UniversalEvent>";
		readonly string PILResponseEventMessage = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2020-06-02T07:01:07</EventTime>
		<EventType>{eventType}</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>PIL</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTR</Value>
			</Context>
			<Context>
				<Type>Transactions</Type>
				<Value>W3siR292ZXJubWVudEFsbG9jYXRlZE51bWJlciI6IjBhZDUyOWQ5LTEwZDctNDBjZC04NmZmLTgwMTFlNDFmNWQyZiIsIlRyYW5zYWN0aW9uTnVtYmVyIjoiVVhGMjAyMDAwMDAwMDAwMSIsIlRyYW5zYWN0aW9uRGF0ZSI6IjIwMjAtMDQtMjhUMTE6NDI6MTUiLCJDb21wbGlhbmNlU3ViVHlwZSI6IkVJQyIsIk9yZ2FuaXNhdGlvbk5hbWUiOiJVeXVtc29mdCBCaWxnaSBUZWtub2xvamlsZXJpIEEuxZ4uIiwiUkVHOlZBVCI6IjkwMDAwNjg0MTgiLCJDdXJyZW5jeUNvZGUiOiJUUlkiLCJFeGNoYW5nZVJhdGUiOiIxLjAwMDAwMDAwIiwiT1NFeFRheEFtb3VudCI6IjUwLjAwIiwiT1NUYXhBbW91bnQiOiI5LjAwMDAifSx7IkdvdmVybm1lbnRBbGxvY2F0ZWROdW1iZXIiOiJiYWFhYTVkMC02NjkwLTQ4YzAtYWUxMy04ZTIyNWVmMDA3MTIiLCJUcmFuc2FjdGlvbk51bWJlciI6IlVYRjIwMjAwMDAwMDAwMTMiLCJUcmFuc2FjdGlvbkRhdGUiOiIyMDIwLTA0LTI4VDE5OjIzOjE4IiwiQ29tcGxpYW5jZVN1YlR5cGUiOiJFSUMiLCJPcmdhbmlzYXRpb25OYW1lIjoiVEVTVCIsIlJFRzpWQVQiOiI5MDAwMDY4NDE4IiwiQ3VycmVuY3lDb2RlIjoiVFJZIiwiRXhjaGFuZ2VSYXRlIjoiMS4wMDAwMDAwMCIsIk9TRXhUYXhBbW91bnQiOiI5Mi4wMCIsIk9TVGF4QW1vdW50IjoiOC4wMDAwIn0seyJHb3Zlcm5tZW50QWxsb2NhdGVkTnVtYmVyIjoiZmM5M2U5YjUtOGM5OS00NWJiLTlhMGYtMDEyN2RkM2Q1Njk4IiwiVHJhbnNhY3Rpb25OdW1iZXIiOiJaRVkyMDIwMDAwMDAwMDE4IiwiVHJhbnNhY3Rpb25EYXRlIjoiMjAyMC0wNS0yMVQxMzowMjo1NCIsIkNvbXBsaWFuY2VTdWJUeXBlIjoiRUlOIiwiT3JnYW5pc2F0aW9uTmFtZSI6ImFsZXluYSIsIlJFRzpWQVQiOiI5MDAwMDY4NDE4IiwiQ3VycmVuY3lDb2RlIjoiVVNEIiwiRXhjaGFuZ2VSYXRlIjoiNi44NzIwMDAwMCIsIk9TRXhUYXhBbW91bnQiOiIxMDAwLjAwIiwiT1NUYXhBbW91bnQiOiIwLjAwMDAifSx7IkdvdmVybm1lbnRBbGxvY2F0ZWROdW1iZXIiOiIwYWQ2NTQxNS01OTcxLTQ2YzktODlkNS02YjJkZjc1ZGI1YTMiLCJUcmFuc2FjdGlvbk51bWJlciI6IlVYRjIwMjAwMDAwMDAwMzIiLCJUcmFuc2FjdGlvbkRhdGUiOiIyMDIwLTA0LTMwVDA1OjU1OjUxIiwiQ29tcGxpYW5jZVN1YlR5cGUiOiJFSUMiLCJPcmdhbmlzYXRpb25OYW1lIjoiVEVTVCIsIlJFRzpWQVQiOiI5MDAwMDY4NDE4IiwiQ3VycmVuY3lDb2RlIjoiVFJZIiwiRXhjaGFuZ2VSYXRlIjoiMS4wMDAwMDAwMCIsIk9TRXhUYXhBbW91bnQiOiIxMTYuNTkiLCJPU1RheEFtb3VudCI6IjQuNDAwMCJ9LHsiR292ZXJubWVudEFsbG9jYXRlZE51bWJlciI6ImFjMjA3OGIxLTU2MzktNDRmNC1hY2FkLTliY2VkZjdhNGI1ZCIsIlRyYW5zYWN0aW9uTnVtYmVyIjoiVVhGMjAyMDAwMDAwMDAzNyIsIlRyYW5zYWN0aW9uRGF0ZSI6IjIwMjAtMDQtMzBUMDY6NDY6MDgiLCJDb21wbGlhbmNlU3ViVHlwZSI6IkVJTiIsIk9yZ2FuaXNhdGlvbk5hbWUiOiJVeXVtc29mdCBCaWxnaSBTaXN0ZW1sZXJpIHZlIFRla25vbG9qaWxlcmkgQS7Fni4iLCJSRUc6VkFUIjoiOTAwMDA2ODQxOCIsIkN1cnJlbmN5Q29kZSI6IlRSWSIsIkV4Y2hhbmdlUmF0ZSI6IjEuMDAwMDAwMDAiLCJPU0V4VGF4QW1vdW50IjoiMTM5MC4wMCIsIk9TVGF4QW1vdW50IjoiMC4wMDAwIn0seyJHb3Zlcm5tZW50QWxsb2NhdGVkTnVtYmVyIjoiZTI0MTEwNzMtYjFhMS00ZWNlLWE4NGItYzJlNGMzOWIwYzMxIiwiVHJhbnNhY3Rpb25OdW1iZXIiOiJVWEYyMDIwMDAwMDAwMDQxIiwiVHJhbnNhY3Rpb25EYXRlIjoiMjAyMC0wNC0zMFQwNjo1NTozMiIsIkNvbXBsaWFuY2VTdWJUeXBlIjoiRUlOIiwiT3JnYW5pc2F0aW9uTmFtZSI6IlV5dW1zb2Z0IEJpbGdpIFNpc3RlbWxlcmkgdmUgVGVrbm9sb2ppbGVyaSBBLsWeLiIsIlJFRzpWQVQiOiI5MDAwMDY4NDE4IiwiQ3VycmVuY3lDb2RlIjoiVFJZIiwiRXhjaGFuZ2VSYXRlIjoiMS4wMDAwMDAwMCIsIk9TRXhUYXhBbW91bnQiOiIxMzkwLjAwIiwiT1NUYXhBbW91bnQiOiIwLjAwMDAifSx7IkdvdmVybm1lbnRBbGxvY2F0ZWROdW1iZXIiOiI0YmVmM2M5OC1kNWExLTQ0OTQtODdmNi03NGRkM2VkZjE0NjYiLCJUcmFuc2FjdGlvbk51bWJlciI6IlVYRjIwMjAwMDAwMDAwNDIiLCJUcmFuc2FjdGlvbkRhdGUiOiIyMDIwLTA0LTMwVDA2OjU3OjM0IiwiQ29tcGxpYW5jZVN1YlR5cGUiOiJFSU4iLCJPcmdhbmlzYXRpb25OYW1lIjoiVXl1bXNvZnQgQmlsZ2kgU2lzdGVtbGVyaSB2ZSBUZWtub2xvamlsZXJpIEEuxZ4uIiwiUkVHOlZBVCI6IjkwMDAwNjg0MTgiLCJDdXJyZW5jeUNvZGUiOiJUUlkiLCJFeGNoYW5nZVJhdGUiOiIxLjAwMDAwMDAwIiwiT1NFeFRheEFtb3VudCI6IjEzOTAuMDAiLCJPU1RheEFtb3VudCI6IjAuMDAwMCJ9LHsiR292ZXJubWVudEFsbG9jYXRlZE51bWJlciI6IjM2NDY4MjEwLTVmMzQtNDZiYi04M2NjLTA4YmU0NmRhZDQxZCIsIlRyYW5zYWN0aW9uTnVtYmVyIjoiVVhGMjAyMDAwMDAwMDA0MyIsIlRyYW5zYWN0aW9uRGF0ZSI6IjIwMjAtMDQtMzBUMDc6MjE6MDUiLCJDb21wbGlhbmNlU3ViVHlwZSI6IkVJTiIsIk9yZ2FuaXNhdGlvbk5hbWUiOiJVeXVtc29mdCBCaWxnaSBTaXN0ZW1sZXJpIHZlIFRla25vbG9qaWxlcmkgQS7Fni4iLCJSRUc6VkFUIjoiOTAwMDA2ODQxOCIsIkN1cnJlbmN5Q29kZSI6IlRSWSIsIkV4Y2hhbmdlUmF0ZSI6IjEuMDAwMDAwMDAiLCJPU0V4VGF4QW1vdW50IjoiMTM5MC4wMCIsIk9TVGF4QW1vdW50IjoiMC4wMDAwIn0seyJHb3Zlcm5tZW50QWxsb2NhdGVkTnVtYmVyIjoiZGUzZmE3ZmUtNWVkNC00M2Y2LWIyMzctZjI5MWM0NTczNzNjIiwiVHJhbnNhY3Rpb25OdW1iZXIiOiJVWEYyMDIwMDAwMDAwMDQ0IiwiVHJhbnNhY3Rpb25EYXRlIjoiMjAyMC0wNC0zMFQwNzoyNjoyOCIsIkNvbXBsaWFuY2VTdWJUeXBlIjoiRUlOIiwiT3JnYW5pc2F0aW9uTmFtZSI6IlV5dW1zb2Z0IEJpbGdpIFNpc3RlbWxlcmkgdmUgVGVrbm9sb2ppbGVyaSBBLsWeLiIsIlJFRzpWQVQiOiI5MDAwMDY4NDE4IiwiQ3VycmVuY3lDb2RlIjoiVFJZIiwiRXhjaGFuZ2VSYXRlIjoiMS4wMDAwMDAwMCIsIk9TRXhUYXhBbW91bnQiOiIxMzkwLjAwIiwiT1NUYXhBbW91bnQiOiIwLjAwMDAifSx7IkdvdmVybm1lbnRBbGxvY2F0ZWROdW1iZXIiOiJjMTZjODU1Ny0zNWUyLTQ1ZWYtOGE3Yi1jZDE0YWI4MDc2YzUiLCJUcmFuc2FjdGlvbk51bWJlciI6IlVYRjIwMjAwMDAwMDAwNDUiLCJUcmFuc2FjdGlvbkRhdGUiOiIyMDIwLTA0LTMwVDA3OjMzOjUxIiwiQ29tcGxpYW5jZVN1YlR5cGUiOiJFSU4iLCJPcmdhbmlzYXRpb25OYW1lIjoiVXl1bXNvZnQgQmlsZ2kgU2lzdGVtbGVyaSB2ZSBUZWtub2xvamlsZXJpIEEuxZ4uIiwiUkVHOlZBVCI6IjkwMDAwNjg0MTgiLCJDdXJyZW5jeUNvZGUiOiJUUlkiLCJFeGNoYW5nZVJhdGUiOiIxLjAwMDAwMDAwIiwiT1NFeFRheEFtb3VudCI6IjEzOTAuMDAiLCJPU1RheEFtb3VudCI6IjAuMDAwMCJ9LHsiR292ZXJubWVudEFsbG9jYXRlZE51bWJlciI6IjRlNzQyYzU2LTdmNjQtNGIwMC1iODdjLWI1MzZhMTIxZDllMyIsIlRyYW5zYWN0aW9uTnVtYmVyIjoiVVhGMjAyMDAwMDAwMDA0NiIsIlRyYW5zYWN0aW9uRGF0ZSI6IjIwMjAtMDQtMzBUMDc6Mzc6MTQiLCJDb21wbGlhbmNlU3ViVHlwZSI6IkVJTiIsIk9yZ2FuaXNhdGlvbk5hbWUiOiJVeXVtc29mdCBCaWxnaSBTaXN0ZW1sZXJpIHZlIFRla25vbG9qaWxlcmkgQS7Fni4iLCJSRUc6VkFUIjoiOTAwMDA2ODQxOCIsIkN1cnJlbmN5Q29kZSI6IlRSWSIsIkV4Y2hhbmdlUmF0ZSI6IjEuMDAwMDAwMDAiLCJPU0V4VGF4QW1vdW50IjoiMTM5MC4wMCIsIk9TVGF4QW1vdW50IjoiMC4wMDAwIn0seyJHb3Zlcm5tZW50QWxsb2NhdGVkTnVtYmVyIjoiNDQwNmZkYjItYWQ1Zi00ZWU5LWI4ZmYtMWRlM2RmNWEwOGU4IiwiVHJhbnNhY3Rpb25OdW1iZXIiOiJVWEYyMDIwMDAwMDAwMDQ4IiwiVHJhbnNhY3Rpb25EYXRlIjoiMjAyMC0wNC0zMFQwOToxOTozOSIsIkNvbXBsaWFuY2VTdWJUeXBlIjoiRUlOIiwiT3JnYW5pc2F0aW9uTmFtZSI6IlV5dW1zb2Z0IEJpbGdpIFNpc3RlbWxlcmkgdmUgVGVrbm9sb2ppbGVyaSBBLsWeLiIsIlJFRzpWQVQiOiI5MDAwMDY4NDE4IiwiQ3VycmVuY3lDb2RlIjoiVFJZIiwiRXhjaGFuZ2VSYXRlIjoiMS4wMDAwMDAwMCIsIk9TRXhUYXhBbW91bnQiOiIxMzkwLjAwIiwiT1NUYXhBbW91bnQiOiIwLjAwMDAifSx7IkdvdmVybm1lbnRBbGxvY2F0ZWROdW1iZXIiOiI2YjNmN2MzZi01YTk0LTRiMzktODBjMy0xYTc3NmY5YTU3NzciLCJUcmFuc2FjdGlvbk51bWJlciI6IlVYRjIwMjAwMDAwMDAwNjEiLCJUcmFuc2FjdGlvbkRhdGUiOiIyMDIwLTA1LTAzVDEyOjM3OjQ5IiwiQ29tcGxpYW5jZVN1YlR5cGUiOiJFSU4iLCJPcmdhbmlzYXRpb25OYW1lIjoiVXl1bXNvZnQgQmlsZ2kgU2lzdGVtbGVyaSB2ZSBUZWtub2xvamlsZXJpIEEuxZ4uIiwiUkVHOlZBVCI6IjkwMDAwNjg0MTgiLCJDdXJyZW5jeUNvZGUiOiJUUlkiLCJFeGNoYW5nZVJhdGUiOiIxLjAwMDAwMDAwIiwiT1NFeFRheEFtb3VudCI6IjEzOTAuMDAiLCJPU1RheEFtb3VudCI6IjAuMDAwMCJ9LHsiR292ZXJubWVudEFsbG9jYXRlZE51bWJlciI6IjcxYmQ5OThiLWEwNzMtNDVjYS04ZjNkLTc4ODE1OWYxNzRlMyIsIlRyYW5zYWN0aW9uTnVtYmVyIjoiWkVZMjAyMDAwMDAwMDAwNCIsIlRyYW5zYWN0aW9uRGF0ZSI6IjIwMjAtMDUtMDNUMjM6Mzc6NTAiLCJDb21wbGlhbmNlU3ViVHlwZSI6IkVJQyIsIk9yZ2FuaXNhdGlvbk5hbWUiOiJhbGV5bmEiLCJSRUc6VkFUIjoiOTAwMDA2ODQxOCIsIkN1cnJlbmN5Q29kZSI6IlRSWSIsIkV4Y2hhbmdlUmF0ZSI6IjEuMDAwMDAwMDAiLCJPU0V4VGF4QW1vdW50IjoiNDkyLjUwIiwiT1NUYXhBbW91bnQiOiI1Ny40MDAwIn0seyJHb3Zlcm5tZW50QWxsb2NhdGVkTnVtYmVyIjoiNDMxMmQ3YzQtN2YxZi00ZTAyLTk3OTYtNWQ5ZTkwNDU5NTU2IiwiVHJhbnNhY3Rpb25OdW1iZXIiOiJVWEYyMDIwMDAwMDAwMDY1IiwiVHJhbnNhY3Rpb25EYXRlIjoiMjAyMC0wNS0wM1QyMzo1ODo0MSIsIkNvbXBsaWFuY2VTdWJUeXBlIjoiRUlOIiwiT3JnYW5pc2F0aW9uTmFtZSI6IkNSTSBLVVJVTVNBTCBJUyBDT1pVTUxFUkkiLCJSRUc6VkFUIjoiOTAwMDA2ODQxOCIsIkN1cnJlbmN5Q29kZSI6IlRSWSIsIkV4Y2hhbmdlUmF0ZSI6IjEuMDAwMDAwMDAiLCJPU0V4VGF4QW1vdW50IjoiMS4wMCIsIk9TVGF4QW1vdW50IjoiNy4wMDAwIn0seyJHb3Zlcm5tZW50QWxsb2NhdGVkTnVtYmVyIjoiZjIwMGM0NjYtMzFlZS0wOGEwLWVhMTEtZGU4ZGRkZGJmZTU1IiwiVHJhbnNhY3Rpb25OdW1iZXIiOiJNU0UyMDIwMDAwMDAwMDA1IiwiVHJhbnNhY3Rpb25EYXRlIjoiMjAyMC0wNS0wNFQwODoxMDoxNiIsIkNvbXBsaWFuY2VTdWJUeXBlIjoiRUlDIiwiT3JnYW5pc2F0aW9uTmFtZSI6IlNPTEVOTkUiLCJSRUc6VkFUIjoiOTAwMDA2ODQxOCIsIkN1cnJlbmN5Q29kZSI6IlRSWSIsIkV4Y2hhbmdlUmF0ZSI6IjEuMDAwMDAwMDAiLCJPU0V4VGF4QW1vdW50IjoiMTAwMjAuMDAiLCJPU1RheEFtb3VudCI6IjE5MjQuMjAwMCJ9LHsiR292ZXJubWVudEFsbG9jYXRlZE51bWJlciI6IjgwYjVmMjU5LTMwNjQtNGNkYS04NWM2LWNhOGEzMTY5NzlhNSIsIlRyYW5zYWN0aW9uTnVtYmVyIjoiVVhGMjAyMDAwMDAwMDA2OCIsIlRyYW5zYWN0aW9uRGF0ZSI6IjIwMjAtMDUtMDRUMTE6NTM6MTEiLCJDb21wbGlhbmNlU3ViVHlwZSI6IkVJQyIsIk9yZ2FuaXNhdGlvbk5hbWUiOiJDUk0gS3VydW1zYWwiLCJSRUc6VkFUIjoiOTAwMDA2ODQxOCIsIkN1cnJlbmN5Q29kZSI6IlRSWSIsIkV4Y2hhbmdlUmF0ZSI6IjEuMDAwMDAwMDAiLCJPU0V4VGF4QW1vdW50IjoiMS4wMCIsIk9TVGF4QW1vdW50IjoiNy4wMDAwIn0seyJHb3Zlcm5tZW50QWxsb2NhdGVkTnVtYmVyIjoiMjFjMGM2MTgtZGJiYS00ZmNlLTk0MmYtZTlkZGYwMTIzMTlhIiwiVHJhbnNhY3Rpb25OdW1iZXIiOiJaRVkyMDIwMDAwMDAwMDA1IiwiVHJhbnNhY3Rpb25EYXRlIjoiMjAyMC0wNS0wNVQwNjoxODoyMyIsIkNvbXBsaWFuY2VTdWJUeXBlIjoiRUlDIiwiT3JnYW5pc2F0aW9uTmFtZSI6ImFsZXluYSIsIlJFRzpWQVQiOiI5MDAwMDY4NDE4IiwiQ3VycmVuY3lDb2RlIjoiVFJZIiwiRXhjaGFuZ2VSYXRlIjoiMS4wMDAwMDAwMCIsIk9TRXhUYXhBbW91bnQiOiIxMzAuMDAiLCJPU1RheEFtb3VudCI6IjEzLjQwMDAifSx7IkdvdmVybm1lbnRBbGxvY2F0ZWROdW1iZXIiOiI2NjE5ODc0Ni0xMjBjLTRmYzEtYTg3Zi1lZTc3NzI4ZWMxYzUiLCJUcmFuc2FjdGlvbk51bWJlciI6IkxZQjIwMjAwMDAwMDAwMTUiLCJUcmFuc2FjdGlvbkRhdGUiOiIyMDIwLTA1LTA1VDEzOjM3OjU5IiwiQ29tcGxpYW5jZVN1YlR5cGUiOiJFSUMiLCJPcmdhbmlzYXRpb25OYW1lIjoiTEVBTiBZQVpJTElNIFZFIELEsEzEsMWexLBNIFRFS05PTE9KxLBMRVLEsCBBLsWeIiwiUkVHOlZBVCI6IjkwMDAwNjg0MTgiLCJDdXJyZW5jeUNvZGUiOiJUUlkiLCJFeGNoYW5nZVJhdGUiOiIxLjAwMDAwMDAwIiwiT1NFeFRheEFtb3VudCI6IjMzOC45OCIsIk9TVGF4QW1vdW50IjoiNjEuMDEwMCJ9LHsiR292ZXJubWVudEFsbG9jYXRlZE51bWJlciI6ImE2NWI3NWI4LTIyNWMtNDc1Ny1hMjliLTFmZDdjZTUzYjhmMCIsIlRyYW5zYWN0aW9uTnVtYmVyIjoiVVhGMjAyMDAwMDAwMDA5NSIsIlRyYW5zYWN0aW9uRGF0ZSI6IjIwMjAtMDUtMDZUMTA6Mjg6MjkiLCJDb21wbGlhbmNlU3ViVHlwZSI6IkVJQyIsIk9yZ2FuaXNhdGlvbk5hbWUiOiJVWVVNU09GVCBCxLBMR8SwIFPEsFNURU1MRVLEsCBWRSBURUtOT0xPSsSwTEVSxLAgVMSwQ0FSRVQgQU5PTsSwTSDFnsSwUktFVMSwIiwiUkVHOlZBVCI6IjkwMDAwNjg0MTgiLCJDdXJyZW5jeUNvZGUiOiJUUlkiLCJFeGNoYW5nZVJhdGUiOiIwLjAwMDAwMDAwIiwiT1NFeFRheEFtb3VudCI6IjUuMDAiLCJPU1RheEFtb3VudCI6IjAuMDAwMCJ9XQ==</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
		readonly string EmptyContextCollection = @"<ContextCollection>{0}</ContextCollection>";
		readonly string EmptyGovernmentAllocatedNumberContext = @"
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>{0}</Value>
			</Context>";
		readonly string InvoiceReceiveEventFailedMessageForSubmit = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<Reason>{testResponseMessage}</Reason>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>StatusCode</Type>
				<Value></Value>
			</Context>
			{isApplicableForRetryContext}
		</ContextCollection>
	</Event>
</UniversalEvent>";
		readonly string IsApplicableForRetryContextValue = @"
			<Context>
				<Type>IsApplicableForRetry</Type>
				<Value>{Value}</Value>
			</Context>";

		static readonly string testResponseMessage = "This is my test response message";

		static readonly string isApplicableForRetryContext = nameof(isApplicableForRetryContext);
		readonly string InvoiceReceiveEventFailedMessageForStatusRequest = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<Reason>Refer to the Uyumsoft Portal for more information</Reason>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>StatusCode</Type>
				<Value>StatusCodeValue</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
		readonly string EventMessage = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Event>
		<DataContext>
			<Company>
				<Code>EDI</Code>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Name>Eagle Datamation International</Name>
			</Company>
			<DataProvider>EDIDATEDI</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>{0}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>ResponseMessage</MessageSubType>
		</EventParameters>
		{1}
	</Event>
</UniversalEvent>";
		readonly string EmptyAttachedDocumentCollection = "<AttachedDocumentCollection></AttachedDocumentCollection>";
		readonly string AttachedDocumentCollectionWithDocument = @"
		<AttachedDocumentCollection>
			<AttachedDocument>
				<FileName>TestMiscellaneousFile.pdf</FileName>
				<ImageData>dGVzdA==</ImageData>
				<Type>
					<Code>MSC</Code>
					<Description>Miscellaneous Document</Description>
				</Type>
				<IsPublished>true</IsPublished>
			</AttachedDocument>
		</AttachedDocumentCollection>";
		readonly string EmptyIsCancelledContext = @"
					<Context>
						<Type>IsCancelled</Type>
						<Value>{0}</Value>
					</Context>
					<Context>
						<Type>Message</Type>
						<Value>{1}</Value>
					</Context>";
		readonly string StatusRequestResponseMessage = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
			<Event>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>AccEInvoicingBatch</Type>
							<Key>{0}</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2021-01-22T08:58:00</EventTime>
				<EventType>IAK</EventType>
				<EventParameters>
					<MessageType>TR</MessageType>
				</EventParameters>
				<ContextCollection>
					<Context>
						<Type>CompanyCode</Type>
						<Value>DTR</Value>
					</Context>
					<Context>
						<Type>StatusCode</Type>
						<Value>{1}</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>";

		const string DocumentActionErrorMessage = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>{0}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:05</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>RCP</MessageSubType>
			<Reason>{1}</Reason>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>StatusCode</Type>
				<Value></Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string StatusRequestSuccessMessage = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
    <Event>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>AccEInvoicingBatch</Type>
                    <Key>{0}</Key>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <EventTime>2021-01-22T08:58:00</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
            <MessageType>TR</MessageType>
        </EventParameters>
        <ContextCollection>
            <Context>
                <Type>CompanyCode</Type>
                <Value>EDI</Value>
            </Context>
            <Context>
                <Type>StatusCode</Type>
                <Value>{1}</Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>";

		const string SetRequestSuccessMessage = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>2</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>SET</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTR</Value>
			</Context>
		</ContextCollection>
		<AttachedDocumentCollection>
			<AttachedDocument>
				<FileName>SetInvoiceTakenResponse_20200602070107.xml</FileName>
				<ImageData>PFNldEludm9pY2VzVGFrZW5SZXNwb25zZSB4bWxucz0iaHR0cDovL3RlbXB1cmkub3JnLyI+DQogIDxTZXRJbnZvaWNlc1Rha2VuUmVzdWx0IElzU3VjY2VkZWQ9InRydWUiIFZhbHVlPSJ0cnVlIiAvPg0KPC9TZXRJbnZvaWNlc1Rha2VuUmVzcG9uc2U+</ImageData>
				<Type Description=""Miscellaneous Document"">MSC</Type>
				<IsPublished>true</IsPublished>
			</AttachedDocument>
		</AttachedDocumentCollection>
	</Event>
</UniversalEvent>";

		const string SendDocumentResponse_ApprovalAccepted = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>2</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>PAR</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTR</Value>
			</Context>
			<Context>
				<Type>StatusCode</Type>
				<Value>ApprovalAccepted</Value>
			</Context>
		</ContextCollection>
		<AttachedDocumentCollection>
			<AttachedDocument>
				<FileName>SendDocumentResponseResponse_20200602070107.xml</FileName>
				<ImageData>PFNlbmREb2N1bWVudFJlc3BvbnNlUmVzcG9uc2UgeG1sbnM9Imh0dHA6Ly90ZW1wdXJpLm9yZy8iPg0KICAgPFNlbmREb2N1bWVudFJlc3BvbnNlUmVzdWx0IElzU3VjY2VkZWQ9InRydWUiIFZhbHVlPSJ0cnVlIi8+DQo8L1NlbmREb2N1bWVudFJlc3BvbnNlUmVzcG9uc2U+DQo=</ImageData>
				<Type Description=""Miscellaneous Document"">MSC</Type>
				<IsPublished>true</IsPublished>
			</AttachedDocument>
		</AttachedDocumentCollection>
	</Event>
</UniversalEvent>";

		const string SendDocumentResponse_ApprovalDeclinedAsAlreadyRejected = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>2</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>PAR</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTR</Value>
			</Context>
			<Context>
				<Type>StatusCode</Type>
				<Value>ApprovalDeclinedAsAlreadyRejected</Value>
			</Context>
			<Context>
				<Type>Message</Type>
				<Value>Faturanın şuanki durumu yanıt vermek için uygun değil, Durum: Declined.EFT-TESTPORTAL</Value>
			</Context>
		</ContextCollection>
		<AttachedDocumentCollection>
			<AttachedDocument>
				<FileName>SendDocumentResponseResponse_20200602070107.xml</FileName>
				<ImageData>ICAgICAgPFNlbmREb2N1bWVudFJlc3BvbnNlUmVzcG9uc2UgeG1sbnM9Imh0dHA6Ly90ZW1wdXJpLm9yZy8iPg0KICAgICAgICAgPFNlbmREb2N1bWVudFJlc3BvbnNlUmVzdWx0IElzU3VjY2VkZWQ9ImZhbHNlIiBNZXNzYWdlPSJGYXR1cmFuxLFuIMWfdWFua2kgZHVydW11IHlhbsSxdCB2ZXJtZWsgacOnaW4gdXlndW4gZGXEn2lsLCBEdXJ1bTogRGVjbGluZWQuRUZULVRFU1RQT1JUQUwiIFZhbHVlPSJmYWxzZSIvPg0KICAgICAgPC9TZW5kRG9jdW1lbnRSZXNwb25zZVJlc3BvbnNlPg0K</ImageData>
				<Type Description=""Miscellaneous Document"">MSC</Type>
				<IsPublished>true</IsPublished>
			</AttachedDocument>
		</AttachedDocumentCollection>
	</Event>
</UniversalEvent>";

		const string SendDocumentResponse_ApprovalDeclinedAsAlreadyApproved = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>2</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>PAR</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTR</Value>
			</Context>
			<Context>
				<Type>StatusCode</Type>
				<Value>ApprovalDeclinedAsAlreadyApproved</Value>
			</Context>
			<Context>
				<Type>Message</Type>
				<Value>Faturanın şuanki durumu yanıt vermek için uygun değil, Durum: Approved.EFT-TESTPORTAL</Value>
			</Context>
		</ContextCollection>
		<AttachedDocumentCollection>
			<AttachedDocument>
				<FileName>SendDocumentResponseResponse_20200602070107.xml</FileName>
				<ImageData>PFNlbmREb2N1bWVudFJlc3BvbnNlUmVzcG9uc2UgeG1sbnM9Imh0dHA6Ly90ZW1wdXJpLm9yZy8iPg0KICAgPFNlbmREb2N1bWVudFJlc3BvbnNlUmVzdWx0IElzU3VjY2VkZWQ9ImZhbHNlIiBNZXNzYWdlPSJGYXR1cmFuxLFuIMWfdWFua2kgZHVydW11IHlhbsSxdCB2ZXJtZWsgacOnaW4gdXlndW4gZGXEn2lsLCBEdXJ1bTogQXBwcm92ZWQuRUZULVRFU1RQT1JUQUwiIFZhbHVlPSJmYWxzZSIvPg0KPC9TZW5kRG9jdW1lbnRSZXNwb25zZVJlc3BvbnNlPg0K</ImageData>
				<Type Description=""Miscellaneous Document"">MSC</Type>
				<IsPublished>true</IsPublished>
			</AttachedDocument>
		</AttachedDocumentCollection>
	</Event>
</UniversalEvent>";

		const string SendDocumentResponse_RejectionAccepted = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>2</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>PRJ</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTR</Value>
			</Context>
			<Context>
				<Type>StatusCode</Type>
				<Value>RejectionAccepted</Value>
			</Context>
		</ContextCollection>
		<AttachedDocumentCollection>
			<AttachedDocument>
				<FileName>SendDocumentResponseResponse_20200602070107.xml</FileName>
				<ImageData>ICAgICAgPFNlbmREb2N1bWVudFJlc3BvbnNlUmVzcG9uc2UgeG1sbnM9Imh0dHA6Ly90ZW1wdXJpLm9yZy8iPg0KICAgICAgICAgPFNlbmREb2N1bWVudFJlc3BvbnNlUmVzdWx0IElzU3VjY2VkZWQ9InRydWUiIFZhbHVlPSJ0cnVlIi8+DQogICAgICA8L1NlbmREb2N1bWVudFJlc3BvbnNlUmVzcG9uc2U+DQo=</ImageData>
				<Type Description=""Miscellaneous Document"">MSC</Type>
				<IsPublished>true</IsPublished>
			</AttachedDocument>
		</AttachedDocumentCollection>
	</Event>
</UniversalEvent>";

		const string SendDocumentResponse_RejectionDeclinedAsAlreadyApproved = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>00001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>PRJ</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTR</Value>
			</Context>
			<Context>
				<Type>StatusCode</Type>
				<Value>RejectionDeclinedAsAlreadyApproved</Value>
			</Context>
			<Context>
				<Type>Message</Type>
				<Value>Faturanın şuanki durumu yanıt vermek için uygun değil, Durum: Approved.EFT-TESTPORTAL</Value>
			</Context>
		</ContextCollection>
		<AttachedDocumentCollection>
			<AttachedDocument>
				<FileName>SendDocumentResponseResponse_20200602070107.xml</FileName>
				<ImageData>ICAgICAgPFNlbmREb2N1bWVudFJlc3BvbnNlUmVzcG9uc2UgeG1sbnM9Imh0dHA6Ly90ZW1wdXJpLm9yZy8iPg0KICAgICAgICAgPFNlbmREb2N1bWVudFJlc3BvbnNlUmVzdWx0IElzU3VjY2VkZWQ9ImZhbHNlIiBNZXNzYWdlPSJGYXR1cmFuxLFuIMWfdWFua2kgZHVydW11IHlhbsSxdCB2ZXJtZWsgacOnaW4gdXlndW4gZGXEn2lsLCBEdXJ1bTogQXBwcm92ZWQuRUZULVRFU1RQT1JUQUwiIFZhbHVlPSJmYWxzZSIvPg0KICAgICAgPC9TZW5kRG9jdW1lbnRSZXNwb25zZVJlc3BvbnNlPg0K</ImageData>
				<Type Description=""Miscellaneous Document"">MSC</Type>
				<IsPublished>true</IsPublished>
			</AttachedDocument>
		</AttachedDocumentCollection>
	</Event>
</UniversalEvent>";

		const string SendDocumentResponse_RejectionDeclinedAsAlreadyRejected = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>00001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>PRJ</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTR</Value>
			</Context>
			<Context>
				<Type>StatusCode</Type>
				<Value>RejectionDeclinedAsAlreadyRejected</Value>
			</Context>
			<Context>
				<Type>Message</Type>
				<Value>Faturanın şuanki durumu yanıt vermek için uygun değil, Durum: Declined.EFT-TESTPORTAL</Value>
			</Context>
		</ContextCollection>
		<AttachedDocumentCollection>
			<AttachedDocument>
				<FileName>SendDocumentResponseResponse_20200602070107.xml</FileName>
				<ImageData>ICAgICAgPFNlbmREb2N1bWVudFJlc3BvbnNlUmVzcG9uc2UgeG1sbnM9Imh0dHA6Ly90ZW1wdXJpLm9yZy8iPg0KICAgICAgICAgPFNlbmREb2N1bWVudFJlc3BvbnNlUmVzdWx0IElzU3VjY2VkZWQ9ImZhbHNlIiBNZXNzYWdlPSJGYXR1cmFuxLFuIMWfdWFua2kgZHVydW11IHlhbsSxdCB2ZXJtZWsgacOnaW4gdXlndW4gZGXEn2lsLCBEdXJ1bTogRGVjbGluZWQuRUZULVRFU1RQT1JUQUwiIFZhbHVlPSJmYWxzZSIvPg0KICAgICAgPC9TZW5kRG9jdW1lbnRSZXNwb25zZVJlc3BvbnNlPg0K</ImageData>
				<Type Description=""Miscellaneous Document"">MSC</Type>
				<IsPublished>true</IsPublished>
			</AttachedDocument>
		</AttachedDocumentCollection>
	</Event>
</UniversalEvent>";

		const string SendDocumentResponseFailureMessageApplicableForRetry = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>00001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-17T20:35:10</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
			<MessageType>TR</MessageType>
			<MessageSubType>PRJ</MessageSubType>
			<Reason>HTTP POST request to 'https://efatura-test.uyumsoft.com.tr/Services/Integration#SendDocumentResponse' exceeded timeout of 30.0 seconds</Reason>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTR</Value>
			</Context>
			<Context>
				<Type>IsApplicableForRetry</Type>
				<Value>True</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		InvoicingBase Invoice;
		AccEInvoicingBatch Batch;
		AccEInvoicingTransactionPivot Pivot;

		protected TurkeyEInvoiceTestHelper HelperTR;
		UniversalObjectFactory UOFactory => uoFactory ?? (uoFactory = new UniversalObjectFactory());
		UniversalObjectFactory uoFactory;

		internal TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(new BusinessObjectFactory()));
		TestObjectCreator testObjectCreator;
	}
}
