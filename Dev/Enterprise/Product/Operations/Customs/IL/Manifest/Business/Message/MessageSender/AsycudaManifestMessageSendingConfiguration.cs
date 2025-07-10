using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestMessageSendingConfiguration
	{
		public BaseMessageSendingObjectParent GetNewMessageSendingObjectParent(AsycudaManifestHeader header) => GetNewMessageSendingObjectParentCore(header);

		public AsycudaManifestQueryMessageSendingObjectParent GetNewQueryMessageSendingObjectParent(AsycudaManifestHeader header) => new AsycudaManifestQueryMessageSendingObjectParent(header);

		protected virtual BaseMessageSendingObjectParent GetNewMessageSendingObjectParentCore(AsycudaManifestHeader header) => new AsycudaManifestMessageSendingObjectParent(header);
	}
}
