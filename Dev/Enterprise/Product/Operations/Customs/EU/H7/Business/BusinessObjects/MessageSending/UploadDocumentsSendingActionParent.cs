using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.EU.H7.Business
{
	public class UploadDocumentsSendingActionParent<TMessageSendingObject> :
		MessageSendingObjectParent<TMessageSendingObject>,
		IMessageSendingObjectFilteredCollectionProvider
		where TMessageSendingObject : UploadDocumentsSendingAction
	{
		public UploadDocumentsSendingActionParent(AsycudaManifestHeader manifestHeader) : base(manifestHeader)
		{
			this.manifestHeader = Argument.NotNull(manifestHeader, nameof(manifestHeader));
		}

		protected readonly AsycudaManifestHeader manifestHeader;

		public sealed override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError =>
			Env.Security.EuH7;

		protected override NonPersistentBusinessObjectCollection<TMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var result = new MessageSendingObjectCollection<TMessageSendingObject>(Factory);
			foreach (var bill in manifestHeader.Bills)
			{
				if (bill.IsDocumentationRequested)
				{
					result.Add(CreateNewMessageSendingObject(bill));
				}
			}

			return result;
		}

		protected TMessageSendingObject CreateNewMessageSendingObject(AsycudaBill bill)
		{
			return Activator.CreateInstance(typeof(TMessageSendingObject), bill) as TMessageSendingObject;
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		public UploadDocumentsSendingActionFilteredCollectionView<TMessageSendingObject> UploadDocumentsSendingActionFilteredCollection => GetUploadDocumentsSendingActionFilteredCollectionCore();

		protected virtual UploadDocumentsSendingActionFilteredCollectionView<TMessageSendingObject> GetUploadDocumentsSendingActionFilteredCollectionCore()
		{
			return uploadDocumentsSendingActionFilteredCollectionView ??= new UploadDocumentsSendingActionFilteredCollectionView<TMessageSendingObject>(SendingObjectsCollection);
		}

		UploadDocumentsSendingActionFilteredCollectionView<TMessageSendingObject> uploadDocumentsSendingActionFilteredCollectionView;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(UploadDocumentsSendingAction.Schema.BillNumber, false, 140),
			new MessageSendingObjectProperty(nameof(UploadDocumentsSendingAction.MovementReferenceNumber), false, 160),
			new MessageSendingObjectProperty(nameof(UploadDocumentsSendingAction.LocalReferenceNumber), false, 160),
			new MessageSendingObjectProperty(nameof(UploadDocumentsSendingAction.EntryStatus), false, 160),
			new MessageSendingObjectProperty(nameof(UploadDocumentsSendingAction.Schema.Action), true, 100),
		};

		public BusinessObjectCollection MessageSendingObjectFilteredCollection => UploadDocumentsSendingActionFilteredCollection;
	}
}
