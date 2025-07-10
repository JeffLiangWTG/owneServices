using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class EDIMessageModificationUserControl : ZUserControl
	{
		public EDIMessageModificationUserControl()
		{
			InitializeComponent();
			MessageSplitContainer.Panel2MinSize = 200;
		}
	}
}
