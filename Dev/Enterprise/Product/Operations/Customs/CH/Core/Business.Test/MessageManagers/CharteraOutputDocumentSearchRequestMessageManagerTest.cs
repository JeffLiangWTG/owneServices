using System;
using CargoWise.Customs.CH.MessageDefinitions.Chartera.documentsearchrequest_v1;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CharteraOutputDocumentSearchRequestMessageManager))]
sealed class CharteraOutputDocumentSearchRequestMessageManagerTest : TestCaseWithFactory
{
	[TestDate(2023, 12, 5, 9, 0, 0)]
	public void TestGenerateMessage() => CombineAssertions(() =>
	{
		using (CHCustomsDataRegistry.Instance.MaxNumberOfSearchAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 14))
		{
			var credentials = CredentialsTestHelper.CreateCurrentCompanyTokenCredential();

			var creationTimeFrom = new ZDateTime(2023, 12, 3, 12, 15, 0);
			var creationTimeTo = new ZDateTime(2023, 12, 4, 10, 20, 0);
			var creationTimeFromUtc = creationTimeFrom.ToUniversalBranchTime();
			var creationTimeToUtc = creationTimeTo.ToUniversalBranchTime();
			AssertNotEquals("Pre-condition: Local time not UTC", creationTimeFrom, creationTimeFromUtc);

			SendingObject.CreationTimeFrom = creationTimeFrom;
			SendingObject.CreationTimeTo = creationTimeTo;

			Manager.GenerateMessages();
			Factory.Save();

			var transaction = Factory.LoadTop1<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_TransactionID, SendingObject.ProcessId));
			AssertNotNull("Transaction created", transaction);
			AssertEquals("CPT_ApplicationCode", CusPollingTransaction.ApplicationCodes.CHCustomsCharteraOutput, transaction.CPT_ApplicationCode);
			AssertEquals("CPT_Type", CompanyPollingTransaction.TransactionTypes.DocumentSearchRequest, transaction.CPT_Type);
			AssertEquals("CPT_ParentTableCode", GlbCompanySchema.Constants.Prefix, transaction.CPT_ParentTableCode);
			AssertEquals("CPT_ParentID", GlbCompany.CurrentCompany.PK, transaction.CPT_ParentID);
			AssertEquals("CPT_Status", CompanyPollingTransaction.StatusCodes.Skip, transaction.CPT_Status);
			AssertEquals("CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
			AssertEquals("CPT_NumberOfAttempts", new ZByte(14), transaction.CPT_NumberOfAttempts);
			AssertEquals("CPT_Reference", CompanyPollingTransaction.ReferenceCodes.Manual, transaction.CPT_Reference);

			var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationReference, SendingObject.ProcessId));
			AssertNotNull("EDIMEssage created", ediMessage);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.CHCustomsCharteraOutput, ediMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeCodeList.Codes.REQ, ediMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest, ediMessage.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediMessage.EM_ReceiveTransmit);
			AssertNotEquals("EM_MessageNum", ZString.Empty, ediMessage.EM_MessageNum);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, ediMessage.EM_Status);
			AssertEquals("EM_LinkTable", CusPollingTransactionSchema.Constants.TableName, ediMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", transaction.PK, ediMessage.EM_LinkUniqueID);
			AssertEquals("EM_GP", credentials.PK, ediMessage.EM_GP);

			var xmlMessage = sendingObject.ToMessageString();
			var xmlDocument = XmlObjectSerializer.Deserialize<DocumentSearchRequest>(xmlMessage);
			AssertEquals("ProcessId", sendingObject.ProcessId, xmlDocument.CurrentQuery.ProcessId);
			AssertEquals($"From {ZDateTime.Now} {ZDateTime.UtcNow}", creationTimeFromUtc.ToDateTime(), xmlDocument.CurrentQuery.From);
			AssertEquals("To", creationTimeToUtc.ToDateTime(), xmlDocument.CurrentQuery.To);

			EventsTestHelper.AssertEventAdded(GlbCompany.CurrentCompany, Events.ManualDocumentSearch, "|FRM=03.12.23 12:15|TO=04.12.23 10:20|TYP=Request");
		}
	});

	[TestDate(2023, 1, 12)]
	public void TestRollbackOnSaveFailed() => CombineAssertions(() =>
	{
		var transactionCountBefore = CountTransactions();
		var logCountBefore = CountLogs();

		SendingObject.CreationTimeFrom = ZDateTime.Now;
		SendingObject.CreationTimeTo = ZDateTime.Now;
		Manager.GenerateMessages();
		Manager.RollbackOnSaveFailed();
		Factory.Save();

		AssertEquals("Transaction deleted", transactionCountBefore, CountTransactions());
		AssertEquals("Company log deleted", logCountBefore, CountLogs());

		int CountLogs() => GlbCompany.GetCurrentCompany(Factory).Logs.GetAllLogs().Count;
		int CountTransactions() => Factory.Load<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow)).Length;
	});

	CharteraOutputDocumentSearchSendingObject SendingObject => sendingObject ?? (sendingObject = new CharteraOutputDocumentSearchSendingObject(Factory));
	CharteraOutputDocumentSearchSendingObject sendingObject;

	CharteraOutputDocumentSearchRequestMessageManager Manager => manager ?? (manager = new CharteraOutputDocumentSearchRequestMessageManager(SendingObject));
	CharteraOutputDocumentSearchRequestMessageManager manager;
}
