using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.Messaging
{
	public class CustomsAndExciseReportInterchangeProvider : InterchangeProviderBase
	{
		public CustomsAndExciseReportInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override string InstructionHowToSetInterchangeSenderID => string.Empty;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var isInterchangePopulated = false;
			foreach (var msg in messages)
			{
				if (msg is EDIMessage edimsg)
				{
					if (!isInterchangePopulated)
					{
						PopulateInterchange(edimsg, interchange);
						isInterchangePopulated = true;
					}
					edimsg.EM_Status = EDIMessage.Status.Sent;
					interchange.ContainedMessages.Add(msg);
				}
			}
		}

		void PopulateInterchange(EDIMessage message, EDIInterchange interchange)
		{
			interchange.EI_ApplicationCode = message.EM_ApplicationCode;
			interchange.EI_InterchangeType = message.EM_MessageType;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_Priority = EDIInterchangePriorityList.Codes.High;
			interchange.EI_To = InterchangeCreator.GetToRecipient();
			interchange.EI_GB = message.EM_GB;
			interchange.EI_From = interchange.Company.LicenceKeyIdentifier;
			interchange.EI_GP = message.EM_GP.IsEmpty ? interchange.Company.GetCredentialPK() : message.EM_GP;

			var endPoint = GetEndPoint(message);
			var webServiceEndPoint = GetWebServiceEndPoint(endPoint);
			interchange.SetHeaderTextWithAttributeDictionary(new Dictionary<string, string>
			{
				{ InterchangeCreator.CustomMsgAttributes.Endpoint, GetWebServiceEndPoint(GetEndPoint(message)) },
				{ InterchangeCreator.CustomMsgAttributes.SigningOption, InterchangeCreator.CustomMsgAttributes.Rest }
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "End Point")]
		public static string GetEndPoint(EDIMessage message)
		{
			var dateString = ((ICustomsAndExciseReportOutboundMessage)message).MessageDate.ToString(Constants.DateTimeFormat.ShortDateTime);
			return (string)message.EM_MessageType switch
			{
				CustomsAndExciseReportTypeList.Codes.PSR => $"/transactions/periods/{dateString}/payer-summary-report",
				CustomsAndExciseReportTypeList.Codes.PCT => $"/transactions/periods/{dateString}/payer-combined-taxes-report",
				CustomsAndExciseReportTypeList.Codes.PTT => $"/transactions/periods/{dateString}/payer-tax-types-report",
				CustomsAndExciseReportTypeList.Codes.PCI => $"/transactions/periods/{dateString}/importer-combined-taxes-report",
				CustomsAndExciseReportTypeList.Codes.DSR => $"/transactions/daily/{dateString}/payer-summary-report",
				CustomsAndExciseReportTypeList.Codes.DCT => $"/transactions/daily/{dateString}/payer-combined-taxes-report",
				CustomsAndExciseReportTypeList.Codes.DTT => $"/transactions/daily/{dateString}/payer-tax-types-report",
				CustomsAndExciseReportTypeList.Codes.UDR => "/transactions/payer-unpaids-report",
				CustomsAndExciseReportTypeList.Codes.BAL => "/transactions/balance",
				_ => dateString,
			};
		}

		public static string GetWebServiceEndPoint(string endPoint)
		{
			return EnvProxy.Instance.IsProductionSystem ? $"https://www.ros.ie/customs/webservice/v1/rest{endPoint}" : $"https://softwaretestnextversion.ros.ie/customs/webservice/v1/rest{endPoint}";
		}
	}
}
