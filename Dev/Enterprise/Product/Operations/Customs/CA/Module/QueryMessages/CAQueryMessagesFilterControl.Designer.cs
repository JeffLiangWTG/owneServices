using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.Module
{
	public partial class CAQueryMessagesFilterControl
	{
		/// <summary>
		/// Required method for Designer support - do not modifyB
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EDIMessage);
			// 
			// CAQueryMessagesFilterControl
			// 
			this.Name = "CAQueryMessagesFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
