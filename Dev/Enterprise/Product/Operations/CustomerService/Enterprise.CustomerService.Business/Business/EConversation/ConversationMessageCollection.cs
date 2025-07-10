using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;

namespace Enterprise.CustomerService.Business
{
	public class ConversationMessageCollection : NonPersistentBusinessObjectCollection<ConversationMessage>
	{
		readonly ConversationNoteCollection notes;

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ConversationMessageCollection(ConversationNoteCollection notes)
		{
			this.notes = notes;

			foreach (ConversationNote note in notes)
			{
				Add(new ConversationMessage(note));
			}

			HasChanges = notes.HasChanges;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ConversationMessage(notes.AddNew());
		}

		public ConversationMessage Add(ConversationNote note)
		{
			var message = new ConversationMessage(note);
			Add(message);
			return message;
		}
	}
}
