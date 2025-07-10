using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.Business
{
	[CodeAlive("Unused class as using EU.ExitControl.GUI.PlugIn.ExitControlPlugIn instead of EU.GUI.PlugIn.ExitSummaryPlugIn")]
	public class ExitNotificationMessageSendingActionParent : MessageSendingActionParent
	{
		public ExitNotificationMessageSendingActionParent(CusExitControlHeader exitHeader) : base(exitHeader, exitHeader.CusExitDetails, x => ((CusExitDetail)x).CED_MovementReferenceNumber, Env.Security.CustomsDeclarationSendWithMessageErrors)
		{
		}

		CusExitControlHeader ExitHeader => (CusExitControlHeader)TopLevelBusinessObject;

		protected override NonPersistentBusinessObjectCollection<MessageSendingAction> GetSendingObjectsCollectionCore()
		{
			var result = new ExitNotificationMessageSendingActionCollection(ExitHeader);
			result.PopulateElements();
			return result;
		}

		public new ExitNotificationMessageSendingActionCollection SendingObjectsCollection => (ExitNotificationMessageSendingActionCollection)base.SendingObjectsCollection;

		protected override bool OnlyOneObjectAllowedToBeSent => false;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(MessageSendingAction.Details), true, 119, Res.GetData("65D1FC0B-B142-4B01-A691-C66208347FFA", "MRN")),
			new MessageSendingObjectProperty(nameof(ExitNotificationMessageSendingAction.ReferenceNumber), true, 114, Res.GetData("115ADD3A-FD4E-41B9-8B79-9922C8C71BED", "Reference Number")),
			new MessageSendingObjectProperty(nameof(ExitNotificationMessageSendingAction.Status), true, 53, Res.GetData("44926AEC-8210-4DF3-BBC3-C4D1A2232F2D", "Status")),
			new MessageSendingObjectProperty(nameof(ExitNotificationMessageSendingAction.StatusDescription), true, 140, Res.GetData("6D0A5F2D-C52F-431C-88BD-A2C1DD2F497B", "Status Description")),
			new MessageSendingObjectProperty(nameof(ExitNotificationMessageSendingAction.MessageType), true, 92, Res.GetData("665B351A-4A36-4E3E-B216-A65D40A01FC7", "Message Type"))
		};
	}
}
