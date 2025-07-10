using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class EntryMessageUserControl : EU.GUI.EntryMessageUserControl
{
	public EntryMessageUserControl()
	{
		InitializeComponent();
	}

	protected override BaseCustomsEntryUserControl GetMessageUserControl() => new MessageUserControl();
}
