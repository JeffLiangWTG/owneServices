using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315
{
	public class ICSEDIMessagingHelper
	{
		public ICSEDIMessagingHelper(AsycudaManifestHeaderBase asycudaManifestHeader)
		{
			AsycudaManifestHeader = asycudaManifestHeader;
		}

		public bool SendMessageAndSave(ZString messageType)
		{
			var result = false;

			GbEDIMessage message = null;

			try
			{
				message = (AsycudaManifestHeader is AsycudaManifestHeaderSS) ? Factory.New<IcsSsGreatBritainEDIMessage>() : Factory.New<IcsNorthernIrelandEDIMessage>();
				BuildMessage(message, messageType);
				AsycudaManifestHeader.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting;

				Factory.Save();

				if (message is IcsSsGreatBritainEDIMessage)
				{
					message.EM_MessageInterpretation = message.EM_MessageInterpretation.Replace(IcsSsGreatBritainEDIMessage.MessageNumberPlaceHolderXml, message.EM_MessageNum);
				}

				result = true;
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				if (message != null && !message.IsInDatabase)
				{
					message.Delete();
				}
			}

			return result;
		}

		AsycudaManifestHeaderBase AsycudaManifestHeader { get; }

		BusinessObjectFactory Factory => AsycudaManifestHeader.Factory;

		void BuildMessage(GbEDIMessage newMessage, ZString messageType)
		{
			if (messageType == GBMessageTypeList.Codes.New)
			{
				if (newMessage is IcsNorthernIrelandEDIMessage)
				{
					var messageWrapper = new DeclarationWrapper(AsycudaManifestHeader);
					var ie315Builder = new IE315MessageBuilder(messageWrapper);
					var ie315Message = ie315Builder.Build();

					var authenticator = new HMRCmarkAuthenticator();
					var ie315MessageSigned = authenticator.SignSoapMessage(ie315Message);
					newMessage.EM_MessageText = ie315MessageSigned;
				}
				else
				{
					var messageWrapper = new SafetyAndSecurity.Messaging.CC315A.DeclarationWrapper(AsycudaManifestHeader);
					var cc315Builder = new CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.CC315AMessageBuilder(messageWrapper);
					var xmlMessage = ((IXmlMessageBuilder)cc315Builder).GenerateXmlMessage();
					newMessage.EM_MessageText = xmlMessage.GetSerializedString();
					newMessage.EM_MessageInterpretation = new CC315AMessagePrettier(messageWrapper).MakeHumanReadable();
				}
				newMessage.EM_MessageSubType = Constants.ICSMessageSubTypes.NEW;
			}
			else if (messageType == GBMessageTypeList.Codes.Amend)
			{
				if (newMessage is IcsSsGreatBritainEDIMessage)
				{
					var messageWrapper = new SafetyAndSecurity.Messaging.CC313A.DeclarationWrapper(AsycudaManifestHeader);
					var cc313Builder = new CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.CC313AMessageBuilder(messageWrapper);
					var xmlMessage = ((IXmlMessageBuilder)cc313Builder).GenerateXmlMessage();
					newMessage.EM_MessageText = xmlMessage.GetSerializedString();
				}
				newMessage.EM_MessageSubType = Constants.ICSMessageSubTypes.AMEND;
			}
			newMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			newMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			newMessage.EM_LinkedObject = AsycudaManifestHeader;
		}
	}
}
