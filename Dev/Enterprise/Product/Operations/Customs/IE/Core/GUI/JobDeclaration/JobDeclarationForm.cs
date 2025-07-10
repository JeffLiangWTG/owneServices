using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class JobDeclarationForm : EU.GUI.JobDeclarationForm
	{
		public JobDeclarationForm() { }

		public JobDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();
	}
}
