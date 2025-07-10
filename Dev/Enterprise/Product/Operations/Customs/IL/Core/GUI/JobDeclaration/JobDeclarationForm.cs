using Enterprise.Customs.GUI;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.GUI
{
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
}
