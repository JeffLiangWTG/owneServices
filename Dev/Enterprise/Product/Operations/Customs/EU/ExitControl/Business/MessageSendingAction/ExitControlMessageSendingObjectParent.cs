using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class ExitControlMessageSendingObjectParent : BaseMessageSendingObjectParent<ExitControlMessageSendingObject>
	{
		public ExitControlMessageSendingObjectParent(CusExitHeader cusExitHeader) : base(cusExitHeader.Factory)
		{
			exitHeader = cusExitHeader;
			reports = GetReports();
		}

		protected virtual CusExitReport[] GetReports() => exitHeader.CusExitReports.Cast<CusExitReport>().ToArray();

		protected readonly CusExitHeader exitHeader;
		protected CusExitReport[] reports;

		public override BusinessObject TopLevelBusinessObject => exitHeader;

		protected override NonPersistentBusinessObjectCollection<ExitControlMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new ExitControlMessageSendingObjectCollection(reports, Factory);
		}

		protected override IEnumerable<INotification> GetNewMessageErrorCollector()
		{
			var reports = SelectedSendingObjects.Cast<ExitControlMessageSendingObject>().Select(x => x.MessagingObject);
			return new ExitControlMessageSendingNotificationCollector(exitHeader, reports).GetMessageErrors();
		}

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.Type), true, 80),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.TransportID), true, 80),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.Location), true, 200),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.DateTime), true, 200),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.MRN), true, 200),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.CustomsStatus), true, 80),
		};
	}
}
