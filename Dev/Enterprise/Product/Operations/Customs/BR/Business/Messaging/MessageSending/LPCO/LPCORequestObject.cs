using CargoWise.ComponentModel;
using CargoWise.Customs.BR.MessageContracts.LPCO.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BR.Business.LPCO;

namespace Enterprise.Customs.BR.Business
{
	public class LPCORequestObject : NonPersistentBusinessObject
	{
		public LPCORequestObject(ZPropertyInfoString messageTypeInfo)
		{
			this.messageTypeInfo = messageTypeInfo;
			messageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
		}
		readonly ZPropertyInfoString messageTypeInfo;

		void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			if (Reason_ReadOnly)
			{
				Reason = ZString.Empty;
			}

			if (NewEffectiveDate_ReadOnly)
			{
				NewEffectiveDate = ZDate.Empty;
			}

			if (Requirement_ReadOnly)
			{
				Requirement = ZInt.Zero;
			}

			if (DocumentAndVersion_ReadOnly)
			{
				DocumentNumber = ZString.Empty;
				DocumentItemNumber = ZInt.Zero;
				Version = ZString.Empty;
			}

			if (Message_ReadOnly)
			{
				Message = ZString.Empty;
			}
		}

		#region Reason

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|Reason", ShortCaption = "Reason")]
		public ZString Reason
		{
			get => fReason;
			set => SetNonPersistentPropertyValue(ReasonInfo, ref fReason, value);
		}

		ZString fReason;

		public ZPropertyInfo ReasonInfo => GetZPropertyInfo(nameof(Reason));

		protected bool Reason_ReadOnly => !IsREQ && !IsRCA && !IsALE && !IsCOM;

		#endregion

		#region NewEffectiveDate

		[ReadOnlyMember(nameof(NewEffectiveDate_ReadOnly))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|NewEffectiveDate", Caption = "New Effective Date")]
		public ZDate NewEffectiveDate
		{
			get => fNewEffectiveDate;
			set => SetNonPersistentPropertyValue(NewEffectiveDateInfo, ref fNewEffectiveDate, value);
		}

		ZDate fNewEffectiveDate;

		bool NewEffectiveDate_ReadOnly => !IsALE;

		public ZPropertyInfo NewEffectiveDateInfo => GetZPropertyInfo(nameof(NewEffectiveDate));

		#endregion

		#region Message

		[ReadOnlyMember(nameof(Message_ReadOnly))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|Message", ShortCaption = "Message")]
		public ZString Message
		{
			get => fMessage;
			set => SetNonPersistentPropertyValue(MessageInfo, ref fMessage, value);
		}

		ZString fMessage;

		public ZPropertyInfo MessageInfo => GetZPropertyInfo(nameof(Message));

		protected bool Message_ReadOnly => !IsMSG;

		#endregion

		#region Compatibility

		[MaxLength(15)]
		[ReadOnlyMember(nameof(DocumentAndVersion_ReadOnly))]
		public ZString DocumentNumber
		{
			get { return fDocumentNumber; }
			set { SetNonPersistentPropertyValue(DocumentNumberInfo, ref fDocumentNumber, value); }
		}

		ZString fDocumentNumber;

		public ZPropertyInfo DocumentNumberInfo => GetZPropertyInfo(nameof(DocumentNumber));

		[MaxLength(3)]
		[ReadOnlyMember(nameof(DocumentAndVersion_ReadOnly))]
		public ZInt DocumentItemNumber
		{
			get { return fDocumentItemNumber; }
			set { SetNonPersistentPropertyValue(DocumentItemNumberInfo, ref fDocumentItemNumber, value); }
		}

		ZInt fDocumentItemNumber;

		public ZPropertyInfo DocumentItemNumberInfo => GetZPropertyInfo(nameof(DocumentItemNumber));

		[MaxLength(3)]
		[ReadOnlyMember(nameof(DocumentAndVersion_ReadOnly))]
		public ZString Version
		{
			get { return fVersion; }
			set { SetNonPersistentPropertyValue(VersionInfo, ref fVersion, value); }
		}

		ZString fVersion;

		public ZPropertyInfo VersionInfo => GetZPropertyInfo(nameof(Version));

		protected bool DocumentAndVersion_ReadOnly => !IsCOM;

		#endregion

		#region Requirement

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|Requirement", Caption = "Requirement")]
		[ReadOnlyMember(nameof(Requirement_ReadOnly))]
		public ZInt Requirement
		{
			get => fRequirement;
			set => SetNonPersistentPropertyValue(RequirementInfo, ref fRequirement, value);
		}

		ZInt fRequirement;

		public ZPropertyInfo RequirementInfo => GetZPropertyInfo(nameof(Requirement));

		protected bool Requirement_ReadOnly => !IsREQ;

		#endregion

		public bool IsREQ => messageTypeInfo.Value == LPCOEntryActionCodeList.Codes.REQ;

		public bool IsRCA => messageTypeInfo.Value == LPCOEntryActionCodeList.Codes.RCA;

		public bool IsALE => messageTypeInfo.Value == LPCOEntryActionCodeList.Codes.ALE;

		public bool IsCOM => messageTypeInfo.Value == LPCOEntryActionCodeList.Codes.COM;

		public bool IsMSG => messageTypeInfo.Value == LPCOEntryActionCodeList.Codes.MSG;

		public IJsonMessageBuilder NewLPCOMessageBuilder()
		{
			IJsonMessageBuilder messageBuilder = null;

			if (IsRCA)
			{
				messageBuilder = new LPCOCancellationRetificationMessageBuilder(new CancelRectificationRequestProvider(this));
			}
			else if (IsREQ)
			{
				messageBuilder = new LPCOAcceptCompatibilityRequestMessageBuilder(new AcceptCompatibilityRequestProvider(this));
			}
			else if (IsCOM)
			{
				messageBuilder = new LPCOCompatibilityRequestMessageBuilder(new CompatibilityRequestProvider(this));
			}
			else if (IsALE)
			{
				messageBuilder = new LPCOApplyExtensionMessageBuilder(new ApplyExtensionRequestProvider(this));
			}
			else if (IsMSG)
			{
				messageBuilder = new LPCOConsentingBodyMessageBuilder(new ConsentingBodyRequestProvider(this));
			}

			return messageBuilder;
		}
	}
}
