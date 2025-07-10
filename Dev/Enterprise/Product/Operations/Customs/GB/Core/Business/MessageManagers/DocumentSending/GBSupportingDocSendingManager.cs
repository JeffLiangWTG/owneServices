using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;

namespace Enterprise.Customs.GB.Business.MessageManagers.DocumentSending
{
	public class GBSupportingDocSendingManager : SupportingDocSendingManager
	{
		public const string EHubID = "GBCustoms";
		public GBSupportingDocSendingManager(ISupportingDocSendingObjectParent declarationWrapper, IMessageNotificationCollector notification) : base(declarationWrapper, notification)
		{
		}

		protected override ZString GetDestination(ISupportingDocumentMessageDataProvider dataWrapper) => EHubID;
	}
}
