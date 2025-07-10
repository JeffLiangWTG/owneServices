using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSFSTProvider : IUnderCustomsControl
	{
		public CUSFSTProvider(SCFSTF message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly SCFSTF message;

		public string MessageIdentifier => message.MetaData.MessageIdentifier;

		public ZDate ArrivalDate => message.Header.ArrivalDateSpecified ? new ZDate(message.Header.ArrivalDate.Date) : ZDate.Empty;

		public ZDate PresentationDate => message.Header.PresentationDateSpecified ? new ZDate(message.Header.PresentationDate.Date) : ZDate.Empty;

		public ZString ReferenceNumber => message.Header.ReferenceNumber;

		public ZString LocalReferenceNumber => message.Header.LRN;

		public string MRN => message.Header.MRN;

		public ZString PreviousReferenceType => message.PreviousAdministrativeReferences?.TypeSpecified ?? false ? message.PreviousAdministrativeReferences?.Type.MapXmlEnumToString() : ZString.Empty;

		public ZString PreviousReferenceNumber => message.PreviousAdministrativeReferences?.PreviousAdministrativeReference?.ReferenceNumber;

		public ZString CustomsOfficeReferenceNumber => message.MetaData.InterchangeSender.Identification.ReferenceNumber;

		public ZString ReferencedMessageIdentifier => message.Header.ReferencedMessageIdentifier;

		public IReadOnlyCollection<IUnderCustomsControlGoodsItem> GoodsItems => goodsItems ?? (goodsItems = message.Body.Select(x => new CUSFSTGoodsItemProvider(x)).ToArray());
		IReadOnlyCollection<IUnderCustomsControlGoodsItem> goodsItems;
	}
}
