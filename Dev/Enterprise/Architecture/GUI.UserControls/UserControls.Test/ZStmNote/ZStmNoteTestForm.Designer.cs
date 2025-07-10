using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed partial class ZStmNoteTestForm
	{
		ZTemplateTabControl TabControl;
		internal ZStmNoteTestTabPage TabPage;

		protected override void InitializeComponent()
		{
			TabControl = new ZTemplateTabControl();
			TabPage = new ZStmNoteTestTabPage();

			TabControl.TabPages.Add(TabPage);
			Controls.Add(TabControl);
		}

	}
}
