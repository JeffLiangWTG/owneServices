using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415MessageSendingObjectParent : BaseMessageSendingObjectParent<RF415MessageSendingObject>
	{
		public RF415MessageSendingObjectParent(AsycudaManifestHeader manifestHeader)
			: base(manifestHeader.Factory)
		{
			this.manifestHeader = Argument.NotNull(manifestHeader, nameof(manifestHeader));
		}

		readonly AsycudaManifestHeader manifestHeader;

		public override BusinessObject TopLevelBusinessObject => manifestHeader;

		public override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.EuH7;

		protected override NonPersistentBusinessObjectCollection<RF415MessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var result = new RF415MessageSendingObjectCollection(Factory);
			foreach (var bill in manifestHeader.Bills)
			{
				result.Add(new RF415MessageSendingObject(bill));
			}

			return result;
		}
	}
}
