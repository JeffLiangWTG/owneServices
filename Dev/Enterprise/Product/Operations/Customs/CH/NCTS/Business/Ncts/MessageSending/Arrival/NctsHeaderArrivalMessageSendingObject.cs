using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderArrivalMessageSendingObject : NctsHeaderCommonMessageSendingObject
{
	public NctsHeaderArrivalMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader)
	{
		SetDefaultValues(false);
	}

	public NctsHeaderArrivalMessageSendingObject(NctsHeader nctsHeader, bool isMultiMrnView) : base(nctsHeader)
	{
		SetDefaultValues(isMultiMrnView);
	}

	void SetDefaultValues(bool isMultiMrnView)
	{
		MessageType = Lookups.MessageTypeList?.Count > 0 ? (ZString)Lookups.MessageTypeList[0].Code : ZString.Empty;
		SendingObjectHelper.CheckHeaderNotNullAndArrivalType(NctsHeader);
		this.isMultiMrnView = isMultiMrnView;
		if (isMultiMrnView)
		{
			var sealsStateValid = MovementHeader.MasterArrivalMovementHeader.MovementReferenceNumbers.Where(x => x.CSI_ReferenceNumber == MovementHeader.Header.MovementReferenceNumber).FirstOrDefault()?.CSI_Status;
			ShouldSend = !YesNoList.IsNo(sealsStateValid) && AllowMultipleMrnSend;
		}
	}

	NctsArrivalMovementHeader MovementHeader => NctsHeader.ArrivalMovementHeader;

	bool isMultiMrnView { get; set; }

	bool AllowMultipleMrnSend
	{
		get
		{
			return (MovementHeader.BM_NoChangesToReport && (MovementHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted && MovementHeader.BM_MessageStatus.IsEmpty))
				|| (MovementHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks && MovementHeader.BM_MessageStatus == LogicalStatusList.Codes.Invalid);
		}
	}

	protected override bool ShouldSend_ReadOnly => isMultiMrnView ? !AllowMultipleMrnSend : ZBool.True;

	protected override void SetDefaultDataCore()
	{
		base.SetDefaultDataCore();
		ShouldSend = ZBool.True;
	}

	public new NctsHeaderArrivalMessageSendingObjectLookups Lookups => (NctsHeaderArrivalMessageSendingObjectLookups)base.Lookups;

	protected override NctsHeaderMessageSendingObjectLookups GetNewLookups() => new NctsHeaderArrivalMessageSendingObjectLookups(this);

	public override ZString LRN => NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum;

	protected override ZString GetMessageSubTypeForEDIMessage()
	{
		switch (MessageType)
		{
			case PassarMessageTypeList.Codes.NT007:
				return MessageSubTypeCodeList.Codes.PassarArrivalNotification;
			case PassarMessageTypeList.Codes.NT044:
				return MessageSubTypeCodeList.Codes.PassarInventoryResult;
			default:
				return ZString.Empty;
		}
	}
}
