using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.EConversation.Testing
{
	class DummyConversationProvider : DummyBusinessObject, IConversationProvider, IStmALogParent
	{
		public DummyConversationProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public ICollection<RelatedParty> AdditionalParticipants { get; } = new List<RelatedParty>();
		IEnumerable<RelatedParty> IConversationProvider.AdditionalParticipants => AdditionalParticipants;

		public void AddRelatedParticipant(IConversationParticipant participant, string relation = null)
		{
			AdditionalParticipants.Add(new RelatedParty(participant, (NoResString)relation));
		}

		JobConversation conversation;
		public virtual JobConversation eConversation
		{
			get
			{
				if (conversation == null)
				{
					conversation = JobConversation.GetOrCreate(this);
					RegisterEditableChildObject(conversation);
				}
				return conversation;
			}
		}

		public ModuleIdentifier ParentModule => DummyModuleIDs.Dummy;
		public ControllerID ParentController => DummyControllerIDs.Dummy;

		public ZGuid LogsParentPK => PK;
		public string LogsParentTableName => TableName;

		Logs _logs;
		public Logs Logs => _logs ?? (_logs = new Logs(this));
		public BusinessObjectFactory LogsFactory => Factory;
		public BusinessObject[] BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();
		public bool DeferFiringWorkflow => true;
		public void ProcessLog(IStmALog log) { }

		public static DummyConversationProvider CreateConversationWithSubscribers(params BusinessObject[] subscribers)
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.NewWithValidTestData<DummyConversationProvider>();
			factory.Save();

			var onTestFactory = subscribers[0].Factory.Load<DummyConversationProvider>(bizo.PK);
			foreach (var sub in subscribers)
			{
				onTestFactory.eConversation.Participants.AddNewParticipant((IConversationParticipant)sub);
			}

			return onTestFactory;
		}

		bool IConversationProvider.SendEmailNotificationsOnSave => sendEmailNotificationsOnSave;
		bool sendEmailNotificationsOnSave;

		public void SetSendEmailNotificationsOnSave(bool sendOnSave)
		{
			sendEmailNotificationsOnSave = sendOnSave;
		}

		void IConversationProvider.RunConversationUpdateActionBeforeSaving()
		{
			updateConversationAction?.Invoke(this);
		}
		Action<IConversationProvider> updateConversationAction;

		public void AssignUpdateOnSavingAction(Action<IConversationProvider> actionToAssign)
		{
			updateConversationAction = actionToAssign;
		}

		string IConversationProvider.EmailSubjectContentOverride => emailSubjectContentOverride;
		string emailSubjectContentOverride;

		public void SetEmailSubjectContentOverride(string value)
		{
			emailSubjectContentOverride = value;
		}

		string IConversationProvider.FromAddressOverride => fromAddressOverride;
		string fromAddressOverride;

		public void SetFromAddressOverride(string value)
		{
			fromAddressOverride = value;
		}

		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => notificationEmailTemplateOverride;
		NotificationEmailTemplate notificationEmailTemplateOverride;

		public void SetNotificationEmailTemplateOverride(NotificationEmailTemplate value)
		{
			notificationEmailTemplateOverride = value;
		}
	}
}
