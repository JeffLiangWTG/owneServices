using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.Manifest.Business;

public class ManifestMessageSendingObject : BaseMessageSendingObject, IMessageSendingObject
{
	public ManifestMessageSendingObject(AsycudaManifestHeader manifestHeader) : base(manifestHeader?.Factory)
	{
		Parent = Argument.NotNull(manifestHeader, nameof(manifestHeader));
		DefaultMessageType();
	}

	public AsycudaManifestHeader Parent { get; }

	public static class Schema
	{
		public const string BillNumber = nameof(ManifestMessageSendingObject.BillNumber);
		public const string MessageType = nameof(ManifestMessageSendingObject.MessageType);
		public const string MessageTypeDescription = nameof(ManifestMessageSendingObject.MessageTypeDescription);
		public const string ShouldSend = nameof(ManifestMessageSendingObject.ShouldSend);
	}

	IMessageAttachee IMessageSendingObject.MessageAttachee => Parent;

	[ReadOnlyMember(nameof(MessageType_ReadOnly))]
	[List(nameof(Lookups) + "." + nameof(ManifestMessageSendingObjectLookups.MessageTypes))]
	[ResourceStringData("4110A51D-7033-4AA3-8180-C76619FCA9B5", Caption = "Message Type")]
	public ZString MessageType
	{
		get => messageType;
		set
		{
			SetNonPersistentPropertyValue(MessageTypeInfo, ref messageType, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateMessageType();
			}
		}
	}
	ZString messageType;

	public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(Schema.MessageType);

	public bool MessageType_ReadOnly => ActionIsDelete;

	[ResourceStringData("E7D9D6AC-37B7-4B26-B661-6EB8E67F23DA", Caption = "Description")]
	[ReadOnly(true)]
	public ZString MessageTypeDescription => Lookups.MessageTypes.GetDescriptionFromCode(MessageType);

	public ZPropertyInfo MessageTypeDescriptionInfo => GetZPropertyInfo(Schema.MessageTypeDescription);

	public ZString MessageTypeText
	{
		get
		{
			switch (MessageType)
			{
				case ManifestMessageTypeList.Codes.Fresh:
					return Res.GetString("8DA2F385-446C-4F56-8CC7-1DB45526E6A4", "Submission of the Fresh CGM Manifest Message to ICEGate.");
				case ManifestMessageTypeList.Codes.Delete:
					return Res.GetString("D80BDB57-8402-4B5D-9063-33566273DCF1", "Delete requisition for registered CGM Manifest Message to ICEGate.");
				case ManifestMessageTypeList.Codes.Amendment:
					return Res.GetString("48E6B6DC-DEC3-41F5-9121-C548901E4D10", "Amendment requisition for registered CGM Manifest Message to ICEGate.");
				default:
					return ZString.Empty;
			}
		}
	}

	public ZPropertyInfo MessageTypeTextInfo => GetZPropertyInfo(nameof(MessageTypeText));

	[ReadOnly(true)]
	public ZString BillNumber => Parent.MasterBill.ABL_BillNumber;

	public ZPropertyInfo BillNumberInfo => GetZPropertyInfo(Schema.BillNumber);

	public ManifestMessageSendingObjectLookups Lookups => lookups ??= GetNewLookups();
	ManifestMessageSendingObjectLookups lookups;

	ManifestMessageSendingObjectLookups GetNewLookups() => new(this);

	public ManifestMessageSendingObjectValidation Validation => GetNewValidation();

	protected ManifestMessageSendingObjectValidation GetNewValidation() => new(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		ShouldSend = ZBool.True;
	}

	void DefaultMessageType()
	{
		MessageType = ActionIsDelete ? ManifestMessageTypeList.Codes.Delete : Lookups.MessageTypes.DefaultCode;
	}

	bool ActionIsDelete => Parent is CGMAsycudaManifestHeader header && header.Action == ManifestMessageTypeList.Codes.Delete;
}
