using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.EdifactResponse;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public static class NCTSEdifactProcessorHelper
	{
		const string XsdSchemaNameEdifactResponseError = "CargoWise.Customs.ES.MessageDefinitions.EdifactResponse.EdifactResponseMessage.xsd";

		public static ICUSRESMessageProvider ProcessEdifactErrorResponse(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var errorResponse = ESXmlObjectSerializer.DeserializeWithValidation<Response>(XsdSchemaNameEdifactResponseError, textReader);

				return new NCTSEdifactErrorMessageHelper(message.Factory, errorResponse.DeclarationStatus, errorResponse.ErrorStatus);
			}
		}
	}
}
