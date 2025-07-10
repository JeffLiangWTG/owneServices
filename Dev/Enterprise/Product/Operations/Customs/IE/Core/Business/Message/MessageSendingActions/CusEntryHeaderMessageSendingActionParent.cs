using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Environment;

namespace Enterprise.Customs.IE.Business
{
	public abstract class CusEntryHeaderMessageSendingActionParent<TSendingAction> : BaseMessageSendingObjectParent<TSendingAction> where TSendingAction : CusEntryHeaderMessageSendingAction
	{
		protected CusEntryHeaderMessageSendingActionParent(JobDeclaration declaration) : base(declaration.Factory)
		{
			JobDeclaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public JobDeclaration JobDeclaration { get; }

		public sealed override BusinessObject TopLevelBusinessObject => JobDeclaration;

		public sealed override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		public bool IsNeedConfirm => SendingObjectsCollection.Cast<CusEntryHeaderMessageSendingAction>()
			.Any(action => action.ShouldSend && action.MessageStatus == LogicalStatusList.Codes.Sent);

		public bool DuplicationPossible => SendingObjectsCollection.Cast<CusEntryHeaderMessageSendingAction>()
			.Any(action => action.ShouldSend && action.EntryHeader.DuplicationPossible && IsCustomsDeclarationOrExportOriginal(action));

		bool IsCustomsDeclarationOrExportOriginal(CusEntryHeaderMessageSendingAction action)
		{
			if ((action.EntryHeader.IsImport &&
				 action.MessageType.EqualsIgnoringCase(AISOutgoingMessageTypeList.Codes.CustomsDeclaration)) ||
				(action.EntryHeader.IsExport &&
				 action.MessageType.EqualsIgnoringCase(AESOutgoingMessageTypeList.Codes.ExportOriginal)))
			{
				return true;
			}

			return false;
		}

		protected override NonPersistentBusinessObjectCollection<TSendingAction> GetSendingObjectsCollectionCore()
		{
			var result = (CusEntryHeaderMessageSendingActionCollection<TSendingAction>)Activator.CreateInstance(SendingObjectCollectionType, this);
			result.PopulateElements();
			return result;
		}

		protected abstract Type SendingObjectCollectionType { get; }

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columns;

		readonly IEnumerable<MessageSendingObjectProperty> columns = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(BaseMessageSendingObject.SchemaShouldSend, ismandatory: true, columnWidth: 80),
			new MessageSendingObjectProperty(nameof(CusEntryHeaderMessageSendingAction.SubStyle), ismandatory: true, columnWidth: 100),
			new MessageSendingObjectProperty(nameof(CusEntryHeaderMessageSendingAction.Description), ismandatory: true, columnWidth: 150),
			new MessageSendingObjectProperty(nameof(CusEntryHeaderMessageSendingAction.LocalReferenceNumber), ismandatory: true, columnWidth: 200),
			new MessageSendingObjectProperty(nameof(CusEntryHeaderMessageSendingAction.MessageStatus), ismandatory: true, columnWidth: 150),
			new MessageSendingObjectProperty(nameof(CusEntryHeaderMessageSendingAction.EntryStatus), ismandatory: true, columnWidth: 100),
			new MessageSendingObjectProperty(nameof(CusEntryHeaderMessageSendingAction.DeclarationType), ismandatory: true, columnWidth: 80),
			new MessageSendingObjectProperty(nameof(CusEntryHeaderMessageSendingAction.MessageTypeForDisplay), ismandatory: true, columnWidth: 80),
			new MessageSendingObjectProperty(nameof(CusEntryHeaderMessageSendingAction.MessageTypeDescription), ismandatory: true, columnWidth: 150),
		};
	}
}
