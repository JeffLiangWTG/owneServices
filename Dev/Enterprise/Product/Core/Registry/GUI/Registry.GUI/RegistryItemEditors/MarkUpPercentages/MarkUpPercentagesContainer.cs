using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class MarkUpPercentagesContainer : ZUserControl
	{
		public MarkUpPercentagesContainer()
		{
			InitializeComponent();
		}

		bool fReadOnly;
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				fReadOnly = value;
				MarkUpGrid.ReadOnly = value;
			}
		}
	}
}
