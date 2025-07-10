using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415DocumentSendingObjectCollection : NonPersistentBusinessObjectCollection<RF415DocumentSendingObject>
	{
		public RF415DocumentSendingObjectCollection(RF415MessageSendingObject messageSendingObject)
		{
			MessageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		}

		public RF415MessageSendingObject MessageSendingObject { get; }

		protected override bool AllowNewCore => MessageSendingObject.ShouldSend;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new RF415DocumentSendingObject(MessageSendingObject);

		protected override bool ElementCanBeAdded(BusinessObject bizO)
		{
			var newBizo = bizO as RF415DocumentSendingObject;

			return newBizo?.MessageSendingObject.PK == MessageSendingObject.PK;
		}
	}
}
