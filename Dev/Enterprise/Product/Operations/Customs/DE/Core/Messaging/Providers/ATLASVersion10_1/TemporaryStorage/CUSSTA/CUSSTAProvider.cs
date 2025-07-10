using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSSTAProvider : ICUSSTA
	{
		public CUSSTAProvider(SCSTAB message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly SCSTAB message;

		public string MessageIdentifier => message.MetaData.MessageIdentifier;

		public ZString CustodianReferenceNumber => message.Custodian?.Identification?.ReferenceNumber;

		public ZString CustodianSubsidiaryNumber => message.Custodian?.Identification?.SubsidiaryNumber;

		public ZString InterchangeRecipientReferenceNumber => message.MetaData.InterchangeRecipient.Identification.ReferenceNumber;

		public ZString InterchangeRecipientSubsidiaryNumber => message.MetaData.InterchangeRecipient.Identification.SubsidiaryNumber;

		public ZString AdditionalReferenceNumber => message.SummaryDeclaration.AdditionalReferenceNumber;

		public IReadOnlyCollection<ICUSSTAGoodsItem> GoodsItems => goodsItems ?? (goodsItems = message.SummaryDeclaration.GoodsItem.Select(x => new CUSSTAGoodsItemProvider(x)).ToArray());
		IReadOnlyCollection<ICUSSTAGoodsItem> goodsItems;
	}
}
