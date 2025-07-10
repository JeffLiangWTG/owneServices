using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZDynamicControlCreationUserControl
	{
		void InitializeComponent()
		{
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ZDynamicControlCreationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.DoubleBuffered = true;
			this.Name = "ZDynamicControlCreationUserControl";
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

	}
}
