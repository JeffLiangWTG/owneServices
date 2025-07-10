using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.TTCE;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCOptionalTreatmentAttributesResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCOptionalTreatmentAttributesResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("A55B53BC-7462-48F8-9A14-7F4B36C3550D", "Optional Treatment Attributes Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.RTT };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is JobDeclaration declaration)
			{
				var ttce = BRMessageHelper.DeserializeObject<RespostaObterTratamentosTributariosImportacaoDTO>(message.EM_MessageText);
				var ncm = ttce?.ncm;
				if (!string.IsNullOrEmpty(ncm) && ttce.codigoPais > 0)
				{
					var countryCode = BRRefCusMapper.MapCustomsCodeCountryToCW1Code(declaration.Factory, ttce.codigoPais.ToString());
					if (!countryCode.IsEmpty)
					{
						var applicationReference = $"{ncm}|{countryCode}|{ttce.dataFatoGerador.Replace("-", string.Empty)}";
						message.EM_ApplicationReference = applicationReference;
					}
					else
					{
						message.EM_Status = EDIMessage.Status.Failed;
						Logger.LogError($"Message #{message.EM_MessageNum}: Code '{ttce.codigoPais}' has not Country mapped.");
					}
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Failed;
					Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
				}
			}
		}
	}
}
