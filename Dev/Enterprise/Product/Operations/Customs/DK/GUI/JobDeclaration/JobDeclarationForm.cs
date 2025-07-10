using Enterprise.Customs.DK.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.DK.GUI
{
	public partial class JobDeclarationForm : EU.GUI.JobDeclarationForm
	{
		public JobDeclarationForm()
		{
		}

		public JobDeclarationForm(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();
	}
}
