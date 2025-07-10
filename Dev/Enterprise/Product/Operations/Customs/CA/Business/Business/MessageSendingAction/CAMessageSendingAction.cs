using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAMessageSendingAction : AutoCAMessageSendingAction
	{
		public new class Schema : AutoCAMessageSendingAction.Schema
		{
			public const string CA_MessageContents = "CA_MessageContents";
			public const string CA_MessageDescription = "CA_MessageDescription";
		}

		public CAMessageSendingAction(CusEntryHeader entry, MessageType messageType, CAMessageSendingActionCollection actions)
			: base(actions.Factory)
		{
			this.entry = entry;
			this.messageType = messageType;
			this.actions = actions;
		}

		#region Bindable Properties

		public ZString CA_MessageDescription
		{
			get { return MessageManager.MessageFriendlyName; }
		}

		public ZPropertyInfo CA_MessageDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CA_MessageDescription); }
		}

		public ZString CA_MessageContents
		{
			get
			{
				if (!cA_MessageContentsCached.HasValue)
				{
					cA_MessageContentsCached = GetMessageContents();
				}
				return cA_MessageContentsCached.Value;
			}
		}
		ZString? cA_MessageContentsCached;

		public ZPropertyInfo CA_MessageContentsInfo
		{
			get { return GetZPropertyInfo(Schema.CA_MessageContents); }
		}

		public override ZBool CA_SaveWithoutSending
		{
			get { return base.CA_SaveWithoutSending; }
			set
			{
				base.CA_SaveWithoutSending = value;
				if (value)
				{
					CA_SendMessage = false;
				}
			}
		}

		[ReadOnlyMember(nameof(CA_SaveWithoutSendingReasonText_ReadOnly))]
		public override ZString CA_SaveWithoutSendingReasonText
		{
			get => base.CA_SaveWithoutSendingReasonText;
			set => base.CA_SaveWithoutSendingReasonText = value;
		}

		protected bool CA_SaveWithoutSendingReasonText_ReadOnly
		{
			get { return !CA_SaveWithoutSending; }
		}

		public override ZBool CA_SendMessage
		{
			get { return base.CA_SendMessage; }
			set
			{
				base.CA_SendMessage = value;
				if (value)
				{
					CA_SaveWithoutSending = false;
				}
			}
		}

		#endregion

		#region Implementation
		internal readonly MessageType messageType;
		internal readonly CusEntryHeader entry;
		internal readonly CAMessageSendingActionCollection actions;

		internal void ProcessWhenSavedWithoutSending()
		{
			if (CA_SaveWithoutSending)
			{
				new Customs.Business.OutstandingAmendmentLogManager(entry).AddANewOutstandingAmendmentLog(CA_SaveWithoutSendingReasonText);
			}
		}

		internal DataLoadingModuleMessageManager MessageManager
		{
			get { return messageManager ?? (messageManager = new DataLoadingModuleMessageManager(entry, this)); }
		}
		DataLoadingModuleMessageManager messageManager;

		ZString GetMessageContents()
		{
			ZString result = Res.GetString("7a9d04ed-753a-4317-93a3-842b8628b06d", "This feature is not supported yet.");
			DataLoadingModuleMessageBuilder builder = MessageManager.GetMessageBuilder(entry, actions.messageSendingMessageType);
			if (builder != null)
			{
				result = builder.GetHumanFriendlyMessageText();
			}
			return result;
		}
		#endregion Implementation
	}
}
