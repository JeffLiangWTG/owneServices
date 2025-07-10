using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public abstract class NctsHeaderDepartureMessageSendingObject : AutoNctsHeaderDepartureMessageSendingObject, ISadMessageSendingObject, ISadOutgoingCustomsMessageGeneratorValuesProvider, IEntryMessageSendingObjectInfo
{
	protected NctsHeaderDepartureMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader?.Factory)
	{
		NctsHeader = nctsHeader.CheckNotNullAndDepartureType();
		DepartureMovement = nctsHeader.MovementHeader;
	}

	protected NctsHeader NctsHeader { get; }
	protected NctsDepartureMovementHeader DepartureMovement { get; }

	public static class Schema
	{
		public const string DepartureStatus = "DepartureStatus";
		public const string CombinedCustomsMessageSubType = "CombinedCustomsMessageSubType";
		public const string MessageStatus = "MessageStatus";
		public const string ReferenceNumber = "ReferenceNumber";
		public const string CustomsMessageSendingMode = "CustomsMessageSendingMode";
	}

	[ResourceStringData("7D95264D-66FD-4210-82A0-2582CD5D1236", ShortCaption = "Dep. Status", Caption = "Departure Status")]
	public ZString DepartureStatus => DepartureMovement.BM_CustomsStatus;

	[ResourceStringData("419FD639-A05C-4D51-BA2C-FCCE772FCCAE", ShortCaption = "Msg. Type", Caption = "Message Type")]
	public ZString CombinedCustomsMessageSubType => combinedCustomsMessageSubType ?? (combinedCustomsMessageSubType = GetCombinedCustomsMessageSubType());
	string combinedCustomsMessageSubType;

	protected virtual ZString GetCombinedCustomsMessageSubType() => GetMessageSubType();

	[ResourceStringData("220FA234-7AD1-487F-A9C6-27036DA12BB5", ShortCaption = "Msg. Status", Caption = "Message Status")]
	public ZString MessageStatus => NctsHeader.EffectiveMessageStatus;

	[ResourceStringData("ECC39828-9579-4299-8088-8C2E08E2999C", ShortCaption = "Ref. No.", Caption = "Reference Number")]
	public ZString ReferenceNumber => NctsHeader.BH_JobReference;

	public ZString CustomsMessageSendingMode
	{
		get => customsMessageSendingMode;
		set => SetNonPersistentPropertyValue(CustomsMessageSendingModeInfo, ref customsMessageSendingMode, value);
	}
	ZString customsMessageSendingMode;

	public ZPropertyInfo CustomsMessageSendingModeInfo => GetZPropertyInfo(Schema.CustomsMessageSendingMode);

	#region ISadMessageSendingObject Members

	ZBool ISadMessageSendingObject.FallbackProcedure => CustomsMessageSendingMode == CustomsMessageSendingModeList.Codes.FallbackProcedure;

	ZString ISadMessageSendingObject.DeclarantTaxNumber => CustomsCredentialHelper.GetAccountFromNode(NctsHeader.BH_CustomsProfile)?.DeclarantTaxNumber ?? ZString.Empty;

	#endregion

	#region ISadCustomsMessageGeneratorValuesProvider

	BusinessObject IOutgoingCustomsMessageGeneratorValuesProvider.Parent => NctsHeader;

	IEnumerable<ISadCustomsMessage> ISadOutgoingCustomsMessageGeneratorValuesProvider.GetCustomsMessageObjects() => GetMessageObjects();

	protected abstract IEnumerable<ISadCustomsMessage> GetMessageObjects();

	ZString IOutgoingCustomsMessageGeneratorValuesProvider.GetSubType() => GetMessageSubType();

	protected abstract ZString GetMessageSubType();

	ZString IOutgoingCustomsMessageGeneratorValuesProvider.GetApplicationReference() => ApplicationReferenceHelper.GetNew(NctsHeader);

	ICustomsMessageFountainProvider ISadOutgoingCustomsMessageGeneratorValuesProvider.FountainProvider => fountainProvider ?? (fountainProvider = new NctsHeaderCustomsMessageFountainProvider(NctsHeader));
	ICustomsMessageFountainProvider fountainProvider;

	#endregion

	public override void ValidateShouldSend()
	{
		if (!IsValidationSuspended)
		{
			Validation.ValidateShouldSend();
		}
	}

	#region IEntryMessageSendingObjectInfo Members

	ZBool IEntryMessageSendingObjectInfo.EntryStatusAllowsSending => NctsHeader.StatusAllowsSending;

	ZString IEntryMessageSendingObjectInfo.EntryReference => NctsHeader.BH_JobReference;

	ZString IEntryMessageSendingObjectInfo.EntryMessageStatus => NctsHeader.EffectiveMessageStatus;

	ZString IEntryMessageSendingObjectInfo.EntryCustomsStatus => NctsHeader.DepartureMovementStatus;

	#endregion
}
