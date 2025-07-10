using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class NctsHeaderSendableCustomsEntry : ISendableCustomsEntry
{
	public NctsHeaderSendableCustomsEntry(NctsHeader nctsHeader, string messageType = EDIMessageTypeList.Codes.NewDeclaration)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		movementHeader = nctsHeader.MovementHeader;
		this.messageType = messageType;
	}

	#region ISendableCustomsEntry

	void ISendableCustomsEntry.ConsumeGuarantee(BusinessObjectFactory factory, ITEDIMessage message)
	{
		var guaranteeTransactionProcessor = new NctsHeaderGuaranteeTransactionProcessor(nctsHeader);
		var transactions = guaranteeTransactionProcessor.AddNewConsumeTransaction();

		message.Saving += OnMessageSaving;

		void OnMessageSaving(EDIMessage m)
		{
			transactions.ForEach(x => x.CPL_AppId = m.EM_MessageNum);
			m.Saving -= OnMessageSaving;
		}
	}

	void ISendableCustomsEntry.PreProcessBeforeSending()
	{
		if (nctsHeader.IsPhase5Departure)
		{
			NctsMovementHeaderValuationDateHelper.SetValuationDate(movementHeader, isAmending: messageType == EDIMessageTypeList.Codes.Amendment);
		}
		else
		{
			movementHeader.BM_EntryDate = ZDate.Today;
		}
	}

	void ISendableCustomsEntry.MarkAsSent(IMessageType sentMessage)
	{
		if (!nctsHeader.StatusAllowsSending)
		{
			nctsHeader.Logs.LogCustomsStatusOverride(nctsHeader.BH_JobReference, nctsHeader.EffectiveMessageStatus, nctsHeader.DepartureMovementStatus);
		}
		if (nctsHeader.IsPhase5)
		{
			MarkAsSentPhase5();
			return;
		}
		MarkAsSentPhase4();
	}

	ZString ISendableCustomsEntry.CustomsProfile => nctsHeader.BH_CustomsProfile;

	#endregion

	void MarkAsSentPhase5()
	{
		nctsHeader.CommonMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.SentToCustoms;
		movementHeader.BM_CustomsStatus = ZString.Empty;

		movementHeader.BM_Phase = messageType switch
		{
			EDIMessageTypeList.Codes.NewDeclaration => NctsMovementHeaderTransactionStatusList.Codes.Declaration,
			EDIMessageTypeList.Codes.Cancellation => NctsMovementHeaderTransactionStatusList.Codes.Cancellation,
			_ => movementHeader.BM_Phase,
		};
	}

	void MarkAsSentPhase4()
	{
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
		foreach (var goodsItemWithSendablePreviousDocuments in movementHeader.GoodsItems.GetItemsWithSendableGroupedPreviousDocuments())
		{
			goodsItemWithSendablePreviousDocuments.BY_Status = EntryLineCustomsStatusList.Codes.Sent;
		}
	}

	readonly NctsHeader nctsHeader;
	readonly NctsDepartureMovementHeader movementHeader;
	readonly string messageType;
}
