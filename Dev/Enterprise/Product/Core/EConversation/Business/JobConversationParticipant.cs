using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.EConversation.Business
{
	public class JobConversationParticipant : AutoJobConversationParticipant
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public JobConversationParticipant(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobConversationParticipantFetchStrategy(this);

		public bool IsEmailParticipant
		{
			get
			{
				return JCP_ParticipantTableCode == "";
			}
		}

		[BusinessObjectMaxLengthTestExclude]
		[BusinessObjectTestExclude]
		[List("Lookups.AvailableParentsList")]
		[ReadOnlyMember(nameof(ParentKey_ReadOnly))]
		public ZString ParentKey
		{
			get
			{
				if (IsEmailParticipant)
				{
					return Parent?.Name ?? "";
				}

				if (!parentCode.HasValue)
				{
					parentCode = Parent?.Code ?? ZString.Empty;
				}

				return parentCode.Value;
			}
			set
			{
				if (IsEmailParticipant)
				{
					throw new InvalidOperationException("This field should be read only right now.");
				}
				else
				{
					SetNonPersistentPropertyValue(ParentKeyInfo, ref parentCode, value);
				}
				UpdateParentIDFromParentKey();
				ParentKeyInfo.RefreshBinding();
			}
		}
		ZString? parentCode;

		protected bool ParentKey_ReadOnly => !CanBeModified || IsEmailParticipant;

		[List("Lookups.RelatedPartyTypesList", AllowOnlyTheseValues = true), ReadOnlyMember(nameof(IsInDatabase))]
		public ZString RelatedPartyTypeName
		{
			get
			{
				var result = Lookups.RelatedPartyTypesList[JCP_ParticipantTableCode, StringComparison.Ordinal]?.Description ?? ZString.Empty;
				if (result == ZString.Empty)
				{
					result = Lookups.RelatedPartyTypesList[JobConversationParticipantLookups.EmailConstant, StringComparison.Ordinal].Description;
				}
				return result;
			}
			set
			{
				var code = Lookups.RelatedPartyTypesList.GetCodeFromDescription(value);
				JCP_ParticipantTableCode = code == JobConversationParticipantLookups.EmailConstant ? "" : code;
				if (!IsInDatabase && Conversation != null && IsConversationIncidentRelated() && JCP_ParticipantTableCode == OrgHeaderSchema.Constants.Prefix)
				{
					JCP_IsSubscribed = false;
				}
				else
				{
					JCP_IsSubscribed = true;
				}
				UpdateParentIDFromParentKey();
				ParentKeyInfo.RefreshBinding();
			}
		}

		protected virtual bool IsConversationIncidentRelated() => Conversation.JCC_ParentTableCode == IncidentRequestSchema.Constants.Prefix;

		public override ZString JCP_ParticipantTableCode
		{ get => base.JCP_ParticipantTableCode;
			set
			{
				base.JCP_ParticipantTableCode = value;
				if (value != ZString.Empty)
				{
					JCP_EmailAddress = ZString.Empty;
				}
			}
		}

		[MaxLength(254)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString EmailAddress
		{
			get
			{
				return Parent?.Email ?? "";
			}
			set
			{
				JCP_EmailAddress = value;
				EmailAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EmailAddressInfo
		{
			get { return GetZPropertyInfo(nameof(EmailAddress)); }
		}

		protected bool EmailAddress_ReadOnly => !CanBeModified || !IsEmailParticipant;

		void UpdateParentIDFromParentKey()
		{
			if (IsEmailParticipant)
			{
				JCP_ParticipantID = ZGuid.Empty;
			}
			else
			{
				var parent = FindParentByKey(parentCode);
				if (parent != Parent || JCP_ParticipantID.IsEmpty)
				{
					JCP_ParticipantID = parent?.PK ?? ZGuid.Invalid;
				}
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateParentKey();
			}
		}

		public ZWrappedPropertyInfo RelatedPartyTypeNameInfo =>
			GetWrappedZPropertyInfo(nameof(RelatedPartyTypeName), sender => JCP_ParticipantTableCodeInfo);

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new DuplicateParticipantMerger(Conversation); }
		}

		public override ZGuid JCP_ParticipantID
		{
			get { return base.JCP_ParticipantID; }
			set
			{
				base.JCP_ParticipantID = value;

				var parent = Parent;
				if (parent != null && ParentKey != parent.Code)
				{
					ParentKey = parent.Code;
				}
			}
		}

		public ZPropertyInfo ParentKeyInfo
		{
			get { return GetZPropertyInfo(nameof(ParentKey)); }
		}

		public IConversationParticipant Parent
		{
			get
			{
				if (IsEmailParticipant)
				{
					return new EmailConversationParticipant(this.JCP_EmailAddress);
				}
				else if (!string.IsNullOrEmpty(JCP_ParticipantTableCode) && JCP_ParticipantID != ZGuid.Empty)
				{
					return (IConversationParticipant)Factory.Load(JCP_ParticipantTableCode, JCP_ParticipantID);
				}
				return null;
			}
		}

		BusinessObject FindParentByKey(string key)
		{
			var parentList = Lookups.AvailableParentsList as IFindBoxListProvider;

			return parentList?.GetBusinessObjectFromCodeWithoutFilter(key);
		}

		[RelatedBusinessObject("Conversation")]
		public override ZGuid JCP_JCC_Conversation
		{
			get { return base.JCP_JCC_Conversation; }
			set { base.JCP_JCC_Conversation = value; }
		}

		public override void OnSaving()
		{
			base.OnSaving();

			var wasSubscribed = (ZBool)JCP_IsSubscribedInfo.OriginalValue;
			var isNowSubscribed = JCP_IsSubscribed;

			if ((!IsInDatabase && isNowSubscribed) || wasSubscribed != isNowSubscribed)
			{
				var participant = Parent;
				var conversation = Conversation;

				if (participant != null && conversation != null)
				{
					var messageBody =
								!IsInDatabase ? ResString.GetMultilingualString("09368b20-22f1-4102-8e17-6e4bc767a330", "{0} has been added to the conversation.", participant.Name) :
								isNowSubscribed ? ResString.GetMultilingualString("f8359be7-e286-44a4-8dfc-c8161f32457a", "{0} has been subscribed to the conversation.", participant.Name) :
								ResString.GetMultilingualString("f2e11725-6f7b-4088-b796-b3f16445148f", "{0} has been unsubscribed from the conversation and will no longer receive notifications.", participant.Name);

					if (IsInDatabase && !isNowSubscribed)
					{
						conversation.UnsubscribedParticipants.Add(this.Parent);
					}

					conversation.Messages.AddNew(null, messageBody);

					AddParticipantChangedLogToParentConversation(participant, isNowSubscribed);
				}
			}
		}

		void AddParticipantChangedLogToParentConversation(IConversationParticipant participant, bool isSubscribed)
		{
			if (Conversation.Parent is IStmALogParent logParent)
			{
				var reference = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} has been {1}", participant.Name, isSubscribed ? (NoResString)"subscribed" : (NoResString)"unsubscribed");
				if (reference.Length > StmALogSchema.SL_Reference.MaxLength)
				{
					reference = reference.Substring(0, StmALogSchema.SL_Reference.MaxLength);
				}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				logParent.Logs.AddNew(Events.EditedARecord, reference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		public JobConversation Conversation => Factory.Load<JobConversation>(JCP_JCC_Conversation);

		bool CanBeModified => !(IsInDatabase || HasSentMessages);

		public override bool CanDelete => CanBeModified;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsInDatabase)
				{
					return ResString.GetMultilingualString("35a67304-84d0-48c1-9393-a2f37b6dc2cf", "You can not delete existing participants. You may un-subscribe them if you do not wish them to be notified of any further messages.");
				}
				else if (HasSentMessages)
				{
					return ResString.GetMultilingualString("21d4ca9d-cfce-4ddf-ad57-670c4d78c482", "You can not delete participants who have sent messages. You may un-subscribe them if you do not wish them to be notified of any further messages.");
				}

				return base.ReasonForNotAbleToDelete;
			}
		}

		bool HasSentMessages
		{
			get
			{
				return Factory.LoadTop1<JobConversationMessage>(new ZQuery(JobConversationMessageSchema.JCM_JCP_Participant, PK)) != null;
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			JCP_ParticipantTableCode = "GS";
			JCP_ParticipantID = Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>()).PK;
		}

#endif

		class DuplicateParticipantMerger : IUniqueIndexFailureHandler
		{
			readonly JobConversation parentConversation;
			BusinessObjectFactory Factory => parentConversation.Factory;

			public DuplicateParticipantMerger(JobConversation conversation)
			{
				this.parentConversation = conversation;
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return JobConversationParticipantSchema.Constants.Indexes.FK_UX__JCP_JCC_Conversation_JCP_ParticipantID_JCP_EmailAddress; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var newParticipants = Factory.GetChanges()
					.GetAddedObjects()
					.OfType<JobConversationParticipant>()
					.Where(participant => !participant.IsInDatabase);

				foreach (var participant in newParticipants)
				{
					MergeWithExistingDuplicate(participant);
				}

				MergeNewDuplicates(newParticipants);

				Factory.Save();

				parentConversation.Factory.ClearQueryCache();
				parentConversation.Participants.RefreshFromDb();
				parentConversation.Messages.RefreshFromDb();
			}

			void MergeNewDuplicates(IEnumerable<JobConversationParticipant> newParticipants)
			{
				foreach (var participant in newParticipants)
				{
					if (participant.IsDeleted)
					{
						continue;
					}

					var duplicates = newParticipants.Where(n =>
						!n.IsDeleted
						&& n.JCP_JCC_Conversation == participant.JCP_JCC_Conversation
						&& n.JCP_ParticipantID == participant.JCP_ParticipantID
						&& n.PK != participant.PK).ToArray();

					for (int i = 0; i < duplicates.Length; i++)
					{
						foreach (var message in parentConversation.Messages.Where(m => m.JCM_JCP_Participant == duplicates[i].PK))
						{
							message.JCM_JCP_Participant = participant.PK;
						}

						duplicates[i].Delete();
					}
				}
			}

			void MergeWithExistingDuplicate(JobConversationParticipant inMemoryParticipant)
			{
				var query = new ZDBOnlyQuery(typeof(JobConversationParticipant))
					.AddToFilter(JobConversationParticipantSchema.JCP_JCC_Conversation, inMemoryParticipant.JCP_JCC_Conversation)
					.AddToFilter(JobConversationParticipantSchema.JCP_ParticipantID, inMemoryParticipant.JCP_ParticipantID);

				var participantInDb = Factory.LoadTop1<JobConversationParticipant>(query);
				if (participantInDb != null)
				{
					foreach (var message in parentConversation.Messages)
					{
						if (message.JCM_JCP_Participant == inMemoryParticipant.PK)
						{
							message.JCM_JCP_Participant = participantInDb.PK;
						}
					}

					inMemoryParticipant.Delete();
				}
			}
		}

		public class EmailConversationParticipant : IConversationParticipant
		{
			public EmailConversationParticipant(string email)
			{
				this.email = email;
			}
			readonly string email;

			public string GetName()
			{
				return email?.Split('@')[0];
			}

			ZString IConversationParticipant.Code => GetName();

			ZString IConversationParticipant.Language => ZString.Empty;

			ZString IConversationParticipant.Name => GetName();

			ZString IConversationParticipant.Location => ZString.Empty;

			ZString IConversationParticipant.OrganisationName => ZString.Empty;

			ZString IConversationParticipant.JobTitle => ZString.Empty;

			ZBool IConversationParticipant.IsActive => true;

			ZBool IConversationParticipant.IsInternal => false;

			ZString IConversationParticipant.Email => email;

			ZString IConversationParticipant.DisplayText => GetName();

			void IConversationParticipant.CheckCanParticipate(INotifications notifications)
			{
				return;
			}
		}
	}

	class JobConversationParticipantFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		new JobConversationParticipant BusinessObject => (JobConversationParticipant)base.BusinessObject;

		public JobConversationParticipantFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			var bizo = BusinessObject;
			var factory = bizo.Factory;

			var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(bizo.JCP_ParticipantTableCode, reportUnknownPrefix: false);

			if (!(type is null || type.IsAbstract))
			{
				factory.AddFetchHint(type, bizo.JCP_ParticipantID);
			}
		}
	}
}
