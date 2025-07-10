using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Business;

class BondedWarehouseEntryMessageProcessor : Customs.Business.MessageProcessors.BondedWarehouseEntryMessageProcessor
{
	public BondedWarehouseEntryMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, EDIMessage> sendMail) : base(messagePK, emailReportThatHasBeenDelayed, sendMail)
	{
	}

	protected override ZDateTime AssessmentDate
	{
		get
		{
			var result = entryHeader.EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Invalid;
			return result.IsValid ? result : ZDateTime.Today;
		}
	}

	protected override string CountryCode => entryHeader.CountryCode;

	protected override bool IsOriginalError
		=> entryHeader.CH_EntryStatus.IsEmpty
		&& (entryHeader.CH_Status.ToString() is ITMessageStatusList.Codes.FailedFromTransmission or ITMessageStatusList.Codes.ErrorOriginal);

	protected override bool HasBeenWithdrawn => false;

	protected override bool IsWithdrawalError => false;

	protected override bool IsAmendmentClear => false;

	protected override bool IsAmendmentError => false;

	new CusEntryHeader entryHeader => (CusEntryHeader)base.entryHeader;
}
