using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.NCTS.DataTransfer
{
	public class NctsHeaderUniversalMessagingHelper
	{
		public string SendViaEHub(NctsHeader nctsMovement, NctsMessageFunctionSet messageFunction)
		{
			IShipmentDataContextManager manager = (IShipmentDataContextManager)nctsMovement.GetUniversalDataContextManager();
			var actionInfo = new ActionInfo(null, nctsMovement);
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(actionInfo));
			((NctsHeaderDataContextManager)manager).CountryCode = nctsMovement.TargetNctsSystemCountryCode;
			var headerData = (UniversalShipment)writer.GetDataObject(nctsMovement);
			var dataContext = headerData.DataContext;
			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				ActionPurpose = new CodeDescriptionPair() { Code = messageFunction.Code.Substring(2), Description = "" }
				//Customs.Business.MessageBuilders.MessageSubTypes.Create or Amendment ?
				//NctsUsername ?
				//NctsPassword ?
				//NctsLastRetrievedMessage ?
				//HeaderPk reference ?
			});

			var interchange = nctsMovement.Factory.New<XmlEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			interchange.EI_From = "CARGOWISEONE";
			interchange.EI_To = "NCTS." + nctsMovement.TargetNctsSystemCountryCode;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter().WriteXML(headerData, stream);
				using (var reader = new StreamReader(stream))
				{
					interchange.EI_BodyText = reader.ReadToEnd();
				}
			}
			var message = interchange.ContainedMessages.AddNew();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_MessageText = interchange.EI_BodyText;
			StringBuilder errMsgBuilder = new StringBuilder();
			if (CanSendMessage(message, errMsgBuilder))
			{
				nctsMovement.Messages.Add(message);
				try
				{
					nctsMovement.EffectiveMessageStatus = messageFunction.SentMessageStatusCode;
					nctsMovement.Factory.Save();
					return Res.GetString("02019F6C-E097-41C7-86D4-C68FD5E2EED9", "Universal XML message queued for upload to eHub");
				}
				catch (ZSaveException ex)
				{
					return Res.GetString("9C1BA38F-4B5B-45E1-8E7E-3C2FE69BA785", "The following error was encountered while saving Universal XML message:{0}", ex.Message);
				}
			}
			else
			{
				message.Delete();
				interchange.Delete();
				return errMsgBuilder.ToString();
			}
		}

		protected virtual bool CanSendMessage(XmlEDIMessage message, StringBuilder errMsgBuilder)
		{
			return true;
		}
	}
}
