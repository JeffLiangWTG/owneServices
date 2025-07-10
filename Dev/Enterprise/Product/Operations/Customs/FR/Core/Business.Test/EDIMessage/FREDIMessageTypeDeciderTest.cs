using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class FREDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var excMessage = CreateEDIMessage(MessageTypeList.Codes.EXC);
			var imcMessage = CreateEDIMessage(MessageTypeList.Codes.IMC);
			var exdMessage = CreateEDIMessage(MessageTypeList.Codes.EXD);
			var imdMessage = CreateEDIMessage(MessageTypeList.Codes.IMD);
			var dcgMessage = CreateEDIMessage(MessageTypeList.Codes.DCG);
			var cinMessage = CreateEDIMessage(MessageTypeList.Codes.CIN);
			var arrMessage = CreateEDIMessage(MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.ARR);
			var depMessage = CreateEDIMessage(MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.DEP);
			var ecsMessage = CreateEDIMessage(MessageTypeList.Codes.ECS);
			var decMessage = CreateEDIMessage(MessageTypeList.Codes.DEC);
			var tp5Message = CreateEDIMessage(MessageTypeList.Codes.TP5);
			var frMessage = CreateEDIMessage("XXX");

			Factory.Save();

			var factory = new BusinessObjectFactory();

			AssertType<DeltaCExportFREDIMessage>(factory.Load<EDIMessage>(excMessage.PK));
			AssertType<DeltaCImportFREDIMessage>(factory.Load<EDIMessage>(imcMessage.PK));
			AssertType<DeltaDExportFREDIMessage>(factory.Load<EDIMessage>(exdMessage.PK));
			AssertType<DeltaDImportFREDIMessage>(factory.Load<EDIMessage>(imdMessage.PK));
			AssertType<DCGResponseFREDIMessage>(factory.Load<EDIMessage>(dcgMessage.PK));
			AssertType<CINImportResponseFREDIMessage>(factory.Load<EDIMessage>(cinMessage.PK));
			AssertType<ECSArrivalFREDIMessage>(factory.Load<EDIMessage>(arrMessage.PK));
			AssertType<ECSDepartureFREDIMessage>(factory.Load<EDIMessage>(depMessage.PK));
			AssertType<ECSFREDIMessage>(factory.Load<EDIMessage>(ecsMessage.PK));
			AssertType<DeltaIEFREDIMessage>(factory.Load<EDIMessage>(decMessage.PK));
			AssertType<NCTSFREDIMessage>(factory.Load<EDIMessage>(tp5Message.PK));
			AssertType<FREDIMessage>(factory.Load<EDIMessage>(frMessage.PK));
		}

		EDIMessage CreateEDIMessage(string messageType, string messageSubType = null)
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType ?? ZString.Empty;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;

			return message;
		}
	}
}
