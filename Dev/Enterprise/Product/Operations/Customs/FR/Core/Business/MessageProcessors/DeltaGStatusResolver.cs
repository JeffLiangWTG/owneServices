using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.EdiMessages;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaGStatusResolver
	{
		public DeltaGStatusResolver(CusEntryHeader entry, FREDIMessage incomingMessage)
		{
			this.entry = entry;
			this.incomingMessage = incomingMessage;
		}

		readonly CusEntryHeader entry;
		readonly FREDIMessage incomingMessage;

		ZString EntryAction
		{
			get
			{
				if (!entryAction.HasValue)
				{
					entryAction = EntryActionHelper.GetEntryAction(entry, incomingMessage);
				}

				return entryAction.Value;
			}
		}
		ZString? entryAction;

		ZString EntryStatus
		{
			get
			{
				if (!entryStatus.HasValue)
				{
					entryStatus = EntryActionHelper.GetEntryStatus(incomingMessage);
				}

				return entryStatus.Value;
			}
		}
		ZString? entryStatus;

		bool HasError
		{
			get
			{
				if (!hasError.HasValue)
				{
					hasError = EntryActionHelper.HasError(incomingMessage);
				}

				return hasError.Value;
			}
		}
		bool? hasError;

		public bool CheckIsOriginalError()
		{
			return ((EntryAction == EntryActionCodeList.Codes.ANT || EntryAction == EntryActionCodeList.Codes.VAL) && HasError) || (EntryStatus == EntryStatusDescriptionCodeList.Codes.ES090);
		}

		public bool CheckIsOriginalClear()
		{
			return (EntryAction == EntryActionCodeList.Codes.VAA || EntryAction == EntryActionCodeList.Codes.EAV || EntryAction == EntryActionCodeList.Codes.VAL) && EntryStatus == EntryStatusDescriptionCodeList.Codes.ES100;
		}

		public bool CheckIsModificationAccepted()
		{
			return EntryAction == EntryActionCodeList.Codes.MAP && !HasError;
		}

		public bool CheckIsModificationRejected()
		{
			return EntryAction == EntryActionCodeList.Codes.MAP && HasError;
		}

		public bool CheckIsRectificationError()
		{
			return (EntryAction == EntryActionCodeList.Codes.REC && HasError) || (EntryActionHelper.IsRectificationRefused(incomingMessage)) || (EntryStatus == EntryStatusDescriptionCodeList.Codes.ES120);
		}

		public bool CheckIsRectificationClear()
		{
			return EntryActionHelper.IsRectificationAccepted(incomingMessage);
		}

		public bool CheckHasBeenWithdrawn()
		{
			return EntryActionHelper.IsWithdrawnAccepted(EntryStatus);
		}

		public bool CheckIsWithdrawalError()
		{
			return EntryActionHelper.IsWithdrawnRefused(entry, incomingMessage);
		}

		public bool ShouldUpdateBondedWarehouse()
		{
			var warehouseTransactionStatus = entry.CH_WarehouseTransactionStatus;
			return WarehouseTransactionStatusList.IsPendingInwardOrOutward(warehouseTransactionStatus) &&
				(CheckIsOriginalClear() || CheckIsOriginalError() || CheckIsModificationAccepted() ||
				CheckIsModificationRejected() || CheckIsRectificationError() || CheckIsRectificationClear() ||
				CheckHasBeenWithdrawn() || CheckIsWithdrawalError());
		}
	}
}
