using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public class MonthlyClosingDeclarationInwardProcessingSender : MonthlyClosingDeclarationSender
	{
		public MonthlyClosingDeclarationInwardProcessingSender(CusReconDeclaration declaration, string messageRole)
			: base(declaration, MonthlyClosingMessageBuilderLoader.MonthlyClosingInwardProcessing, new SCIPEDMessageHeaderProvider(declaration, messageRole), messageRole)
		{
		}

		public override bool MessageHasInformationToSend => ((ISCIPEDHeader)messageHeaderProvider.Header).Bodies.Any();

		protected override IEnumerable<int> LineNumbersInMessage10_1 => ((Messaging.ATLASVersion10_1.MonthlyClosingMessageBuilder<CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.VSCIPK>)MessageBuilder).LineNumbersInMessage;
	}
}
