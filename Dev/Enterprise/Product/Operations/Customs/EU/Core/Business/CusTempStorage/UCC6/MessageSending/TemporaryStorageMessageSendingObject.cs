using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageMessageSendingObject : AutoTemporaryStorageMessageSendingObject
	{
		public new class Schema : AutoTemporaryStorageMessageSendingObject.Schema
		{
			public const string AlternativeDateOfAcceptance = "AlternativeDateOfAcceptance";
		}
		public TemporaryStorageMessageSendingObject(TemporaryStorageHeader header) : base(header?.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				SetMessageSendingDefaultValues();
			}
		}

		public readonly TemporaryStorageHeader Header;

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageMessageSendingObjectLookups.MessageTypes))]
		public override ZString MessageType
		{
			get => base.MessageType;
			set => base.MessageType = value;
		}

		public TemporaryStorageMessageSendingObjectLookups Lookups => new TemporaryStorageMessageSendingObjectLookups(this);

		void SetMessageSendingDefaultValues()
		{
			ShouldSend = true;
			MessageType = Header.MessagingProvider?.GetDefaultMessageType(this) ?? ZString.Empty;
		}

		[ResourceStringData("EU.TemporaryStorageMessageSendingObject|AlternativeDateOfAcceptance", Caption = "Alternative Date of Acceptance")]
		public virtual ZDate AlternativeDateOfAcceptance
		{
			get => alternativeDateOfAcceptance;
			set
			{
				SetNonPersistentPropertyValue(AlternativeDateOfAcceptanceInfo, ref alternativeDateOfAcceptance, value);
			}
		}
		ZDate alternativeDateOfAcceptance;
		public ZPropertyInfo AlternativeDateOfAcceptanceInfo => GetZPropertyInfo(nameof(AlternativeDateOfAcceptance));

		protected override bool VOCReason_ReadOnly => MessageType != TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD;

		[ResourceStringData("EU.TemporaryStorageMessageSendingObject|EntryStatus", Caption = "Entry Status")]
		public override ZString EntryStatus => Header.CustomsStatus;

		[ResourceStringData("EU.TemporaryStorageMessageSendingObject|MessageStatus", Caption = "Message Status")]
		public override ZString MessageStatus => Header.AMA_MessageStatus;
	}
}
