using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<AsycudaManifestMessageSendingObject>
	{
		public AsycudaManifestMessageSendingObjectCollection(AsycudaManifestHeader header) : base(header.Factory)
		{
			this.header = header;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new AsycudaManifestMessageSendingObject(header);

		protected override bool AllowNewCore => false;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newSendingObject = (AsycudaManifestMessageSendingObject)child;

			newSendingObject.ShouldSend = true;
			newSendingObject.MessageType = ILMessageTypeList.Codes.MAN;
			newSendingObject.MessageSubType = ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest;
		}

		readonly AsycudaManifestHeader header;
	}
}
