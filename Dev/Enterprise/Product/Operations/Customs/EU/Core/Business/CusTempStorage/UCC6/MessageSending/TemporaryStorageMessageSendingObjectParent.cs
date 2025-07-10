using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageMessageSendingObjectParent<T, THeader> : BaseMessageSendingObjectParent<T>
		where T : TemporaryStorageMessageSendingObject
		where THeader : TemporaryStorageHeader
	{
		public TemporaryStorageMessageSendingObjectParent(THeader header) : base(header.Factory)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		protected readonly THeader header;

		public override BusinessObject TopLevelBusinessObject => header;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
		{
			get
			{
				yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.MessageType, true);
				yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.VOCReason, false);
				yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.Date, false);
			}
		}

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsTemporaryStorageSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<T> GetSendingObjectsCollectionCore()
		{
			if (messageSendingObjectCollection == null)
			{
				messageSendingObjectCollection = new TemporaryStorageMessageSendingObjectCollection<T, THeader>(header);
				messageSendingObjectCollection.Add(GetSingleMessageSendingObject());
			}

			RegisterEditableChildObject(messageSendingObjectCollection);
			return messageSendingObjectCollection;
		}
		NonPersistentBusinessObjectCollection<T> messageSendingObjectCollection;

		TemporaryStorageMessageSendingObject GetSingleMessageSendingObject()
		{
			return new TemporaryStorageMessageSendingObject(header);
		}

		protected override ZString GetAdditionalWarningsCore()
		{
			var result = new ZStringBuilder(base.GetAdditionalWarningsCore());

			if (header.IsENSReuse)
			{
				result.AppendLine(Res.GetString("E6B55964-93BA-4D0D-A993-1218CAFA96BF", "When ENS Re-use = True, only minimal data will be sent to customs including MRN and Bill numbers. Bill details and related items will not be sent."));
			}

			return result.ToString();
		}
	}
}
