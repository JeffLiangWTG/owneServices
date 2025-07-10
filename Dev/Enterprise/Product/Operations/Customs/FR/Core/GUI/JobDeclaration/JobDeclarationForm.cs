using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class JobDeclarationForm : EU.GUI.JobDeclarationForm
	{
		public JobDeclarationForm()
			: base()
		{
		}

		public JobDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();
	}
}
