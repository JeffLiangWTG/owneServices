using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.NCTS.Business;

public class BondedWarehouseNctsHeaderMessageProcessor : BondedWarehouseMessageProcessor
{
	public BondedWarehouseNctsHeaderMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, EDIMessage> sendMail) : base(messagePK, emailReportThatHasBeenDelayed)
	{
		this.sendMail = Argument.NotNull(sendMail, nameof(sendMail));
	}

	readonly Action<EmailDef, EDIMessage> sendMail;

	protected override void SendEmailCore(EmailDef email)
	{
		sendMail(email, message);
	}

	protected NctsHeader NctsHeader
	{
		get { return (NctsHeader)supporter; }
	}

	protected override string GetReferenceDetail()
	{
		return Res.GetString("{5837AC3F-6BC5-40B2-BD60-33E539D3E51C}", @"<strong>Job Reference: {0}
Local Reference Number: {1}
Entry Number: {2}", EmailDefBuilder.GetJobLink(NctsHeader, NctsHeader.BH_JobReference), NctsHeader.LocalReferenceNumber, NctsHeader.MovementReferenceEntryNumber);
	}

	protected override string GetSubject(string subjectPrefix)
	{
		return Res.GetString("{E77CF7EE-E043-405F-8AB7-DDBE9E9A270E}", "{0} for NCTS: {1}", subjectPrefix, NctsHeader.BH_JobReference);
	}

	protected override IWarehouseIntegrationSupporter GetSupporter()
	{
		return (NctsHeader)message.EM_LinkedObject;
	}

	protected override bool HasBeenWithdrawn => false;
	protected override bool IsOriginalError => false;
	protected override bool IsAmendmentError => false;
	protected override bool IsAmendmentClear => false;
	protected override bool IsWithdrawalError => false;
}
