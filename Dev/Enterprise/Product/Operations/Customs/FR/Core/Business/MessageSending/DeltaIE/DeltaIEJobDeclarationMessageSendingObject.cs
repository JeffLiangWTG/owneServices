using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaIEJobDeclarationMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public DeltaIEJobDeclarationMessageSendingObject(EU.Business.Declaration.CusEntryHeader header) : base(header)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new sealed class Schema : Customs.Business.JobDeclarationMessageSendingObject.Schema
		{
			public const string Description = nameof(Description);
			public const string SubStyle = nameof(SubStyle);
			public const string DateTime = nameof(DateTime);
			public const string Update = nameof(Update);
			public new const int ChangeAcknowledgementIndicatorMaxLength = 5;
			public const string ForOperationalAction = nameof(ForOperationalAction);
		}

		public Customs.Business.CusEntryInstruction EntryInstruction => Header.EntryInstruction;

		public DeltaIEJobDeclarationMessageSendingObjectLookups Lookups => new DeltaIEJobDeclarationMessageSendingObjectLookups(this);

		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new DeltaIEJobDeclarationMessageSendingObjectValidation(this);
		}

		public new DeltaIEJobDeclarationMessageSendingObjectValidation Validation => (DeltaIEJobDeclarationMessageSendingObjectValidation)base.Validation;

		protected override ZString GetDefaultMessageType()
		{
			if (Lookups.MessageSubTypeList.Count > 0)
			{
				return Lookups.MessageSubTypeList[0].Code;
			}
			else
			{
				return ZString.Empty;
			}
		}

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				base.ShouldSend = value;
				Validation.ValidateChangeAcknowledgementIndicator();
			}
		}

		#region Description
		[ResourceStringData("Enterprise.Customs.FR.Business.DeltaIEJobDeclarationMessageSendingObject|Description", Caption = "Description")]
		public ZString Description => EntryInstruction?.CEI_Description ?? ZString.Empty;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(Schema.Description);
		#endregion

		#region EntryStatus
		[ResourceStringData("Enterprise.Customs.FR.Business.DeltaIEJobDeclarationMessageSendingObject|EntryStatus", Caption = "Entry Status")]
		public override ZString EntryStatus => base.EntryStatus;
		#endregion

		#region SubStyle
		[ResourceStringData("Enterprise.Customs.FR.Business.DeltaIEJobDeclarationMessageSendingObject|SubStyle", Caption = "Sub Style")]
		public ZString SubStyle => EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

		public ZPropertyInfo SubStyleInfo => GetZPropertyInfo(Schema.SubStyle);
		#endregion

		#region DateTime
		[ResourceStringData("Enterprise.Customs.FR.Business.DeltaIEJobDeclarationMessageSendingObject|DateTime", Caption = "Date Time")]
		public ZDateTime DateTime => ZDateTime.Now;

		public ZPropertyInfo DateInfo => GetZPropertyInfo(Schema.DateTime);
		#endregion

		#region Update
		[ResourceStringData("Enterprise.Customs.FR.Business.DeltaIEJobDeclarationMessageSendingObject|Update", Caption = "Update")]
		[ReadOnly(false)]
		public ZBool Update
		{
			get
			{
				return update;
			}
			set
			{
				SetNonPersistentPropertyValue(UpdateInfo, ref update, value);
			}
		}
		ZBool update;

		public ZPropertyInfo UpdateInfo => GetZPropertyInfo(Schema.Update);
		#endregion

		#region MessageType

		[ResourceStringData("Enterprise.Customs.FR.Business.DeltaIEJobDeclarationMessageSendingObject|MessageType", ShortCaption = "Msg. Type", Caption = "Message Type")]
		[List(nameof(Lookups) + "." + nameof(DeltaIEJobDeclarationMessageSendingObjectLookups.MessageSubTypeList))]
		public override ZString MessageType
		{
			get => base.MessageType;
			set
			{
				var oldValue = MessageType;
				base.MessageType = value;
				if (oldValue != value)
				{
					Validation.ValidateChangeAcknowledgementIndicator();
				}
			}
		}

		protected override bool MessageType_ReadOnly => false;
		#endregion

		#region VOCReason

		[ResourceStringData("Enterprise.Customs.FR.Business.DeltaIEJobDeclarationMessageSendingObject|VOCReason", ShortCaption = "Reason", MediumCaption = "Reason", Caption = "Reason")]
		public override ZString VOCReason { get => base.VOCReason; set => base.VOCReason = value; }

		protected override bool VOCReason_ReadOnly => MessageType != DeltaIESendMessageSubTypeList.Codes.Invalidation && MessageType != DeltaIESendMessageSubTypeList.Codes.AmendmentRequest;

		#endregion

		#region ChangeAcknowledgementIndicator

		[ResourceStringData("Enterprise.Customs.FR.Business.DeltaIEJobDeclarationMessageSendingObject|ChangeAcknowledgementIndicator", ShortCaption = "Motivation", MediumCaption = "Motivation", Caption = "Motivation")]
		[MaxLength(Schema.ChangeAcknowledgementIndicatorMaxLength)]
		[List(nameof(Lookups) + "." + nameof(DeltaIEJobDeclarationMessageSendingObjectLookups.MotivationList))]
		public override ZString ChangeAcknowledgementIndicator { get => base.ChangeAcknowledgementIndicator; set => base.ChangeAcknowledgementIndicator = value; }

		protected override bool ChangeAcknowledgementIndicator_ReadOnly => MessageType != DeltaIESendMessageSubTypeList.Codes.AmendmentRequest && MessageType != DeltaIESendMessageSubTypeList.Codes.Invalidation;

		#endregion

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			ShouldSend = IsOneAndOnlyEntry
				|| MovementReferenceNumber.IsEmpty;
		}

		public override bool IsNew => true;

		public ZString WrapperModifier { get; set; } = ZString.Empty;

		public string OperatorRequestReference => Header.CorrelationID + "-" + ZDateTime.UtcNow.ToString("yyyyMMddhhmmss");
	}
}
