using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.Messaging.Business;
namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaGMessageSender : EntryMessageSender<DeltaGJobDeclarationMessageSendingObject>
	{
		public DeltaGMessageSender(DeltaGJobDeclarationMessageSendingObject decWrapper, ErrorCollector errorCollector) : base(decWrapper, errorCollector)
		{
		}
		protected override bool ShouldIncreaseSequenceNumber => true;

		protected override MessageBuilderManager<DeltaGJobDeclarationMessageSendingObject> GetBuilderManager()
		{
			return new DeltaGMessageBuilderManager(errorCollector);
		}

		protected override void AddPermitsIfApplicable(EDIMessage message)
		{
			if ((objectToSend.MessageType == EntryActionCodeList.Codes.VAL || objectToSend.MessageType == EntryActionCodeList.Codes.REC || objectToSend.MessageType == EntryActionCodeList.Codes.VAA) && (entryHeader.Declaration.Ai2Permit != null || entryHeader.Declaration.CustomsGuarantee != null))
			{
				var permitProcessor = new FRCusPermitCusDecProcessorForMessage(entryHeader, messageType, objectToSend.MessageType == EntryActionCodeList.Codes.REC, entryHeader.Declaration.Ai2Permit);
				try
				{
					permitProcessor.AddPermitRecordsAndLockMutexIfNeeded();
					permitProcessor.AddPermitTransactions(message, (msg) => FRPermitHelper.GetPermitAppIdForMessage(message));
				}
				finally
				{
					permitProcessor.UnlockPermitMutexes();
				}
			}
			else if (objectToSend.MessageType == EntryActionCodeList.Codes.INV)
			{
				PermitHelper.RollbackPermitTransactionsIfSendingCancel(message, null, FRPermitHelper.GetPermitAppIdForMessage, ZString.Empty, message.GetCountryCodeSafe(), PermitTransactionStatusList.Codes.Pending);
			}
		}

		protected override void Message_Saving(EDIMessage message)
		{
			if (FRCustomsDataRegistry.DeltaGFallbackIsActive)
			{
				message.EM_HeldUntilDate = ZDateTime.MaxSmallDateTime;

				if (entryHeader != null)
				{
					entryHeader.InitFRCustomsFallbackEntryNumber();
					entryHeader.DeltaGFallbackStatus = DeltaGFallbackStatusList.Codes.PPW;
					entryHeader.DeltaGFallbackIssueDate = ZDateTime.Now;
				}
			}
			else
			{
				if (entryHeader != null && entryHeader.IsDeltaGFallbackInactiveAndNotRegularised)
				{
					entryHeader.DeltaGFallbackStatus = DeltaGFallbackStatusList.Codes.RGM;
				}
			}
		}

		protected override void CreateEntrySnapshotIfApplicable()
		{
			if (entryHeader != null && entryHeader.Declaration.IsDeltaD && (ApplicableEntryActionCodeForSnapshot.Contains(objectToSend.MessageType)))
			{
				var snapshot = entryHeader.Snapshots.AddNew();
				{
					snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current;
					snapshot.CES_MessageType = objectToSend.MessageType;
					snapshot.CES_SnapshotXml = entryHeader.GetEntrySnapshotData();
				}
			}
		}

		internal static ZString[] ApplicableEntryActionCodeForSnapshot => new ZString[] { EntryActionCodeList.Codes.VAL, EntryActionCodeList.Codes.VAA };
	}
}
