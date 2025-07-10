using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class EntryMessageUserControl : CustomsEntryAndDiscardedMessagesUserControl
	{
		public EntryMessageUserControl()
			: this(null)
		{
		}

		public EntryMessageUserControl(BaseJobDeclaration declaration)
			: base(declaration)
		{
			RequiresMergeLabel.AllowOverlap(MessagesTabControl);
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl() => new MessageUserControl(JobDeclaration);
	}
}
