//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConversationParticipantValidation
//
//    This class should be used for overriding validation in AutoJobConversationParticipantValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.EConversation.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;

	public class JobConversationParticipantValidation : AutoJobConversationParticipantValidation
	{
		public JobConversationParticipantValidation(AutoJobConversationParticipant parent) : base(parent)
		{
		}

		new JobConversationParticipant Parent => (JobConversationParticipant)base.Parent;

		protected override void CheckJCP_ParticipantID()
		{
			if (Parent.IsEmailParticipant) { return; }

			base.CheckJCP_ParticipantID();
		}

		protected virtual void CheckParentKey()
		{
			if (Parent.IsEmailParticipant) { return; }

			if (Parent.Parent == null)
			{
				Parent.ParentKeyInfo.AddError(ListValidation.InvalidCodeMessage.ToString());
			}
			else if (IsAlreadyInCollection)
			{
				Parent.ParentKeyInfo.AddWarning(Res.GetString("442604c7-e034-4c9d-9434-0ae688f9f816", "This participant has already been added"));
			}
			else if (!Parent.Parent.IsActive)
			{
				Parent.ParentKeyInfo.AddWarning(Res.GetString("e1119179-70c2-4142-942b-51f72ea5dfd6", "This participant is inactive"));
			}
			else if (!Parent.IsInDatabase)
			{
				Parent.Parent.CheckCanParticipate(Parent.ParentKeyInfo);
			}
		}

		protected virtual void CheckEmailAddress()
		{
			if (!Parent.IsEmailParticipant) { return; }

			if (Parent.EmailAddress.IsEmpty || !EmailAddressValidation.IsEmailAddressValid(Parent.EmailAddress))
			{
				Parent.EmailAddressInfo.AddError(Res.GetString("782d42a7-3a93-4acc-ad4a-e750d76a9332", "Enter a valid Email Address."));
			}
			else if (EmailIsAlreadyInCollection && !Parent.IsInDatabase)
			{
				Parent.EmailAddressInfo.AddError(Res.GetString("442604c7-e034-4c9d-9434-0ae688f9f816", "This participant has already been added"));
			}
		}

		bool EmailIsAlreadyInCollection =>
			Parent
				.Conversation?
				.Participants
				.Any(participant => participant != Parent && participant.EmailAddress.Trim().ToUpperInvariant() == Parent.EmailAddress.Trim().ToUpperInvariant()) ?? false;

		bool IsAlreadyInCollection =>
			Parent
				.Conversation?
				.Participants
				.Any(participant => participant != Parent && participant.JCP_ParticipantID == Parent.JCP_ParticipantID) ?? false;

		public void ValidateParentKey()
		{
			ValidateCalculatedProperty(Parent.ParentKeyInfo);
		}

		public void ValidateEmailAddress()
		{
			ValidateCalculatedProperty(Parent.EmailAddressInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateParentKey();
			ValidateEmailAddress();
		}
	}
}
