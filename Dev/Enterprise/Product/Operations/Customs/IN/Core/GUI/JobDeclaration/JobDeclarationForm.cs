using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.GUI;

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
