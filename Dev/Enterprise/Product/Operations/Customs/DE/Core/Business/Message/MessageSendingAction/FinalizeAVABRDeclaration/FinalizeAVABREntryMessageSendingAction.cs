using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business;

public class FinalizeAVABREntryMessageSendingAction : MessageSendingAction
{
	public FinalizeAVABREntryMessageSendingAction(BusinessObject messagingObject, MessageSendingActionParent actionParent) : base(messagingObject, x => ((CusEntryHeader)x).EntryNumber, actionParent)
	{
	}

	public new CusEntryHeader MessagingObject => (CusEntryHeader)base.MessagingObject;

	public CusEntryInstruction EntryInstruction => MessagingObject.EntryInstruction;

	public ZString DeclarationType => EntryInstruction?.CEI_Style ?? ZString.Empty;

	public ZString Description => EntryInstruction?.CEI_Description ?? ZString.Empty;

	public ZString EntryStatus => MessagingObject.CH_EntryStatus;

	public ZString RegistrationNumber => MessagingObject?.MovementReferenceNumber ?? ZString.Empty;

	public ZBool CanSend => EntryStatus != UniversalReferenceConstants.EntryStatus.TX8;

	protected override bool ShouldSend_ReadOnly => !CanSend;

	[ResourceStringData("96A757D8-B5C4-4278-A18A-CDD600B6AFCC", Caption = "Finalize?")]
	public override ZBool ShouldSend { get => base.ShouldSend; set => base.ShouldSend = value; }
}
