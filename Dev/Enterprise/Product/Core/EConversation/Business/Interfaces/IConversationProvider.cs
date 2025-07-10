using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.EConversation.Business
{
	public interface IConversationProvider
	{
		JobConversation eConversation { get; }
		ModuleIdentifier ParentModule { get; }
		ControllerID ParentController { get; }
		IEnumerable<RelatedParty> AdditionalParticipants { get; }
		bool SendEmailNotificationsOnSave { get; }
		void RunConversationUpdateActionBeforeSaving();
		string EmailSubjectContentOverride { get; }
		string FromAddressOverride { get; }
		NotificationEmailTemplate NotificationEmailTemplateOverride { get; }
	}

	public class RelatedParty
	{
		public IMultilingualString Relation { get; }
		public IConversationParticipant Participant { get; }

		public RelatedParty(IConversationParticipant participant, IMultilingualString relation = null)
		{
			Participant = participant;
			Relation = relation;
		}
	}
}
