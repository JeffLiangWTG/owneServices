using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;

namespace Enterprise.EConversation.Business
{
	public class JobConversation : AutoJobConversation, IConversation, ICustomTextTemplateContext
	{
		public JobConversation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobConversationFetchStrategy(this);

		#region Business Object Overrides

		protected override void OnFactorySaving()
		{
			var updatedParent = false;

			if (Parent != null
				&& Parent.HasChanges
				&& Parent is IConversationProvider provider
				&& provider.SendEmailNotificationsOnSave)
			{
				provider.RunConversationUpdateActionBeforeSaving();
				updatedParent = true;
			}

			PerformBroadcast();

			base.OnFactorySaving();

			if (HasChanges && updatedParent)
			{
				EConversationEmailBuilder.GenerateAndQueueEmailNotifications(this);
			}
		}

		void PerformBroadcast()
		{
			var messagesToBroadcast = Messages.Where(m => m.HasChanges && m.JCM_IsBroadcast).ToList();
			var relatedItemProvider = GetRelatedItemProvider();

			if (messagesToBroadcast.Count == 0 || relatedItemProvider == null)
			{
				return;
			}

			var broadcastRecipientConversations = relatedItemProvider.RelatedItems
				.OfType<IConversationBroadcastRecipient>()
				.ToDictionary(c => c.EConversation);

			foreach (var conversation in broadcastRecipientConversations)
			{
				conversation.Key.Messages.CopyFromBroadcast(messagesToBroadcast);
				conversation.Value.GenerateAndSendBroadcastEmailNotifications();
			}
		}

		protected virtual IWorkTaskRelatedItemProvider GetRelatedItemProvider()
		{
			if (Parent is IWorkTaskRelatedItemProvider relatedItemProvider)
			{
				return relatedItemProvider;
			}

			return null;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			Messages.ClearBroadcastBuffer();
		}

		#endregion

		#region Related Business Objects

		[ChildEditable]
		public JobConversationMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new JobConversationMessageCollection(Factory, this);
					RegisterEditableChildObject(messages);
				}

				return messages;
			}
		}
		JobConversationMessageCollection messages;

		public JobConversationMessage AddMessageFromCurrentUser(string body, bool isInternal, bool isSystem = false, bool isBroadcast = false)
		{
			var participant = Participants.GetOrAdd((IConversationParticipant)EnvProxy.Instance.CurrentUser);

			return Messages.AddNew(participant, body, isInternal, isSystem, isBroadcast);
		}

		public BusinessObject Parent => Factory.Load(JCC_ParentTableCode, JCC_ParentID);

		public List<IConversationParticipant> UnsubscribedParticipants = new List<IConversationParticipant>();

		public ZBool HideStaffName { get; set; } = false;

		#endregion

		#region Delete

		public override bool CanDelete => false;

		public override void Delete()
		{
			Messages.DeleteAll();
			Participants.DeleteAll();

			base.Delete();
		}

		#endregion

		#region Participant Views

		JobConversationParticipantCollection participants;
		JobConversationParticipantCollection _staff;
		JobConversationParticipantCollection _group;
		JobConversationParticipantCollection _relatedParties;

		[ChildEditable]
		public JobConversationParticipantCollection Participants
			=> participants ?? (participants = CreateParticipantsCollectionForTables());

		[ChildEditable]
		public JobConversationParticipantCollection Staff
			=> _staff ?? (_staff = CreateParticipantsCollectionForTables(GlbStaffSchema.Constants.Prefix));

		[ChildEditable]
		public JobConversationParticipantCollection Groups
			=> _group ?? (_group = CreateParticipantsCollectionForTables(GlbGroupSchema.Constants.Prefix));

		[ChildEditable]
		public JobConversationParticipantCollection RelatedParties
			=> _relatedParties ?? (_relatedParties = CreateParticipantsCollectionForTables(OrgContactSchema.Constants.Prefix, OrgHeaderSchema.Constants.Prefix, ""));

		JobConversationParticipantCollection CreateParticipantsCollectionForTables(params string[] tableCodes)
		{
			var collection = tableCodes.Length > 0 ?
				new JobConversationParticipantCollection(Factory, this, tableCodes) :
				new JobConversationParticipantCollection(Factory, this);

			RegisterEditableChildObject(collection);

			return collection;
		}

		#endregion

		#region GetOrCreate

		public static JobConversation GetOrCreate(BusinessObject parent)
		{
			if (!parent.IsInDatabase)
			{
				throw new ArgumentException("Parent must be saved to create a conversation");
			}

			return GetConversation(parent) ?? CreateConversation(parent);
		}

		static JobConversation CreateConversation(BusinessObject parent, int retries = 3)
		{
			SqlApplicationLock mutex;
			if (Db.Connection.TryGetLock("CreatingNewConversation" + parent.PK, out mutex))
			{
				using (mutex)
				{
					var result = GetConversation(parent);

					if (result == null)
					{
						var pk = CreateAndSaveNewConversationOnNewFactory(parent);
						result = parent.Factory.Load<JobConversation>(pk);
					}

					return result;
				}
			}
			else if (retries > 0)
			{
				Thread.Sleep(250);
				return CreateConversation(parent, retries - 1);
			}
			else
			{
				throw new InvalidOperationException(SqlLockExceptionMessage);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message only seen by developers")]
		public static string SqlLockExceptionMessage => "SQL Lock for new conversation could not be created";

		public static ZGuid CreateAndSaveNewConversationOnNewFactory(BusinessObject parent, string defaultMessage = null)
		{
			var factory = new BusinessObjectFactory();
			var conversation = CreateWithoutCheckingForExistingConversation(parent, factory);
			if (!string.IsNullOrEmpty(defaultMessage))
			{
				conversation.Messages.AddNew(null, defaultMessage, isSystem: true);
			}

			using (FactorySaveAlerterOverride.TemporarilyOverride(conversation))
			{
				factory.Save();
			}

			return conversation.PK;
		}

		public static JobConversation CreateWithoutCheckingForExistingConversation(BusinessObject parent, BusinessObjectFactory factory)
		{
			var conversation = factory.New<JobConversation>();
			conversation.JCC_ParentID = parent.PK;
			conversation.JCC_ParentTableCode = parent.TablePrefix;

			return conversation;
		}

		public static JobConversation GetConversation(BusinessObject parent)
		{
			var query = new ZDBOnlyQuery(typeof(JobConversation));
			query.AddToFilter(JobConversationSchema.JCC_ParentID, parent.PK);
			query.ReLoadExistingRows = true;
			return parent.Factory.LoadTop1<JobConversation>(query);
		}

		#endregion

		#region For display

		public bool IsEmpty => !Messages.Any();

		public bool AnyLocalMessageContains(string text)
		{
			return Messages.Any(msg => msg.JCM_IsLocal && msg.Body.Contains(text, StringComparison.OrdinalIgnoreCase));
		}

		public IList<IConversationMessage> GetTimeOrderedMessages()
		{
			return Messages
				.Cast<IConversationMessage>()
				.OrderBy(message => -message.SystemCreateTimeInUtc.Ticks)
				.ToList();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZBlob NextMessage { get; set; }

		public ZBlob NextMessage_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(NextMessage);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				NextMessage = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZString NextMessagePlainText { get; set; }

		#endregion

		public string GetTextTemplateContextID(object dataSource, CargoWise.ComponentModel.KBindingMemberInfo bindingMemberInfo)
		{
			if (bindingMemberInfo.BindingField.Equals(nameof(NextMessagePlainText)) || bindingMemberInfo.BindingField.Equals(nameof(NextMessage)))
			{
				return this.TableName + "." + nameof(NextMessage);
			}
			else
			{
				return this.TableName + "." + bindingMemberInfo.BindingField;
			}
		}

		public BusinessObject[] GetTextTemplateContextBusinessObject(object dataSource, CargoWise.ComponentModel.KBindingMemberInfo bindingMemberInfo)
		{
			return Parent != null && Parent is BusinessObject ? new BusinessObject[] { this, Parent } : new BusinessObject[] { this };
		}

		#region For Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			JCC_ParentTableCode = DummyBusinessObject.Schema.TablePrefix;
			JCC_ParentID = Guid.NewGuid();
		}

#endif
		#endregion
	}

	class JobConversationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		new JobConversation BusinessObject => (JobConversation)base.BusinessObject;

		public JobConversationFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			var bizo = BusinessObject;
			var factory = bizo.Factory;

			factory.AddFetchHint(JobConversationMessageSchema.Instance, bizo.Messages.CompleteFilter);
			factory.AddFetchHint(JobConversationParticipantSchema.Instance, bizo.Participants.CompleteFilter);
		}
	}
}
