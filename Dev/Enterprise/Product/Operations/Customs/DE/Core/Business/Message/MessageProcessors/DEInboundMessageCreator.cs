using System;
using System.Globalization;
using System.Xml;
using CargoWise.Customs.DE.MessageDefinitions.ZHub;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DE.Business
{
	public abstract class DEInboundMessageCreator<TEDIMessage> : IInboundMessageCreator
		where TEDIMessage : EDIMessage
	{
		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var bodyText = interchange.EI_BodyText;
			if (bodyText.IsEmpty)
			{
				interchange.EI_Status = EDIInterchange.Status.Error;
				interchange.Logs.AddNew(Events.ErrorReport, "NO DE CUSTOMS DATA");
			}
			else
			{
				using (var textReader = interchange.GetEI_BodyTextReader())
				{
					var deCustomsData = XmlObjectSerializer.DeserializeWithXSDValidation<DECustomsData>(typeof(DECustomsData).GetEmbeddedResourcePath(), textReader);
					if (deCustomsData != null)
					{
						if (ZDateTimeOffset.TryParse(deCustomsData.LogbookTime, out var convertedLogbookTime, CultureInfo.InvariantCulture))
						{
							interchange.EI_DeliveredTime = convertedLogbookTime;
						}
						else
						{
							interchange.Logs.AddNew(Events.ErrorReport, FormattableString.Invariant($"LogbookTime Invalid. Interchange Number: {interchange.EI_InterchangeNum}, LogBookTime: {deCustomsData.LogbookTime}"));
						}
						if (deCustomsData.CustomsData != null)
						{
							var customsDataNode = ((XmlNode[])deCustomsData.CustomsData)[0];
							var applicationReference = customsDataNode.Name;
							var interchangeDataProvider = GetInterchangeDataProvider(interchange.Factory, customsDataNode, applicationReference);
							var message = interchange.Factory.New<TEDIMessage>();
							message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
							message.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);
							message.EM_ApplicationReference = applicationReference;
							message.EM_MessageType = interchangeDataProvider.MessageType;
							message.EM_MessageSubType = ((ZString)interchangeDataProvider.MessageSubType).SubstringSafe(0, EDIMessage.Schema.EM_MessageSubTypeMaxLength);
							message.EM_MessageText = bodyText;
							message.EM_Status = EDIMessage.Status.Queued;
							interchange.ContainedMessages.Add(message);
							message.SetLogbookEORIBranchSuffix(interchangeDataProvider.InterchangeRecipientEORIBranch);
							message.SetLogbookLocalReferenceNumber(interchangeDataProvider.LocalReferenceNumber);
						}
					}
				}
			}
		}

		protected abstract IInterchangeCustomsDataProvider GetInterchangeDataProvider(BusinessObjectFactory factory, XmlNode customsMessageRootNode, string applicationReference);
	}
}
