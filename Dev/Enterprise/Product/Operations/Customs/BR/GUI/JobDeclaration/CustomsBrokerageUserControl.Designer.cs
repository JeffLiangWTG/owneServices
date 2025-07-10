using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.BR.GUI
{
	public partial class CustomsBrokerageUserControl
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			//components = new System.ComponentModel.Container();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// CustomsBrokerageUserControl
			//
			this.Name = "CustomsBrokerageUserControl";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		readonly System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
