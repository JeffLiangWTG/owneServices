using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class EDIMessageMainUserControl : EDIMessageStandAloneUserControl
	{
		public EDIMessageMainUserControl()
		{
			InitializeComponent();
			EditForm();
		}

		void EditForm()
		{
			Controls.Remove(messageContentsPanel);

			var message = this.FindSingleOrDefault<ZGroupBox>("messageDetailsGroupBox");
			if (message != null)
			{
				message.Dock = System.Windows.Forms.DockStyle.Left;
				message.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
				message.Controls.Remove(messageSubTypeTextBox);
			}

			processingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			processingDetailsGroupBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			processingDetailsGroupBox.Controls.Remove(applicationReferenceTextBox);

			var interchange = this.FindSingleOrDefault<ZGroupBox>("interchangeDetailsGroupBox");
			if (interchange != null)
			{
				interchange.Dock = System.Windows.Forms.DockStyle.Fill;
			}
		}
	}
}
