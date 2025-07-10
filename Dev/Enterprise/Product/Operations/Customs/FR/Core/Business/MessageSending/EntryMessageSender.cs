using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public abstract class EntryMessageSender<T> : MessageSender<T>
		where T : JobDeclarationMessageSendingObject
	{
		protected EntryMessageSender(T decWrapper, ErrorCollector errorCollector) : base(decWrapper, errorCollector)
		{
			this.decWrapper = Argument.NotNull(decWrapper, nameof(decWrapper));
			entryHeader = decWrapper.Header;
		}

		protected override bool PreSend()
		{
			originalequenceNumber = entryHeader.CH_SequenceNumber;
			originalStatus = entryHeader.CH_Status;
			var isOKToSend = true;
			if (objectToSend.IsNew || objectToSend.IsAmend)
			{
				isOKToSend = new FRMessageManagerCreditCheckWithSecurityHelper(objectToSend, errorCollector).WarnAboutCreditChecks();
			}
			return isOKToSend;
		}

		protected override void PreCreateEdiMessage()
		{
			if (ShouldIncreaseSequenceNumber)
			{
				entryHeader.CH_SequenceNumber = originalequenceNumber + 1;
			}

			entryHeader.CH_Status = MessageStatusCodeList.Codes.AWR;
		}

		protected abstract bool ShouldIncreaseSequenceNumber { get; }

		protected override void PostCreateEdiMessage(EDIMessage message)
		{
			AddPermitsIfApplicable(message);
			AddDocumentsIfApplicable(message);
			CreateEntrySnapshotIfApplicable();
		}

		protected override void PostSend(ZBool result)
		{
			if (!result)
			{
				entryHeader.CH_SequenceNumber = originalequenceNumber;
				entryHeader.CH_Status = originalStatus;
			}
		}

		protected virtual void AddPermitsIfApplicable(EDIMessage message)
		{
		}

		protected virtual void AddDocumentsIfApplicable(EDIMessage message)
		{
		}

		protected virtual void CreateEntrySnapshotIfApplicable()
		{
		}

		protected JobDeclarationMessageSendingObject decWrapper;
		protected Declaration.CusEntryHeader entryHeader;
		ZInt originalequenceNumber;
		ZString originalStatus;
	}
}
