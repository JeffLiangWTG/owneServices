using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.BE.Business.CusTempStorage;

public class TemporaryStorageMessageSendingObject : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject
{
	public TemporaryStorageMessageSendingObject(EU.Business.CusTempStorage.TemporaryStorageHeader header) : base(header)
	{
		using (GetValidationDataSuspender())
		using (SuspendSettingHasChanges())
		{
			SetMessageSendingDefaultValues();
		}
	}

	public class BeSchema : Schema
	{
		public const string IsTestDeclaration = "IsTestDeclaration";
		public const string ReferenceNumber = "ReferenceNumber";
	}

	void SetMessageSendingDefaultValues()
	{
		var provider = Header.MessagingProvider;
		if (provider is TemporaryStorageMessagingProvider beProvider)
		{
			MessageType = beProvider.GetDefaultMessageType(this);
			DeclarationType = Header.AMA_MessageType;
		}
	}
	public new TemporaryStorageHeader Header => (TemporaryStorageHeader)base.Header;

	public override ZString EntryStatus => Header.CustomsStatus;

	[ReadOnlyMember(nameof(DeclarationType_ReadOnly))]
	public override ZString DeclarationType { get => base.DeclarationType; set => base.DeclarationType = value; }

	protected override bool DeclarationType_ReadOnly => true;

	[ResourceStringData("Enterprise.Customs.BE.Business.CusTempStorage.TemporaryStorageMessageSendingObject|IsTestDeclaration", Caption = "Test?")]
	public ZBool IsTestDeclaration
	{
		get => isTestDeclaration;
		set => SetNonPersistentPropertyValue(IsTestDeclarationInfo, ref isTestDeclaration, value);
	}
	ZBool isTestDeclaration;

	public ZPropertyInfo IsTestDeclarationInfo => GetZPropertyInfo(BeSchema.IsTestDeclaration);

	[ResourceStringData("Enterprise.Customs.BE.Business.CusTempStorage.TemporaryStorageMessageSendingObject|Reference Number", Caption = "Reference Number")]
	public ZString ReferenceNumber => Header.LRN;

	public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(BeSchema.ReferenceNumber);
}
