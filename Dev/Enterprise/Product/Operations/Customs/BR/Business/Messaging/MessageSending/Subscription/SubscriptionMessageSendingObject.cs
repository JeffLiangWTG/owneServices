using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Customs.BR.MessageContracts.Subscription.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.Subscription;
using Enterprise.Customs.Business;
using static Enterprise.Customs.BR.Business.GlbExternalPassword_BRS;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class SubscriptionMessageSendingObject : BaseMessageSendingObject, IMessageSendingObject, IObsoleteValidation
	{
		public SubscriptionMessageSendingObject(GlbExternalPassword_BRS subscription) : base(subscription.Factory)
		{
			Subscription = Argument.NotNull(subscription, nameof(subscription));
		}

		public static class Schema
		{
			public const string MessageType = "MessageType";
			public const string EventId = "EventId";
			public const string SubmittedDate = "SubmittedDate";
			public const string Status = "Status";
			public const int MessageTypeMaxLength = 3;
		}

		public readonly GlbExternalPassword_BRS Subscription;

		public CodeDescriptionPairList SubscriptionMessageTypes => Subscription.Factory.GetCachedValue<SubscriptionMessageTypesList>();

		#region MessageType

		[MaxLength(SubscriptionMessageSendingObject.Schema.MessageTypeMaxLength)]
		[List(nameof(SubscriptionMessageTypes))]
		[ResourceStringData("Enterprise.Customs.BR.Business.MessageSending|MessageType", Caption = "Message Type")]
		public ZString MessageType
		{
			get { return fMessageType; }
			set
			{
				SetNonPersistentPropertyValue(MessageTypeInfo, ref fMessageType, value);

				if (!IsValidationSuspended)
				{
					ValidateMessageType();
				}
			}
		}

		ZString fMessageType;

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(Schema.MessageType);

		#endregion

		#region EventId

		[MaxLength(40)]
		[ResourceStringData("Enterprise.Customs.BR.Business.MessageSending|EventId", Caption = "Event Id")]
		public ZString EventId => Subscription.GP_UserID;

		public ZPropertyInfo EventIdInfo => GetZPropertyInfo(Schema.EventId);

		#endregion

		#region Submitted Date

		[ResourceStringData("Enterprise.Customs.BR.Business.MessageSending|SubmittedDate", Caption = "Submitted Date")]
		public ZDateTime SubmittedDate => Subscription.SubmittedDate;

		public ZPropertyInfo SubmittedDateInfo => GetZPropertyInfo(Schema.SubmittedDate);

		#endregion

		#region Status

		[ResourceStringData("Enterprise.Customs.BR.Business.MessageSending|Status", Caption = "Status")]
		public ZString Status => Subscription.GP_PasswordStatus;

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(Schema.Status);

		#endregion

		#region ShouldSend

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				base.ShouldSend = value;
				if (!IsValidationSuspended)
				{
					ValidateMessageType();
				}
			}
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			MessageType = SubscriptionMessageTypesList.Codes.ORI;
		}

		protected void ValidateMessageType()
		{
			MessageTypeInfo.ClearAllNotifications();

			if (ShouldSend)
			{
				if ((MessageType == SubscriptionMessageTypesList.Codes.AMD || MessageType == SubscriptionMessageTypesList.Codes.CAN) && Subscription.GP_MailBoxID.IsEmpty)
				{
					MessageTypeInfo.AddError(Res.GetString("93886DC1-7646-4D29-BBDE-B1CCD98AB85D", "Unsubscribed events cannot be either amended or canceled."));
				}
				else if (Subscription.GP_StatusReason == StatusReasons.Subscribed && MessageType == EDIMessageSubTypeList.Codes.Original)
				{
					MessageTypeInfo.AddError(Res.GetString("76D0081E-A031-4797-AD38-7EFF8D486309", "Cannot subscribed to an already subscribed event."));
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateMessageType();
			base.RunPreSaveValidationCore();
		}

		public BusinessObject MessageAttachee => Subscription;

		public ZString GetMessageOwner() => ZString.Empty;

		public ZString GetMessageTypeForEDIMessage() => MessageTypeList.Codes.SUB;

		public ZString GetApplicationReference() => Subscription.GP_MailBoxID;

		public ZGuid GetGlbExternalPasswordPK() => BRGlbStaffWrapper.Get(Subscription.Staff)?.CCTPassword?.PK ?? ZGuid.Empty;

		public ZString GetMessageText() => new SubscriptionMessageBuilder(new NotificationSubscriptionProvider(this)).GetMessageText();
	}
}
