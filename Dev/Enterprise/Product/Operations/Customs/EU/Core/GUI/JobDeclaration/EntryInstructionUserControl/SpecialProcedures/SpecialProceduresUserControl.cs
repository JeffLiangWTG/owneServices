using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class SpecialProceduresUserControl : ZUserControl
	{
		public SpecialProceduresUserControl()
		{
			InitializeComponent();
			UpdateDynamicSpecialProceduresPanel();
		}

		void UpdateDynamicSpecialProceduresPanel()
		{
			var layout = GetSpecialProceduresLayout();
			if (layout != null)
			{
				DynamicSpecialProceduresPanel.UpdateLayout(layout);
			}
		}

		protected virtual IPanelLayoutProvider GetSpecialProceduresLayout() => new SpecialProceduresLayout();
	}
}
