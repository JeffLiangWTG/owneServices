using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class DocumentSendingObjectCollection : NonPersistentBusinessObjectCollection<DocumentSendingObject>
	{
		public DocumentSendingObjectCollection(CusEntryHeader entryHeader, BusinessObject parentSendingObject) : base(entryHeader?.Factory)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.parentSendingObject =  parentSendingObject;
		}

		readonly CusEntryHeader entryHeader;
		readonly BusinessObject parentSendingObject;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new DocumentSendingObject(entryHeader, parentSendingObject);

		protected override void OnAdded(BusinessObject bizO)
		{
			base.OnAdded(bizO);
			if (bizO is DocumentSendingObject documentSendingObject && parentSendingObject is AdditionalInfoSendingObject additionalInfoSendingObject && additionalInfoSendingObject.Action != null)
			{
				additionalInfoSendingObject.Action.ShouldSendInfo.ValueChanged += documentSendingObject.ValidateAllAndRefreshBinding;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (bizO is DocumentSendingObject documentSendingObject && parentSendingObject is AdditionalInfoSendingObject additionalInfoSendingObject && additionalInfoSendingObject.Action != null)
			{
				additionalInfoSendingObject.Action.ShouldSendInfo.ValueChanged -= documentSendingObject.ValidateAllAndRefreshBinding;
			}
		}
	}
}
