using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSFINProvider : ICUSFIN
	{
		public CUSFINProvider(SCFING message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly SCFING message;

		public string MessageIdentifier => message.MetaData.MessageIdentifier;

		public ZString CustodianReferenceNumber => message.Custodian?.Identification?.ReferenceNumber;

		public ZString CustodianSubsidiaryNumber => message.Custodian?.Identification?.SubsidiaryNumber;

		public ZString InterchangeRecipientReferenceNumber => message.MetaData.InterchangeRecipient.Identification.ReferenceNumber;

		public ZString InterchangeRecipientSubsidiaryNumber => message.MetaData.InterchangeRecipient.Identification.SubsidiaryNumber;

		public ZString AdditionalRegistrationNumber => message.SummaryDeclaration.AdditionalRegistrationNumber;

		public ZString AdditionalReferenceNumber => message.SummaryDeclaration.AdditionalReferenceNumber;

		public string MRN => message.SummaryDeclaration.MRN;

		public ZString CompletionType => message.SummaryDeclaration.CompletionType;

		public IReadOnlyCollection<ICUSFINGoodsItem> GoodsItems => goodsItems ?? (goodsItems = message.SummaryDeclaration.GoodsItem.Select(x => new CUSFINGoodsItemProvider(x)).ToArray());
		IReadOnlyCollection<ICUSFINGoodsItem> goodsItems;
	}
}
