using System.Collections.Generic;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class CustomsServiceErrorUniversalEventResponseMessageProcessor : ESCommonResponseMessageProcessor<BusinessObject, UniversalEventWrapper>
	{
		public CustomsServiceErrorUniversalEventResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Customs Service Error UniversalEvent Declaration Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent, Messaging.DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest };

		protected override void ProcessMessageCore(EDIMessage message, BusinessObject linkedBusinessObject, UniversalEventWrapper provider)
		{
			message.EM_MessageInterpretation = new CustomsServiceErrorUniversalEventMessagePrettyFormatter(provider).CreateMessageDetailsRejected();

			SetCHStatusAsFailed(linkedBusinessObject);
			SetMessageStatusAsFailed(message);
		}

		protected override UniversalEventWrapper GetMessageProviderCore(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				var bodyText = bodyTextReader.ReadToEnd();
				if (bodyText.Contains(UniversalEventTag))
				{
					return new UniversalEventWrapper(bodyText);
				}
				else
				{
					throw new XmlSchemaValidationException(Res.GetString("32BF0081-34E2-4EAD-B3A0-BA8D0EC673F0", "There is an error in XML document (1, 1)."));
				}
			}
		}

		const string UniversalEventTag = "UniversalEvent";
	}
}
