using System.Drawing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public partial class LoadingSectionControl : ZUserControl
	{
		public LoadingSectionControl()
		{
			InitializeComponent();

			loadingIndicatorLabel.Font = new Font("Arial", 12F, FontStyle.Bold);
		}

		public string LoadingText
		{
			get => loadingIndicatorLabel.Text;
			set => loadingIndicatorLabel.Text = value;
		}
	}
}
