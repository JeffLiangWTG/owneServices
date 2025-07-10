using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class MessageSendingActionParent : BaseMessageSendingObjectParent<MessageSendingAction>
	{
		public MessageSendingActionParent(BusinessObject topBusinessObject, IEnumerable<BusinessObject> messagingEntities, Func<BusinessObject, ZString> getDetails, SecurityCheckpoint securityCheckpointToSendWithMessageError) : base(topBusinessObject.Factory)
		{
			this.topBusinessObject = topBusinessObject;
			this.messagingEntities = messagingEntities;
			this.getDetails = getDetails;
			this.securityCheckpointToSendWithMessageError = securityCheckpointToSendWithMessageError;
		}

		readonly BusinessObject topBusinessObject;
		readonly IEnumerable<BusinessObject> messagingEntities;
		readonly Func<BusinessObject, ZString> getDetails;
		readonly SecurityCheckpoint securityCheckpointToSendWithMessageError;

		public override BusinessObject TopLevelBusinessObject => topBusinessObject;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => securityCheckpointToSendWithMessageError;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => base.MessageSendingObjectProperties.Append
			(
				new MessageSendingObjectProperty((NoResString)"Details", true, 200, Res.GetData("2608A4CC-9A5E-4D2F-B07E-A9B6264FD502", "Details")) // MessageSendingObjectProperty Name
			);

		public new MessageSendingValidation MessageSendingValidation => (MessageSendingValidation)base.MessageSendingValidation;

		protected override NonPersistentBusinessObjectCollection<MessageSendingAction> GetSendingObjectsCollectionCore()
		{
			var coll = new MessageSendingActionCollection(messagingEntities, getDetails, Factory);
			coll.PopulateElements();
			return coll;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ClearRowNotifications();
			if (OnlyOneObjectAllowedToBeSent && SendingObjectsCollection.Cast<MessageSendingAction>().Count(x => x.ShouldSend) > 1)
			{
				AddRowError(Res.GetString("5A07EA94-3309-471E-97A3-E0507CFF395C", "Please select only one declaration to send."));
			}
		}

		protected virtual bool OnlyOneObjectAllowedToBeSent => true;

		protected override IEnumerable<INotification> GetNewMessageErrorCollector()
			=> new SelectiveMessageErrorCollector(topBusinessObject, SelectedSendingObjects.Cast<MessageSendingAction>().Select(x => x.MessagingObject)).GetMessageErrors();

		protected override Customs.Business.MessageSendingValidation GetNewMessageSendingValidation()
			=> MessageSendingValidation.New(TopLevelBusinessObject, GetNewMessageErrorCollector(), NotificationCollector.GetWarnings(), SecurityCheckpointToSendWithMessageError ?? Env.Security.CustomsDeclarationSendWithMessageErrors);

		protected override ZString GetAdditionalWarningsCore()
		{
			var result = string.Empty;
			if (SelectedSendingObjects.Any())
			{
				result = Regex.Replace(MessageSendingValidation.CheckBusinessObjectLevelWarning().NotificationsAsString(), "(?<!\r)\n", "\r\n");
			}
			return result;
		}
	}
}
