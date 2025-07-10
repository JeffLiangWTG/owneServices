using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class RequestedDocumentsUserControl : ZUserControl
	{
		public RequestedDocumentsUserControl()
		{
			InitializeComponent();
			CustomizeLayout();
		}

		void CustomizeLayout()
		{
			RequestedDocumentsGrid.SuspendLayout();
			SuspendLayout();

			CustomizeLayoutCore();

			RequestedDocumentsGrid.ResumeLayout(false);
			RequestedDocumentsGrid.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		protected virtual void CustomizeLayoutCore()
		{
		}
	}
}
