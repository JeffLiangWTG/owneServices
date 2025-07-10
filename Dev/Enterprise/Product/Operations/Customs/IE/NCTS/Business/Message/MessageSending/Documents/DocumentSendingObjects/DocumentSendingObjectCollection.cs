using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DocumentSendingObjectCollection : NonPersistentBusinessObjectCollection<DocumentSendingObject>
	{
		public DocumentSendingObjectCollection(ISupportingDocObject nctsHeader) : base(nctsHeader.Factory)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		readonly ISupportingDocObject nctsHeader;

		protected override BusinessObject CreateNonPersistentBusinessObject() => DocumentSendingObject.New(nctsHeader);
	}
}
