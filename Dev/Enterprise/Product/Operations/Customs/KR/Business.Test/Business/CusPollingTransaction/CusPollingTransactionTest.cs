using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Business.CusPollingTransaction;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusPollingTransaction))]
	sealed class CusPollingTransactionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var cusPollingTransaction = Factory.New<CusPollingTransaction>();
			AssertEquals("KRC", cusPollingTransaction.CPT_ApplicationCode);
		}
		public void TestSettingParentBusinessObject()
		{
			var dltMessage = Factory.New<EDIMessage>();
			dltMessage.EM_MessageType = "DLT";

			var cusPollingTransaction1 = Factory.New<CusPollingTransaction>();
			cusPollingTransaction1.CPT_ParentID = dltMessage.PK;
			cusPollingTransaction1.CPT_ParentTableCode = dltMessage.TablePrefix;
			AssertNotNull(cusPollingTransaction1.ParentDLTMessage);
			AssertEquals(dltMessage.PK, cusPollingTransaction1.ParentDLTMessage.PK);
		}
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var queryGUID = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";
			var cusPollingTransaction = Factory.New<CusPollingTransaction>();
			cusPollingTransaction.CPT_ApplicationCode = ApplicationCodes.KRCustoms;
			cusPollingTransaction.CPT_Reference = ElectronicDocumentTypeList.Codes._830;
			cusPollingTransaction.CPT_Status = CusPollingTransactionStatusList.Codes.Opened;
			cusPollingTransaction.CPT_NumberOfAttempts = 1;
			cusPollingTransaction.CPT_TransactionID = queryGUID;
			return cusPollingTransaction;
		}

		public void TestRCVItem()
		{
			var cpt = (CusPollingTransaction)GetNewBusinessObject();

			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_ApplicationCode = ApplicationCodes.KRCustoms;
			outgoingInterchange.EI_InterchangeNum = "2022111718093511931234";
			outgoingInterchange.EI_InterchangeType = EDIInterchangeType.DOC;
			outgoingInterchange.EI_Status = EDIInterchange.Status.Queued;
			outgoingInterchange.EI_SessionGUID = new ZGuid("9B6AC3A4-DCEF-430E-AA65-2AD6BEB9C8FD");

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodes.KRCustoms;
			outgoingMessage.EM_MessageType = EDIInterchangeType.DOC;
			outgoingMessage.EM_Status = EDIMessage.Status.Queued;
			outgoingMessage.EM_LinkedObject = cpt;
			outgoingMessage.EM_EI = outgoingInterchange.PK;

			Factory.Save();

			AssertNotNull(cpt.DOCMessage);
			AssertEquals("1", cpt.DOCMessage.EM_MessageNum);
			AssertEquals(EDIMessageStatusList.Descriptions.Queued, cpt.DOCMessage.EM_StatusDescription);
			AssertEquals("2022111718093511931234", cpt.DOCMessage.Interchange.EI_InterchangeNum);
			AssertEquals(EDIInterchangeType.DOC, cpt.DOCMessage.Interchange.EI_InterchangeType);
			AssertEquals(EDIInterchange.Status.Queued, cpt.DOCMessage.Interchange.EI_Status);
			AssertEquals(EDIMessage.Status.Queued, cpt.DOCMessage.EM_Status);

			AssertNull(cpt.RCVMessage);

			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = ApplicationCodes.KRCustoms;
			incomingInterchange.EI_InterchangeNum = "2022111818093511935678";
			incomingInterchange.EI_InterchangeType = EDIInterchangeType.RSP;
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_Status = EDIInterchange.Status.Received;
			incomingInterchange.EI_SessionGUID = outgoingInterchange.EI_SessionGUID;

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = ApplicationCodes.KRCustoms;
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5AS;
			incomingMessage.EM_Status = EDIMessage.Status.Received;
			incomingMessage.EM_EI = incomingInterchange.PK;

			Factory.Save();

			AssertNotNull(cpt.RCVMessage);
			AssertEquals("2", cpt.RCVMessage.EM_MessageNum);
			AssertEquals(EDIMessageStatusList.Descriptions.Received, cpt.RCVMessage.EM_StatusDescription);
			AssertEquals("2022111818093511935678", cpt.RCVMessage.Interchange.EI_InterchangeNum);
			AssertEquals(EDIInterchangeType.RSP, cpt.RCVMessage.Interchange.EI_InterchangeType);
			AssertEquals(EDIInterchange.Status.Received, cpt.RCVMessage.Interchange.EI_Status);
			AssertEquals(EDIMessageStatusList.Codes.Received, cpt.RCVMessage.EM_Status);
		}
	}
}
