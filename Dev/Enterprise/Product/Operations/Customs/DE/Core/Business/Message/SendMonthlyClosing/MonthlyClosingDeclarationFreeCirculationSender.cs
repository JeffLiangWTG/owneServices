using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	public class MonthlyClosingDeclarationFreeCirculationSender : MonthlyClosingDeclarationSender
	{
		public MonthlyClosingDeclarationFreeCirculationSender(CusReconDeclaration declaration, string messageRole)
			: base(declaration, MonthlyClosingMessageBuilderLoader.MonthlyClosingFreeCirculation, new CFCPEDMessageHeaderProvider(declaration, messageRole), messageRole)
		{
		}

		public override bool MessageHasInformationToSend => ((ICFCPEDHeader)messageHeaderProvider.Header).Bodies.Any();

		protected override IEnumerable<int> LineNumbersInMessage10_1 => ((Messaging.ATLASVersion10_1.MonthlyClosingMessageBuilder<CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.ECFCPF>)MessageBuilder).LineNumbersInMessage;
	}
}
