using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class MessageSendingActionParent : EU.NCTS.Business.NctsHeaderMessageSendingObjectParent
	{
		public MessageSendingActionParent(EU.NCTS.Business.NctsHeader nctsHeader)
			: base(nctsHeader)
		{
			this.nctsHeader = (NctsHeader)Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		readonly NctsHeader nctsHeader;

		public override BusinessObject TopLevelBusinessObject => nctsHeader;

		protected override NonPersistentBusinessObjectCollection<EU.NCTS.Business.NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore() => new MessageSendingActionCollection(nctsHeader);

		public new MessageSendingActionCollection SendingObjectsCollection => (MessageSendingActionCollection)base.SendingObjectsCollection;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;
		
		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => new List<MessageSendingObjectProperty>
		{
			new MessageSendingObjectProperty(MessageSendingAction.Schema.IsTestDeclaration, true, 40),
			new MessageSendingObjectProperty(MessageSendingAction.Schema.AdditionalDeclarationType, true, 100),
			new MessageSendingObjectProperty(EU.NCTS.Business.AutoNctsHeaderMessageSendingObject.Schema.LRN, true, 160),
			new MessageSendingObjectProperty(EU.NCTS.Business.AutoNctsHeaderMessageSendingObject.Schema.MRN, true, 160),
			new MessageSendingObjectProperty(MessageSendingAction.Schema.EntryType, true, 100),
			new MessageSendingObjectProperty(MessageSendingAction.Schema.EntryStatus, true, 100)
		};

		protected override void HookMessageSendingObjectEvents(BaseMessageSendingObject bo)
		{
			base.HookMessageSendingObjectEvents(bo);
			if (bo is MessageSendingAction action)
			{
				action.EntryTypeInfo.ValueChanged += (sender, e) => ResetValidationMessages();
			}
		}

		public bool ShowValidationErrors => SendingObjectsCollection.Cast<MessageSendingAction>().Any(x => x.ShowValidationErrors);

		protected override ZString GetBizObjValidationMessageErrors() => ShowValidationErrors ? base.GetBizObjValidationMessageErrors() : ZString.Empty;
	}
}
