using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.Messaging
{
	public class PBNInterchangeProvider : InterchangeProviderBase
	{
		public PBNInterchangeProvider(NonDependentEDIMessageCollection readyMessages) : base(readyMessages)
		{
		}

		protected override string InstructionHowToSetInterchangeSenderID => string.Empty;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.FirstOrDefault() is EDIMessage message)
			{
				interchange.EI_ApplicationCode = message.EM_ApplicationCode;
				interchange.EI_InterchangeType = message.EM_MessageType;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				interchange.EI_Priority = EDIInterchangePriorityList.Codes.High;
				interchange.EI_IsActive = true;
				interchange.EI_To = InterchangeCreator.GetToRecipient();
				interchange.EI_GB = message.EM_GB;
				var company = interchange.Company;
				interchange.EI_From = company.LicenceKeyIdentifier;
				interchange.EI_GP = message.EM_GP.IsEmpty ? company.GetCredentialPK() : message.EM_GP;
				interchange.EI_BodyText = message.EM_MessageText;
				interchange.EI_Status = EDIInterchange.Status.Queued;

				PopulateInterchangeHeader(message, interchange);

				interchange.ContainedMessages.Add(message);
				message.EM_Status = EDIMessage.Status.Sent;
			}
		}

		void PopulateInterchangeHeader(EDIMessage message, EDIInterchange interchange)
		{
			var endPointConfig = WebServiceEndPointProvider.GetSubmissionURL(message.Factory, message.EM_ApplicationCode, message.EM_MessageType);
			if (InterchangeCreator.HttpSchemesRegex.IsMatch(endPointConfig)
			)
			{
				string endpointUri;
				if (!message.EM_ApplicationReference.IsEmpty)
				{
					const string pbnIdPlaceHolder = "{pbnId}";
					endpointUri = endPointConfig.Replace(pbnIdPlaceHolder, message.EM_ApplicationReference);
				}
				else
				{
					endpointUri = endPointConfig;
				}

				interchange.SetHeaderTextWithAttributeDictionary(new Dictionary<string, string> {
					{ InterchangeCreator.CustomMsgAttributes.Endpoint, endpointUri },
					{ InterchangeCreator.CustomMsgAttributes.SigningOption, InterchangeCreator.CustomMsgAttributes.Rest }
				});
			}
			else
			{
				throw new DeveloperNotificationException(
					@$"Invalid end point configuration.
  {nameof(message.EM_ApplicationCode)} ""{message.EM_ApplicationCode}"",{nameof(message.EM_MessageType)} ""{message.EM_MessageType}"",config: ""{endPointConfig}"""
				);
			}
		}
	}
}
