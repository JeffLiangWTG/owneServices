using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.BR.Business
{
	public class SubscriptionMessageSendingObjectParent : BaseMessageSendingObjectParent<SubscriptionMessageSendingObject>, IMessageSendingObjectParent
	{
		public SubscriptionMessageSendingObjectParent(BRGlbStaffWrapper staffWrapper) : base(staffWrapper.Factory)
		{
			this.staffWrapper = Argument.NotNull(staffWrapper, nameof(staffWrapper));
		}

		readonly BRGlbStaffWrapper staffWrapper;

		public override BusinessObject TopLevelBusinessObject => staffWrapper;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<SubscriptionMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var collection = new SubscriptionMessageSendingObjectCollection(Factory);

			foreach (GlbExternalPassword_BRS subscription in staffWrapper.EventSubscriptions)
			{
				collection.Add(new SubscriptionMessageSendingObject(subscription));
			}
			return collection;
		}
	}
}
