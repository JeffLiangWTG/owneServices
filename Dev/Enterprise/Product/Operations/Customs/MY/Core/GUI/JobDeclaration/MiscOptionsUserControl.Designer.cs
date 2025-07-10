using Enterprise.Customs.GUI;

namespace Enterprise.Customs.MY.GUI
{
	public partial class MiscOptionsUserControl : BaseMiscOptionsUserControl
	{
		void InitializeComponent()
		{
			this.MiscOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MiscOptionsUserControl
			// 
			this.Name = "MiscOptionsUserControl";
			this.MiscOptionsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
