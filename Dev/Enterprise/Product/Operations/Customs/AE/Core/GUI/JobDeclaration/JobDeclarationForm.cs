using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AE.GUI;

public partial class JobDeclarationForm : BaseJobDeclarationForm
{
	public JobDeclarationForm()
		: base()
	{
	}

	public JobDeclarationForm(JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitializeComponent();
		base.InitialiseForm();
	}

	protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

	protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();

	public override int RoutingTabIndex => 2;
}

