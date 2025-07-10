using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRDLTMessageProcessorTest : TestCaseWithFactory
	{
		public void TestDLTProcessor()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = Constants.EDIInterchangeType.DLT;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageData = ZBlob.FromUTF8(@"2020040909570320200409-ELI-edb54e29-d2aa-49ea-849c-87f29bbf1f42,GOVCBR5AF
2020040909570620200409-ELI-e41c8a55-9a74-4a86-a3cd-96dca925e0c1,GOVCBR5AA
");
			Factory.Save();

			ZQuery query = new ZQuery(CusPollingTransactionSchema.CPT_ParentID, incomingMessage.PK);
			query.AddToFilter(CusPollingTransactionSchema.CPT_ParentTableCode, incomingMessage.TablePrefix);
			var cpt = Factory.Load<CusPollingTransaction>(query);
			AssertEquals(0, cpt.Length);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			cpt = Factory.Load<CusPollingTransaction>(query);

			AssertEquals(2, cpt.Length);

			AssertEquals(CusPollingTransactionStatusList.Codes.Opened, cpt[0].CPT_Status);
			AssertEquals("2020040909570320200409-ELI-edb54e29-d2aa-49ea-849c-87f29bbf1f42", cpt[0].CPT_TransactionID);
			AssertEquals(ElectronicDocumentTypeList.Codes._5AF, cpt[0].CPT_Reference);

			AssertEquals(CusPollingTransactionStatusList.Codes.Opened, cpt[1].CPT_Status);
			AssertEquals("2020040909570620200409-ELI-e41c8a55-9a74-4a86-a3cd-96dca925e0c1", cpt[1].CPT_TransactionID);
			AssertEquals(ElectronicDocumentTypeList.Codes._5AA, cpt[1].CPT_Reference);
		}
	}
}
