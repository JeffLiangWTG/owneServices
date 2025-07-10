using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.MessageManagers.DocumentSending;

namespace Enterprise.Customs.GB.CDS.Messaging.DocumentSending
{
	public class CDSSupportingDocSendingManager : GBSupportingDocSendingManager
	{
		readonly JobDeclarationSupportingDocSendingObjectParent declarationWrapper;

		public CDSSupportingDocSendingManager(JobDeclarationSupportingDocSendingObjectParent declarationWrapper, IMessageNotificationCollector notification)
			: base(declarationWrapper, notification)
		{
			this.declarationWrapper = Argument.NotNull(declarationWrapper, "declarationWrapper");
		}

		protected override void CollectNotificationsFromSendingObjects(MessageSendingNotificationCollection notifications)
		{
			base.CollectNotificationsFromSendingObjects(notifications);
			var checker = new CdsGlbExternalPasswordCheckerWithNotifications((JobDeclaration)declarationWrapper.ParentDeclaration, notifications);
			_ = checker.PasswordExistsAndOkToSendToCds;
		}
	}
}
