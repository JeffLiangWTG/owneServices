using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;

namespace Enterprise.Messaging.Business
{
	public class XtMessageInfo : IXtMessageInfo
	{
		public XtMessageInfo(EDIInterchange interchange, ILogger logger)
		{
			this.interchange = interchange;
			this.logger = logger;
		}
		readonly EDIInterchange interchange;
		readonly ILogger logger;

		public string ApplicationCode => interchange.EI_ApplicationCode.ToString();
		public string MessageType => interchange.EI_InterchangeType.ToString();
		public string DestinationParty => interchange.EI_To.ToString();
		public string SourceParty => interchange.EI_From.ToString();
		public string MessageTrackingID => interchange.EI_SessionGUID.ToString();

		public BinaryReader GetMessageData()
		{
			if (interchange is IMessageDataProvider provider)
			{
				return provider.GetMessageData();
			}

			return new BinaryReader(interchange.GetEI_BodyTextReader().CopyAndDispose());
		}

		public Dictionary<string, string> XTMessageAttributes
		{
			get
			{
				var attributes = new Dictionary<string, string>();
				attributes.AddRangeToDictionaryIfValid(interchange.EI_HeaderText.ToString().GetHeaderTextDictionary(logger));

				try
				{
					if (interchange is IxTMessageAttributeProvider providerFromInterchange)
					{
						attributes.AddRangeToDictionaryIfValid(providerFromInterchange.GetMessageAttrDictionary());
					}
					if (interchange.ExternalPassword is IxTMessageAttributeProvider providerFromExternalPassword)
					{
						attributes.AddRangeToDictionaryIfValid(providerFromExternalPassword.GetMessageAttrDictionary());
					}

					return attributes;
				}
				catch (CryptographicException cEx)
				{
					var cExMsg = $"Interchange.MessageTrackingID={MessageTrackingID}) failed to attach certificate information to outgoing xT message: {cEx.Message}.";
					logger.Log(LogType.Error, cExMsg);
					var registration = ObjectFactory.Get<IProductRegistration>();
					if (!registration.IsWiseTechGlobalInternalSystem())
					{
						ErrorReporter.ReportOnce(cEx.Message, cExMsg, cEx);
					}
					throw new Exception(cExMsg);
				}
			}
		}
	}
}
