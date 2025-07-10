using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestQueryMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<AsycudaManifestQueryMessageSendingObject>
	{
		public AsycudaManifestQueryMessageSendingObjectCollection(AsycudaManifestHeader header) : base(header.Factory)
		{
			this.header = header;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new AsycudaManifestQueryMessageSendingObject(header);

		protected override bool AllowNewCore => false;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newSendingObject = (AsycudaManifestQueryMessageSendingObject)child;
			newSendingObject.ShouldSend = true;
			newSendingObject.MessageType = ILMessageTypeList.Codes.MAN;
			newSendingObject.MessageSubType = ILEDIMessageSubTypeList.Codes.ManifestQueryRequest;
			newSendingObject.ManifestNumber = header.AMA_ManifestNumber;
			newSendingObject.ParentDealNumber = header.Bills.Cast<AsycudaBill>()
				.FirstOrDefault(b => !b.ParentDealNumber.IsEmpty)
				?.ParentDealNumber ?? ZString.Empty;
		}
		readonly AsycudaManifestHeader header;
	}
}
