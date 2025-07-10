using System.Collections;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415DocumentSendingObject : AutoRF415DocumentSendingObject
	{
		public RF415DocumentSendingObject(RF415MessageSendingObject messageSendingObject)
			: base(messageSendingObject.Bill.Factory)
		{
			MessageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		}

		public RF415MessageSendingObject MessageSendingObject { get; }

		[List(nameof(DocumentTypeList))]
		public override ZString DocumentType
		{
			get => base.DocumentType;
			set => base.DocumentType = value;
		}

		ZString DataGrouping => MessageSendingObject.DataGrouping;

		public ICollection DocumentTypeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(MessageSendingObject.Bill.Factory, DataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Today);
	}
}
