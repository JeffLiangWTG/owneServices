using CargoWise.ComponentModel;
using CargoWise.Customs.BR.MessageContracts.LPCO.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BR.Business.LPCO;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class LPCOMessageSendingObject : BaseMessageSendingObject, IMessageSendingObject, IObsoleteValidation
	{
		public LPCOMessageSendingObject(LPCOMessageSendingObjectParent parent) : base(parent.Factory)
		{
			Parent = CargoWise.Common.Argument.NotNull(parent, nameof(parent));
			RequestObject = new LPCORequestObject(MessageTypeInfo as ZPropertyInfoString);
		}

		public readonly LPCOMessageSendingObjectParent Parent;
		public LPCORequestObject RequestObject { get; private set; }

		public static class Schema
		{
			public const int MessageTypeMaxLength = 3;
		}

		public CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<LPCOMessageTypesList>();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			MessageType = LPCOMessageTypesList.Codes.ORI;
		}

		public bool IsWaitingForResponse => Parent.LPCOHeader.CPH_MessageStatus == BRMessageStatusList.Codes.AwaitingResponse;

		public LPCOMessageSendingObjectValidation Validation => new LPCOMessageSendingObjectValidation(this);

		#region ShouldSend

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				base.ShouldSend = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateShouldSend();
				}
			}
		}

		public ZPropertyInfo EntryStatusInfo => GetZPropertyInfo(nameof(ShouldSend));

		#endregion

		#region MessageType

		[MaxLength(SubscriptionMessageSendingObject.Schema.MessageTypeMaxLength)]
		[List(nameof(MessageTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.LPCOMessageSendingObject|MessageType", Caption = "Message Type")]
		public ZString MessageType
		{
			get { return fMessageType; }
			set
			{
				SetNonPersistentPropertyValue(MessageTypeInfo, ref fMessageType, value);
			}
		}
		ZString fMessageType;
		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(nameof(MessageType));

		#endregion

		#region Submitted Date

		[ResourceStringData("Enterprise.Customs.BR.Business.LPCOMessageSendingObject|SubmittedDate", Caption = "Submitted Date")]
		public ZDateTime SubmittedDate => ZDateTime.Empty;

		public ZPropertyInfo SubmittedDateInfo => GetZPropertyInfo(nameof(SubmittedDate));

		#endregion

		#region Customs Status

		[ResourceStringData("Enterprise.Customs.BR.Business.LPCOMessageSendingObject|CustomsStatus", Caption = "Customs Status")]
		public ZString CustomsStatus => ZString.Empty;

		public ZPropertyInfo CustomsStatusInfo => GetZPropertyInfo(nameof(CustomsStatus));

		#endregion

		#region MessageStatusDescription

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|MessageStatusDescription", Caption = "Message Status Description")]
		public ZString MessageStatusDescription => ZString.Empty;

		public ZPropertyInfo MessageStatusDescriptionInfo => GetZPropertyInfo(nameof(MessageStatusDescription));

		#endregion

		#region RequestObject

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|AmendmentReason", ShortCaption = "Reason")]
		public ZString Reason { get => RequestObject.Reason; set => RequestObject.Reason = value; }
		public ZPropertyInfo ReasonInfo => GetWrappedZPropertyInfo(nameof(Reason), x => RequestObject.ReasonInfo);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|NewEffectiveDate", Caption = "New Effective Date")]
		public ZDate NewEffectiveDate { get => RequestObject.NewEffectiveDate; set => RequestObject.NewEffectiveDate = value; }
		public ZPropertyInfo NewEffectiveDateInfo => GetWrappedZPropertyInfo(nameof(NewEffectiveDate), x => RequestObject.NewEffectiveDateInfo);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|Requirement", Caption = "Requirement")]
		public ZInt Requirement { get => RequestObject.Requirement; set => RequestObject.Requirement = value; }
		public ZPropertyInfo RequirementInfo => GetWrappedZPropertyInfo(nameof(Requirement), x => RequestObject.RequirementInfo);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|PermitNumber", Caption = "Permit Number")]
		public ZString PermitNumber { get => RequestObject.DocumentNumber; set => RequestObject.DocumentNumber = value; }
		public ZPropertyInfo PermitNumberInfo => GetWrappedZPropertyInfo(nameof(PermitNumber), x => RequestObject.DocumentNumberInfo);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.LPCOMessageSendingObject|Message", ShortCaption = "Message")]
		public ZString Message { get => RequestObject.Message; set => RequestObject.Message = value; }
		public ZPropertyInfo MessageInfo => GetWrappedZPropertyInfo(nameof(Message), x => RequestObject.MessageInfo);

		#endregion

		#region IMessageSendingObject

		public BusinessObject MessageAttachee => Parent.LPCOHeader;

		public ZString GetMessageOwner() => ZString.Empty;

		public ZString GetMessageTypeForEDIMessage() => Business.MessageTypeList.Codes.LPC;

		public ZString GetApplicationReference() => RequestObject.IsREQ ? new ZString(Parent.LPCOHeader.CPH_Number + "|" + RequestObject.Requirement) : Parent.LPCOHeader.CPH_Number;

		public ZGuid GetGlbExternalPasswordPK() => Parent.BrokerCertificate?.PK ?? ZGuid.Empty;

		public ZString GetMessageText()
		{
			IJsonMessageBuilder messageBuilder = null;

			if (MessageType == LPCOMessageTypesList.Codes.ORI)
			{
				messageBuilder = new LPCOIncludeRequestMessageBuilder(new IncludeLpcoRequestProvider(this));
			}
			else
			{
				messageBuilder = RequestObject.NewLPCOMessageBuilder();
			}

			return messageBuilder?.GenerateJsonMessage().GetSerializedString();
		}

		#endregion
	}
}
