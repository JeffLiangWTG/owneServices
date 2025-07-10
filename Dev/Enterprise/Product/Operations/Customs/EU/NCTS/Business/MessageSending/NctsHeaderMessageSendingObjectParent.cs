using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Environment;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderMessageSendingObjectParent : BaseMessageSendingObjectParent<NctsHeaderMessageSendingObject>
	{
		public NctsHeaderMessageSendingObjectParent(NctsHeader nctsHeader) : base(nctsHeader?.Factory)
		{
			NctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		protected NctsHeader NctsHeader { get; }

		public override BusinessObject TopLevelBusinessObject => NctsHeader;

		public override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.EuNctsSendWithMessageErrors;

		public new MessageSendingValidation MessageSendingValidation => (MessageSendingValidation)base.MessageSendingValidation;

		protected override NonPersistentBusinessObjectCollection<NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var sendingObjectCollection = new NctsHeaderMessageSendingObjectCollection(Factory);
			sendingObjectCollection.Add(new NctsHeaderMessageSendingObject(NctsHeader));
			return sendingObjectCollection;
		}

		public bool SendAndSaveMessages() => SendAndSaveMessagesCore();

		protected virtual bool SendAndSaveMessagesCore() => false;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
		{
			get
			{
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.LRN, true, 160);
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.MRN, true, 160);
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.MessageType, true, 100);
				if (SupportReleaseRequest)
				{
					yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.ReleaseRequest, true, 100);
				}
				if (SupportJustification)
				{
					yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.Justification, true, 100);
				}
			}
		}

		protected override Customs.Business.MessageSendingValidation GetNewMessageSendingValidation()
			=> MessageSendingValidation.New(TopLevelBusinessObject, GetNewMessageErrorCollector(), NotificationCollector.GetWarnings(), SecurityCheckpointToSendWithMessageError ?? Env.Security.CustomsDeclarationSendWithMessageErrors);

		protected override IEnumerable<INotification> GetNewMessageErrorCollector()
			=> new SelectiveMessageErrorCollector(TopLevelBusinessObject, SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>().Select(x => x.NctsHeader)).GetMessageErrors();

		protected override ZString GetAdditionalWarningsCore()
		{
			var result = string.Empty;
			if (NctsHeader.Configuration.MessageSendingConfiguration.ShouldFillAdditionalWarningsOnSendScreen && SelectedSendingObjects.Any())
			{
				result = Regex.Replace(MessageSendingValidation.CheckBusinessObjectLevelWarning().NotificationsAsString(), "(?<!\r)\n", "\r\n");
			}
			return result;
		}

		public bool SupportReleaseRequest => SendingObjectsCollection.Cast<NctsHeaderMessageSendingObject>()
			.Any(x => x.Lookups.MessageTypeList.ContainsCode(x.ReleaseRequestCode));

		public bool SupportJustification => NctsHeader.Configuration.MessageSendingConfiguration.ShowJustification(NctsHeader) && SendingObjectsCollection.Cast<NctsHeaderMessageSendingObject>().Any(x => x.Lookups.MessageTypeList.ContainsCode(NCTS5DeparturePhaseList.Codes.Cancellation));
	}
}
