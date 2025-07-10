using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.H7.Business
{
	public class UploadDocumentsSendingAction : MessageSendingObject
	{
		public UploadDocumentsSendingAction(AsycudaBill bill) : base(bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}

		readonly AsycudaBill bill;
		AdditionalInfoSendingObjectCollection addInfoCollection;

		public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(nameof(MovementReferenceNumber));

		public AdditionalInfoSendingObjectCollection AddInfoCollection
		{
			get
			{
				if (addInfoCollection == null)
				{
					addInfoCollection = CreateAddInfoCollection();
					addInfoCollection.LoadElements();
					RegisterEditableChildObject(addInfoCollection);
				}
				return addInfoCollection;
			}
		}

		[ResourceStringData("1B9A4579-390F-4724-BFF8-B48BD6876E64", Caption = "MRN", FullDescription = "Movement Reference Number")]
		public ZString MovementReferenceNumber => bill.MovementReferenceNumber;

		protected override bool Action_ReadOnly => true;

		protected virtual AdditionalInfoSendingObjectCollection CreateAddInfoCollection() => new AdditionalInfoSendingObjectCollection(bill, this);
	}
}
