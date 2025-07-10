using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace ZClientEDI.Test.ServiceTasks.XTCredentialManagement
{
	public static class XTCredentialManagementTestUtils
	{
		public static void CreateRequestAndAckResponseInterchange(BusinessObjectFactory factory, out EDIInterchange request, out EDIInterchange response)
		{
			request = factory.New<EDIInterchange>();
			request.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			request.EI_IsActive = false;
			request.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			request.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			request.EI_TransportType = EDIInterchangeTransportTypeList.Codes.tXT;
			request.EI_From = "Any";
			request.EI_To = "XH";
			request.EI_SessionGUID = request.PK;
			response = factory.New<EDIInterchange>();
			response.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			response.EI_IsActive = true;
			response.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			response.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			response.EI_TransportType = EDIInterchangeTransportTypeList.Codes.tXT;
			response.EI_From = "XH";
			response.EI_To = "Any";
			response.EI_SessionGUID = request.PK;
			response.EI_BodyText = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n<Header>\r\n<SenderID>Any</SenderID>\r\n<RecipientID>XH</RecipientID>\r\n</Header>\r\n<Body>\r\n<UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\" version=\"1.1\">\r\n<Event>\r\n<EventTime>SomeTime</EventTime>\r\n<EventType>IAK</EventType>\r\n<EventParameters>\r\n<MessageType>MessageType</MessageType>\r\n</EventParameters>\r\n</Event>\r\n</UniversalEvent>\r\n</Body>\r\n</UniversalInterchange>";
			factory.Save();
		}

		public static void CreateRequestAndNackResponseInterchange(BusinessObjectFactory factory, out EDIInterchange request, out EDIInterchange response)
		{
			request = factory.New<EDIInterchange>();
			request.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			request.EI_IsActive = false;
			request.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			request.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			request.EI_TransportType = EDIInterchangeTransportTypeList.Codes.tXT;
			request.EI_From = "Any";
			request.EI_To = "XH";
			request.EI_SessionGUID = request.PK;
			response = factory.New<EDIInterchange>();
			response.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			response.EI_IsActive = true;
			response.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			response.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			response.EI_TransportType = EDIInterchangeTransportTypeList.Codes.tXT;
			response.EI_From = "XH";
			response.EI_To = "Any";
			response.EI_SessionGUID = request.PK;
			response.EI_BodyText = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n<Header>\r\n<SenderID>Any</SenderID>\r\n<RecipientID>XH</RecipientID>\r\n</Header>\r\n<Body>\r\n<UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\" version=\"1.1\">\r\n<Event>\r\n<EventTime>SomeTime</EventTime>\r\n<EventType>IRJ</EventType>\r\n<EventParameters>\r\n<MessageType>Message Type</MessageType>\r\n<Reason>It failed for whatever reason</Reason>\r\n</EventParameters>\r\n</Event>\r\n</UniversalEvent>\r\n</Body>\r\n</UniversalInterchange>";
			factory.Save();
		}
	}
}
