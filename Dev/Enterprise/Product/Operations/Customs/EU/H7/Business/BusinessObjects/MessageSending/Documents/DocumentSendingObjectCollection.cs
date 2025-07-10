using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.H7.Business
{
	public class DocumentSendingObjectCollection : NonPersistentBusinessObjectCollection<DocumentSendingObject>
	{
		public DocumentSendingObjectCollection(AsycudaBill bill, BusinessObject parentSendingObject) : base(bill?.Factory)
		{
			this.parentSendingObject = parentSendingObject;
		}

		readonly BusinessObject parentSendingObject;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new DocumentSendingObject((AdditionalInfoSendingObject)parentSendingObject);
	}
}
