using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.MessageBuilders.TTCE.Outgoing;
using CargoWise.Customs.BR.MessageDefinitions.TTCE;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class ImportTaxTreatmentsOptionalMessageSender
	{
		public ImportTaxTreatmentsOptionalMessageSender(JobDeclaration declaration, RespostaObterTratamentosTributariosImportacaoDTO response)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			this.response = Argument.NotNull(response, nameof(response));
		}

		protected readonly JobDeclaration declaration;
		protected readonly RespostaObterTratamentosTributariosImportacaoDTO response;

		public void SendMessage()
		{
			if (response.fundamentosOpcionaisDisponiveis?.Count > 0 )
			{
				var message = declaration.Factory.New<BREDIMessage>();
				message.EM_MessageType = MessageTypeList.Codes.RTT;
				message.EM_MessageSubType = EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes;
				message.EM_LinkedObject = declaration;
				message.EM_GP = declaration.BrokerCertificate?.PK ?? ZGuid.Empty;
				message.EM_MessageText = new ImportTaxTreatmentsMessageBuilder(new ImportTaxTreatmentsOptionalProvider(response)).GetMessageText();
			}
		}
	}
}

