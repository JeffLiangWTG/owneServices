using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia.Testing
{
	public class EInvoicingEventMessageMYProcessorTest : TestCaseWithFactory
	{
		#region Submit Transaction

		public void TestProcess_WhenSubmissionTransactionReturnAccecpted()
		{
			var (arInvoice, pivotSubmit, batchSubmit) = ProcessSubmit(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction,
				submitResult: MalaysiaProcessorConstants.SubmitResult.Accepted, governmentAllocatedBatchRefNumber: "123", govermentAllocatedID: "456");

			AssertEquals("123", batchSubmit.AIB_GovernmentAllocatedNumber);
			AssertResult(arInvoice, currentPivot: pivotSubmit, currentPivotExpectStatus: EInvoicingPivotState.Delivered,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Delivered,
				nextPivotActionType: EInvoicingPivotActionType.StatusCheck, nextPivotCount: 1);

			AssertEquals("456", arInvoice.AH_GovernmentAllocatedID);
		}

		public void TestProcess_WhenSubmissionTransactionReturnRejected()
		{
			var rejectMessage = "Submit is Rejected";
			var (arInvoice, pivotSubmit, batchSubmit) = ProcessSubmit(AutoEvents.InterchangeRejectedCode, MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction, submitResult: MalaysiaProcessorConstants.SubmitResult.Rejected, error: rejectMessage);

			AssertEquals(string.Empty, batchSubmit.AIB_GovernmentAllocatedNumber);
			AssertResult(arInvoice, currentPivot: pivotSubmit, currentPivotExpectStatus: EInvoicingPivotState.Failed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Failed,
				nextPivotActionType: EInvoicingPivotActionType.StatusCheck, nextPivotCount: 0, rejectMessage);
		}

		public void TestProcess_WhenSubmissionTransactionReturnError()
		{
			var rejectMessage = "Submit is Error";
			var (arInvoice, pivotSubmit, batchSubmit) = ProcessSubmit(AutoEvents.InterchangeRejectedCode, MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction, error: rejectMessage);

			AssertEquals(string.Empty, batchSubmit.AIB_GovernmentAllocatedNumber);
			AssertResult(arInvoice, currentPivot: pivotSubmit, currentPivotExpectStatus: EInvoicingPivotState.Failed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Failed,
				nextPivotActionType: EInvoicingPivotActionType.StatusCheck, nextPivotCount: 0, rejectMessage);
		}

		(TransactionHeader, AccEInvoicingTransactionPivot, AccEInvoicingBatch) ProcessSubmit(string eventType, string messageSubType = "", string submitResult = "", string submissionStatus = "", string error = "", string governmentAllocatedBatchRefNumber = "", string govermentAllocatedID = "")
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);

			var pivotSubmit = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Sent);
			var batchSubmit = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotSubmit, 1, Constants.EInvoicingBatchState.Sent);

			Factory.Save();

			Process(batchSubmit, eventType, messageSubType, submitResult, submissionStatus, error, governmentAllocatedBatchRefNumber, govermentAllocatedID);

			return (arInvoice, pivotSubmit, batchSubmit);
		}

		#endregion

		#region GetSubmission

		public void TestProcess_WhenGetSubmissionReturnValid()
		{
			var (arInvoice, pivotSubmit, pivotGetSubmission) = ProcessQueryActionType(EInvoicingPivotActionType.StatusCheck, AutoEvents.InterchangeAcknowledgedCode,
				MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, submissionStatus: MalaysiaProcessorConstants.SubmissionStatus.Valid);

			AssertResult(arInvoice, currentPivot: pivotGetSubmission, currentPivotExpectStatus: EInvoicingPivotState.Succeed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Succeed,
				nextPivotActionType: EInvoicingPivotActionType.DocumentAction, nextPivotCount: 1);
		}

		public void TestProcess_WhenGetSubmissionReturnSubmitted()
		{
			var (arInvoice, pivotSubmit, pivotGetSubmission) = ProcessQueryActionType(EInvoicingPivotActionType.StatusCheck, AutoEvents.InterchangeAcknowledgedCode,
				MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, submissionStatus: MalaysiaProcessorConstants.SubmissionStatus.Submitted);

			AssertResult(arInvoice, currentPivot: pivotGetSubmission, currentPivotExpectStatus: EInvoicingPivotState.Succeed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.InProcessing,
				nextPivotActionType: EInvoicingPivotActionType.DocumentAction, nextPivotCount: 0);
		}

		public void TestProcess_WhenGetSubmissionReturnInvalid()
		{
			var (arInvoice, pivotSubmit, pivotGetSubmission) = ProcessQueryActionType(EInvoicingPivotActionType.StatusCheck, AutoEvents.InterchangeAcknowledgedCode,
				MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, submissionStatus: MalaysiaProcessorConstants.SubmissionStatus.Invalid);

			AssertResult(arInvoice, currentPivot: pivotGetSubmission, currentPivotExpectStatus: EInvoicingPivotState.Succeed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Delivered,
				nextPivotActionType: EInvoicingPivotActionType.DocumentDetail, nextPivotCount: 1);
		}

		public void TestProcess_WhenGetSubmissionReturnError()
		{
			var errorMessage = "Get Submission Fail";

			var (arInvoice, pivotSubmit, pivotGetSubmission) = ProcessQueryActionType(EInvoicingPivotActionType.StatusCheck, AutoEvents.InterchangeRejectedCode,
				MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, error: errorMessage);

			AssertResult(arInvoice, currentPivot: pivotGetSubmission, currentPivotExpectStatus: EInvoicingPivotState.Failed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Failed,
				nextPivotActionType: EInvoicingPivotActionType.DocumentDetail, nextPivotCount: 0, errorMessage);
		}

		public void TestProcess_WhenGetSubmissionReturnIAK_ClearError()
		{
			var errorMessage = "Get Submission Fail";

			var (arInvoice, pivotSubmit, pivotGetSubmission) = ProcessQueryActionType(EInvoicingPivotActionType.StatusCheck, AutoEvents.InterchangeRejectedCode,
					MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, error: errorMessage);
			pivotGetSubmission.AIP_Status = EInvoicingPivotState.Discarded;

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			var batchQuery = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 3, Constants.EInvoicingBatchState.Sent);

			Process(batchQuery, AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, submissionStatus: MalaysiaProcessorConstants.SubmissionStatus.Valid);

			AssertResult(arInvoice, currentPivot: pivotQuery, currentPivotExpectStatus: EInvoicingPivotState.Succeed,
					submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Succeed,
					nextPivotActionType: EInvoicingPivotActionType.DocumentAction, nextPivotCount: 1, error: string.Empty);
		}

		public void TestProcess_WhenRepeatedGetSubmissionReturnInvalid()
		{
			var (arInvoice, pivotSubmit, pivotGetSubmission) = ProcessQueryActionType(EInvoicingPivotActionType.StatusCheck, AutoEvents.InterchangeAcknowledgedCode,
				MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, submissionStatus: MalaysiaProcessorConstants.SubmissionStatus.Invalid);
			pivotGetSubmission.AIP_Status = EInvoicingPivotState.Discarded;

			var secondGetSubmissionPivot = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			var secondGetSubmissionBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(secondGetSubmissionPivot, 3, Constants.EInvoicingBatchState.Sent);
			Process(secondGetSubmissionBatch, AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, submissionStatus: MalaysiaProcessorConstants.SubmissionStatus.Invalid);
			AssertResult(arInvoice, currentPivot: secondGetSubmissionPivot, currentPivotExpectStatus: EInvoicingPivotState.Succeed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Delivered,
				nextPivotActionType: EInvoicingPivotActionType.DocumentDetail, nextPivotCount: 1);
			AssertDiscardPivot(arInvoice, EInvoicingPivotActionType.DocumentDetail, EInvoicingPivotState.Discarded);
		}

		void AssertDiscardPivot(TransactionHeader arInvoice, string actionType, string status)
		{
			Assert(Factory.Exists(typeof(AccEInvoicingTransactionPivot),
				new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, status)));
		}

		public void TestProcess_AmendentCreatePivotWhenReturnValid()
		{
			TestProcess_AmendentCreatePivot(true);
		}

		public void TestProcess_AmendentCreatePivotWhenReturnInValid()
		{
			TestProcess_AmendentCreatePivot(false);
		}

		public void TestProcess_AmendentCreatePivot(bool isOriginalSuccess)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var (arInvoice, pivotSubmit, batchSubmit) = ProcessSubmit(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction,
						submitResult: MalaysiaProcessorConstants.SubmitResult.Accepted, governmentAllocatedBatchRefNumber: "123", govermentAllocatedID: "456");

				var amendInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				amendInvoice.AH_TransactionBelongsToGroup = arInvoice.PK;
				amendInvoice.AH_ComplianceSubType = "02";
				amendInvoice.AH_SystemCreateTimeUtc = ZDateTime.Today;
				var amendInvoiceline = TestObjectCreator.CreateInvoiceLine(amendInvoice, 110m, arInvoice.TransactionCurrency, 1m);
				amendInvoiceline.AL_AT = TestObjectCreator.ExtraServiceTax.PK;

				var amendCredit = TestObjectCreator.CreateARCreditNote("AR003", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, "desc");
				amendCredit.AH_TransactionBelongsToGroup = arInvoice.PK;
				amendCredit.AH_ComplianceSubType = "03";
				amendCredit.AH_SystemCreateTimeUtc = ZDateTime.Today;
				var amendCreditline = TestObjectCreator.CreateARCreditNoteLine(amendCredit, null, TestObjectCreator.FRT, 2000.00m, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Test");
				amendCreditline.AL_AT = TestObjectCreator.ExtraServiceTax.PK;

				var getSubmissionPivot = GetActionTypePivot(arInvoice, EInvoicingPivotActionType.StatusCheck);
				var getSubmissionBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(getSubmissionPivot, 3, EInvoicingBatchState.Sent);
				if (isOriginalSuccess)
				{
					Process(getSubmissionBatch, AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, submissionStatus: MalaysiaProcessorConstants.SubmissionStatus.Valid);
					AssertResult(arInvoice, currentPivot: getSubmissionPivot, currentPivotExpectStatus: EInvoicingPivotState.Succeed,
							submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Succeed,
							nextPivotActionType: EInvoicingPivotActionType.DocumentAction, nextPivotCount: 1);
					AssertNotNull(GetActionTypePivot(amendInvoice, EInvoicingPivotActionType.Submit));
					AssertNotNull(GetActionTypePivot(amendCredit, EInvoicingPivotActionType.Submit));
				}
				else
				{
					Process(getSubmissionBatch, AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, submissionStatus: MalaysiaProcessorConstants.SubmissionStatus.Invalid);
					AssertResult(arInvoice, currentPivot: getSubmissionPivot, currentPivotExpectStatus: EInvoicingPivotState.Succeed,
							submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Delivered,
							nextPivotActionType: EInvoicingPivotActionType.DocumentDetail, nextPivotCount: 1);
					AssertNull(GetActionTypePivot(amendInvoice, EInvoicingPivotActionType.Submit));
					AssertNull(GetActionTypePivot(amendCredit, EInvoicingPivotActionType.Submit));
				}
			}
		}

		AccEInvoicingTransactionPivot GetActionTypePivot(AccTransactionHeader header, string actionType)
		{
			return Factory.LoadTop1<AccEInvoicingTransactionPivot>(
					new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, header.PK)
							.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType));
		}

		public void TestProcess_WhenGetSubmissionReturnEmpty()
		{
			var (arInvoice, pivotSubmit, pivotGetSubmission) = ProcessQueryActionType(EInvoicingPivotActionType.StatusCheck, AutoEvents.InterchangeAcknowledgedCode,
					MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, submissionStatus: string.Empty);

			AssertResult(arInvoice, currentPivot: pivotGetSubmission, currentPivotExpectStatus: EInvoicingPivotState.Succeed,
					submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.InProcessing,
					nextPivotActionType: EInvoicingPivotActionType.DocumentAction, nextPivotCount: 0);
		}

		#endregion

		#region GetDocument

		public void TestProcess_WhenGetDocumentReturnSuccess()
		{
			var (arInvoice, pivotSubmit, pivotGetDocument) = ProcessQueryActionType(EInvoicingPivotActionType.DocumentAction, AutoEvents.InterchangeAcknowledgedCode,
				MalaysiaEInvoiceAPICommandList.Codes.GetDocument);

			pivotSubmit.AIP_Status = EInvoicingPivotState.Succeed;

			AssertResult(arInvoice, currentPivot: pivotGetDocument, currentPivotExpectStatus: EInvoicingPivotState.Succeed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Succeed,
				nextPivotActionType: "", nextPivotCount: 0);
		}

		public void TestProcess_WhenGetDocumentReturnError()
		{
			var errorMessage = "Get Document is Fail";

			var (arInvoice, pivotSubmit, pivotGetDocument) = ProcessQueryActionType(EInvoicingPivotActionType.DocumentAction, AutoEvents.InterchangeRejectedCode,
				MalaysiaEInvoiceAPICommandList.Codes.GetDocument, error: errorMessage);

			pivotSubmit.AIP_Status = EInvoicingPivotState.Succeed;

			AssertEquals(errorMessage, pivotGetDocument.AIP_ErrorDescription);
			AssertResult(arInvoice, currentPivot: pivotGetDocument, currentPivotExpectStatus: EInvoicingPivotState.Failed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Succeed,
				nextPivotActionType: "", nextPivotCount: 0, error: "");
		}

		#endregion

		#region GetDocumentDetail

		public void TestProcess_WhenGetDocumentDetailReturnSuccess()
		{
			var (arInvoice, pivotSubmit, pivotGetDocumentDetail) = ProcessQueryActionType(EInvoicingPivotActionType.DocumentDetail, AutoEvents.InterchangeAcknowledgedCode,
				MalaysiaEInvoiceAPICommandList.Codes.GetDocumentDetail);

			AssertResult(arInvoice, currentPivot: pivotGetDocumentDetail, currentPivotExpectStatus: EInvoicingPivotState.Succeed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Failed,
				nextPivotActionType: "", nextPivotCount: 0);
		}

		public void TestProcess_WhenGetDocumentDetailReturnError()
		{
			var errorMessage = "Get Document Detail is Fail";

			var (arInvoice, pivotSubmit, pivotGetDocumentDetail) = ProcessQueryActionType(EInvoicingPivotActionType.DocumentDetail, AutoEvents.InterchangeRejectedCode,
				MalaysiaEInvoiceAPICommandList.Codes.GetDocumentDetail, error: errorMessage);

			AssertResult(arInvoice, currentPivot: pivotGetDocumentDetail, currentPivotExpectStatus: EInvoicingPivotState.Failed,
				submitPivot: pivotSubmit, submitPivotExpectStatus: EInvoicingPivotState.Failed,
				nextPivotActionType: "", nextPivotCount: 0, error: "");
		}

		#endregion

		#region Extra

		public void TestProcess_CannotOverrideGovernmentID()
		{
			var (arInvoice, pivotSubmit, batchSubmit) = ProcessSubmit(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction,
				submitResult: MalaysiaProcessorConstants.SubmitResult.Accepted, governmentAllocatedBatchRefNumber: "123", govermentAllocatedID: "456");

			AssertEquals("456", arInvoice.AH_GovernmentAllocatedID);

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			var batchQuery = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 2, Constants.EInvoicingBatchState.Sent);

			Process(batchQuery, AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetSubmission,
				submissionStatus: MalaysiaProcessorConstants.SubmissionStatus.Valid, govermentAllocatedID: "789");

			var transactionIdentifier = FormattableString.Invariant($"{arInvoice.AH_Ledger} {arInvoice.AH_TransactionType} {arInvoice.AH_TransactionNum}");  // Concatenation of database fields. Always used in ResString.
			var expectWarning = $"Government Allocated ID can't be overridden. Existing value: '456' & New value: '789' for invoice batch 2, transaction {transactionIdentifier}, in {arInvoice.Company.CompanyName}.";
			Assert(Logger.Logs.Any(x => x.Message.Contains(expectWarning)));
			AssertEquals("456", arInvoice.AH_GovernmentAllocatedID);
		}

		#endregion

		void AssertResult(TransactionHeader arInvoice, AccEInvoicingTransactionPivot currentPivot, string currentPivotExpectStatus, AccEInvoicingTransactionPivot submitPivot, string submitPivotExpectStatus, string nextPivotActionType, int nextPivotCount, string error = "")
		{
			AssertEquals(currentPivotExpectStatus, currentPivot.AIP_Status);
			AssertEquals(submitPivotExpectStatus, submitPivot.AIP_Status);
			AssertEquals(error, submitPivot.AIP_ErrorDescription);

			if (string.IsNullOrEmpty(nextPivotActionType))
			{
				return;
			}

			var getSubmissionPivot = Factory.Load<AccEInvoicingTransactionPivot>(
				new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, nextPivotActionType)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued));
			AssertEquals(nextPivotCount, getSubmissionPivot.Length);
			if (nextPivotCount > 0)
			{
				AssertEquals(EInvoicingPivotState.Queued, getSubmissionPivot[0].AIP_Status);
			}
		}

		(TransactionHeader, AccEInvoicingTransactionPivot, AccEInvoicingTransactionPivot) ProcessQueryActionType(string queryPivotActionType, string eventType, string messageSubType = "", string submitResult = "", string submissionStatus = "", string error = "")
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);

			var pivotSubmit = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			var batchSubmit = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotSubmit, 1, Constants.EInvoicingBatchState.Sent);

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, queryPivotActionType, EInvoicingPivotState.Sent);
			var batchQuery = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 2, Constants.EInvoicingBatchState.Sent);

			Factory.Save();

			Process(batchQuery, eventType, messageSubType, submitResult, submissionStatus, error);

			return (arInvoice, pivotSubmit, pivotQuery);
		}

		void Process(AccEInvoicingBatch batch, string eventType, string messageSubType = "", string submitResult = "", string submissionStatus = "", string reason = "", string governmentAllocatedBatchRefNumber = "", string govermentAllocatedID = "")
		{
			var universalEvent = GetUniversalEvent(eventType, messageSubType, submitResult, submissionStatus, reason, governmentAllocatedBatchRefNumber, govermentAllocatedID);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, EDIMessage, universalEvent, batch);
			var processor = new EInvoicingEventMessageMYProcessor(eventMessageProcessorData, CountryEInvoicingObjectFactory);
			processor.Process();
		}

		UniversalEvent GetUniversalEvent(
			string eventType,
			string messageSubType,
			string submitResult,
			string submissionStatus,
			string reason,
			string governmentAllocatedBatchRefNumber = "",
			string governmentAllocatedID = ""
			)
		{
			var submissionStatusContext = string.IsNullOrEmpty(submissionStatus)
					? string.Empty
					: $@"
<Context>
	<Type>EINV_MY_SubmissionStatus</Type>
	<Value>{submissionStatus}</Value>
</Context>";

			var governmentAllocatedBatchRefNumberContext = string.IsNullOrEmpty(governmentAllocatedBatchRefNumber)
				? string.Empty
				: $@"
<Context>
	<Type>EINV_GovtAllocatedBatchRefNumber</Type>
	<Value>{governmentAllocatedBatchRefNumber}</Value>
</Context>
";

			var governmentAllocatedIDContext = string.IsNullOrEmpty(governmentAllocatedID)
				? string.Empty
				: $@"
<Context>
	<Type>EINV_GovtAllocatedTransactionRefNumber</Type>
	<Value>{governmentAllocatedID}</Value>
</Context>
";

			EDIMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2024-03-29T09:30:10</EventTime>
		<EventType>{eventType}</EventType>
		<EventParameters>
			<MessageType>MY</MessageType>
			<MessageSubType>{messageSubType}</MessageSubType>
			<Reason>{reason}</Reason>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>EINV_MY_SubmitResult</Type>
				<Value>{submitResult}</Value>
			</Context>
			<Context>
				<Type>EINV_MY_SubmissionStatus</Type>
				<Value>{submissionStatus}</Value>
			</Context>
			{submissionStatusContext}
			{governmentAllocatedBatchRefNumberContext}
			{governmentAllocatedIDContext}
		</ContextCollection>
	</Event>
</UniversalEvent>";
			return EDIMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
		}

		IXmlSessionTracker Logger => logger ?? (logger = new XmlSessionTracker(new ServiceTaskLogForTesting()));
		IXmlSessionTracker logger;

		EDIMessage EDIMessage => ediMessage ?? (ediMessage = EDIMessageTestFactory.New(Factory));
		EDIMessage ediMessage;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		CountryEInvoicingObjectFactory CountryEInvoicingObjectFactory => countryEInvoicingObjectFactory ?? (countryEInvoicingObjectFactory = new MalaysiaEInvoicingObjectFactory());
		CountryEInvoicingObjectFactory countryEInvoicingObjectFactory;
	}
}
