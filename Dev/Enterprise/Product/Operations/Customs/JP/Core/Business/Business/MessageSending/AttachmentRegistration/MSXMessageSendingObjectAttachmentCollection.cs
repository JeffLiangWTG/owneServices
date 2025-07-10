using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Business
{
	public class MSXMessageSendingObjectAttachmentCollection : NonPersistentBusinessObjectCollection<MSXMessageSendingObjectAttachment>
	{
		public MSXMessageSendingObjectAttachmentCollection(MSXMessageSendingObject msxMessageSendingObject)
		{
			Parent = Argument.NotNull(msxMessageSendingObject, nameof(msxMessageSendingObject));
			MaxCountValidationEnable(MaxRowCount, Res.GetString("749371BA-DEBF-41CC-9F22-BE3C35923EEE", "You are not allowed to attach more than 10 documents to one single message"));
		}

		public static int MaxRowCount => 10;

		public MSXMessageSendingObject Parent { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject() => new MSXMessageSendingObjectAttachment(Parent);

		protected override bool AllowNewCore => base.AllowNewCore && Count < MaxRowCount;
	}
}
