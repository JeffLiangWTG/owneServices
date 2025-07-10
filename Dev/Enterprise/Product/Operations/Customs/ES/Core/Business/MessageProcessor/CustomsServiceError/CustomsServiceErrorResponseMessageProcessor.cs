using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Xhub.Products.Customs;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class CustomsServiceErrorResponseMessageProcessor : ESCommonResponseMessageProcessor<BusinessObject, CommonCustomsServiceError>
	{
		public CustomsServiceErrorResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Customs Service Error Declaration Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.CustomsServiceError };

		const string XsdSchemaNameCommonCustomsServiceError = "CargoWise.Customs.ES.MessageDefinitions.CustomsServiceError.CommonCustomsServiceError.xsd";

		protected override void ProcessMessageCore(EDIMessage message, BusinessObject linkedBusinessObject, CommonCustomsServiceError provider)
		{
			message.EM_MessageInterpretation = new CustomsServiceErrorMessagePrettyFormatter(provider).CreateMessageDetailsRejected();

			SetCHStatusAsFailed(linkedBusinessObject);
			SetMessageStatusAsFailed(message);
		}

		protected override CommonCustomsServiceError GetMessageProviderCore(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				return ESXmlObjectSerializer.DeserializeWithValidation<CommonCustomsServiceError>(XsdSchemaNameCommonCustomsServiceError, textReader);
			}
		}
	}
}
