using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.EU.H7.Business
{
	public class DocumentRequestSendingActionParent<TMessageSendingObject> : MessageSendingObjectParent<TMessageSendingObject> where TMessageSendingObject : DocumentRequestSendingAction
	{
		public DocumentRequestSendingActionParent(AsycudaManifestHeader manifestHeader) : base(
			manifestHeader)
		{
			this.manifestHeader = Argument.NotNull(manifestHeader, nameof(manifestHeader));
		}

		protected readonly AsycudaManifestHeader manifestHeader;

		public sealed override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.EuH7;

		protected override NonPersistentBusinessObjectCollection<TMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var result = new MessageSendingObjectCollection<TMessageSendingObject>(Factory);
			foreach (var bill in manifestHeader.Bills)
			{
				result.Add(CreateNewMessageSendingObject(bill));
			}

			return result;
		}

		protected TMessageSendingObject CreateNewMessageSendingObject(AsycudaBill bill)
		{
			return Activator.CreateInstance(typeof(TMessageSendingObject), bill) as TMessageSendingObject;
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(DocumentRequestSendingAction.Schema.BillNumber, false, 140),
			new MessageSendingObjectProperty(nameof(DocumentRequestSendingAction.MovementReferenceNumber), false, 140),
		};
	}
}
