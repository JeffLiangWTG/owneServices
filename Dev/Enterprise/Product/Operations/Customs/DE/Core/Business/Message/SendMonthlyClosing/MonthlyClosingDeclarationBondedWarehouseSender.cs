using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	public sealed class MonthlyClosingDeclarationBondedWarehouseSender : MonthlyClosingDeclarationSender
	{
		public MonthlyClosingDeclarationBondedWarehouseSender(CusReconDeclaration declaration, string messageRole)
			: base(declaration, MonthlyClosingMessageBuilderLoader.MonthlyClosingBondedWarehouse, new SCWPEDMessageHeaderProvider(declaration, messageRole), messageRole)
		{
		}

		public override bool MessageHasInformationToSend => ((ISCWPEDHeader)messageHeaderProvider.Header).Bodies.Any();

		protected override IEnumerable<int> LineNumbersInMessage10_1 => ((Messaging.ATLASVersion10_1.MonthlyClosingMessageBuilder<CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LSCWPM>)MessageBuilder).LineNumbersInMessage;
	}
}
