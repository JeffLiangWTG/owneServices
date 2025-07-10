using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class CustomJobConversationMessage : NonPersistentBusinessObject
	{
		public ZBool IsChecked
		{
			get => isChecked;
			set
			{
				SetNonPersistentPropertyValue(IsCheckedInfo, ref isChecked, value);
			}
		}
		ZBool isChecked;

		public virtual ZPropertyInfo IsCheckedInfo
		{
			get { return GetZPropertyInfo(nameof(IsChecked)); }
		}

		public ZDateTime PostedTimeUtc { get; private set; }
		public ZString Body { get; private set; }
		public ZBool IsInternal { get; private set; }
		public ZString WorkItemNumber { get; private set; }
		public JobConversationParticipant Sender { get; private set; }
		public bool IsSystemMessage { get; private set; }

		public static class Schema
		{
			public const string TableName = "JobConversationMessage";
		}

		public CustomJobConversationMessage(JobConversationMessage message)
			: base()
		{
			PopulateProperties(message);
		}

		public CustomJobConversationMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		void PopulateProperties(JobConversationMessage message)
		{
			if (message != null)
			{
				PostedTimeUtc = message.JCM_PostedTimeUtc;
				Body = message.JCM_Body;
				IsInternal = message.JCM_IsInternal;
				WorkItemNumber = message.Conversation?.Parent is IWorkItem wi ? wi.WKI_WorkItemNumber : ZString.Empty;
				Sender = message.Sender;
				IsSystemMessage = message.IsSystemMessage;
			}
		}
	}
}
