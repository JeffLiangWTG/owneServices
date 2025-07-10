using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Security;
using static CargoWise.EntityFramework.ZNotificationCollector;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class ECSExitHeaderMessageSendingObjectParent : BaseMessageSendingObjectParent<ECSExitHeaderMessageSendingObject>
	{
		public ECSExitHeaderMessageSendingObjectParent(ECSMessageSendingObject sendingObject) : base(sendingObject.ExitHeader.Factory)
		{
			this.sendingObject = sendingObject;
			ParentExitHeader = sendingObject.ExitHeader;
		}
		readonly ECSMessageSendingObject sendingObject;
		public CusExitControlHeader ParentExitHeader { get; }

		public ICertificateProvider CertificateData => sendingObject;

		public ZBool ShouldEditMessage => sendingObject.ShouldEditMessage;

		protected ECSExitHeaderMessageSendingObject CreateNewECSExitHeaderMessageSendingObject(CusExitDetail exitDetail)
			=> new ECSExitHeaderMessageSendingObject(exitDetail);

		protected override NonPersistentBusinessObjectCollection<ECSExitHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var result = new ECSExitHeaderMessageSendingObjectCollection<ECSExitHeaderMessageSendingObject>(Factory);
			foreach (CusExitDetail detail in ParentExitHeader.CusExitDetails)
			{
				result.Add(CreateNewECSExitHeaderMessageSendingObject(detail));
			}
			return result;
		}

		protected override IEnumerable<INotification> GetNewMessageErrorCollector()
		{
			return new CustomsNotificationCollector(ParentExitHeader, true, false, PropertyDescriptionType.HumanReadableName).GetMessageErrors();//ParentExitHeader, SendingObjectsCollection.Cast<ECSExitHeaderMessageSendingObject>().Where(x => x.ShouldSend).Select(x => x.ExitDetail)).GetMessageErrors();
		}

		public IEnumerable<ECSExitHeaderMessageSendingObject> ObjectsToSend
			=> SendingObjectsCollection.OfType<ECSExitHeaderMessageSendingObject>().Where(x => x.ShouldSend);

		public override BusinessObject TopLevelBusinessObject => ParentExitHeader;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(ECSExitHeaderMessageSendingObject.Schema.MRN), true, 140),
			new MessageSendingObjectProperty(nameof(ECSExitHeaderMessageSendingObject.Schema.ArrivalNotificationDate), true, 140),
			new MessageSendingObjectProperty(nameof(ECSExitHeaderMessageSendingObject.Schema.Status), true, 100)
		};
	}
}
