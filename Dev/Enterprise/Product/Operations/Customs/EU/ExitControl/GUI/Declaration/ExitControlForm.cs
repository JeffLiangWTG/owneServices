using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public partial class ExitControlForm : ZTemplateForm
{
	public ExitControlForm(CusExitHeader exitHeader) : base(exitHeader)
	{
		InitializeComponent();
		PlugIns.Add(ControllerIDs.DocDataPlugIn);
		AddExitControlMenu();
		ExitControlUserControl.SetExitControlMenuItem(ExitControlMenuItem);
		WorkflowTabPage.Initialize(exitHeader);
	}

	protected override bool ShowAuditTab => true;
	public override string FormCaption => ExitHeader.HumanReadableName;

	void AddExitControlMenu()
	{
		var menuItems = MainMenu.MenuItems;
		_ = menuItems.Add(menuItems.IndexOf(HelpMenuItem), ExitControlMenuItem);
	}

	void WorkflowTabPage_InitializeTab(object sender, System.EventArgs e)
	{
		this.WorkflowTabPage.SuspendLayout();
		this.WorkflowTabPage.ResumeLayout(false);
		this.WorkflowTabPage.PerformLayout();
	}

	ExitControlMenuItem ExitControlMenuItem => exitControlMenuItem ??= new ExitControlMenuItem { ExitHeader = ExitHeader };
	ExitControlMenuItem exitControlMenuItem;

	CusExitHeader ExitHeader => (CusExitHeader)BusinessEntity;
}
