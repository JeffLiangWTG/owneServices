using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AdditionalInfoSendingObjectCollection : NonPersistentBusinessObjectCollection<AdditionalInfoSendingObject>
	{
		public AdditionalInfoSendingObjectCollection(AsycudaBill bill, UploadDocumentsSendingAction action)
		{
			Bill = Argument.NotNull(bill, nameof(bill));
			Action = Argument.NotNull(action, nameof(action));
			MaxCountValidationEnable(99);
		}

		AsycudaBill Bill { get; }
		UploadDocumentsSendingAction Action { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject() => CreateAdditionalInfoSendingObject(null);

		public void LoadElements()
		{
			RemoveAndDeleteAll();
			foreach (var requested in Bill.RequestedDocuments.Where(req => req.IsOpen))
			{
				var sendingObject = CreateAdditionalInfoSendingObject(requested);
				Add(sendingObject);
			}
		}

		protected virtual AdditionalInfoSendingObject CreateAdditionalInfoSendingObject(RequestedDocument document) => new AdditionalInfoSendingObject(Bill, Action, document);
	}
}
