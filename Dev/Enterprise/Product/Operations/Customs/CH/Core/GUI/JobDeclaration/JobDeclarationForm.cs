using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CH.GUI;

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
}
