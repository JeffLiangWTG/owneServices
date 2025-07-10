using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.ProductCatalog;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogLinkMessageSendingObject : IMessageSendingObject
	{
		public GoodsCatalogLinkMessageSendingObject(GoodsCatalogMessageSendingObject parent, IEnumerable<ForeignOperator> foreignOperators)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
			ForeignOperators = Argument.NotNull(foreignOperators, nameof(foreignOperators));
			MessageType = EDIMessageSubTypeList.Codes.Link;
		}

		public GoodsCatalogMessageSendingObject Parent;

		public IEnumerable<ForeignOperator> ForeignOperators;

		public ZString MessageType { get; set; }

		public BusinessObjectFactory Factory => Parent.Factory;

		public BusinessObject MessageAttachee => Parent.MessageAttachee;

		public ZString GetApplicationReference() => Parent.GetApplicationReference();

		public ZGuid GetGlbExternalPasswordPK() => Parent.GetGlbExternalPasswordPK();

		public ZString GetMessageOwner() => Parent.GetMessageOwner();

		public ZString GetMessageText() => new ManufacturersToProductMessageBuilder(ManufacturersToProductsProvider.New(ForeignOperators))?.GetMessageText();

		public ZString GetMessageTypeForEDIMessage() => MessageTypeList.Codes.CAT;
	}
}
