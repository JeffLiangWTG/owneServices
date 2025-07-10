using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class JobDeclarationForm : EU.GUI.JobDeclarationForm
{
	public JobDeclarationForm() { }

	public JobDeclarationForm(JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected override IEDIMenu GetNewTopLevelMenuCore()
	{
		return new EDIMenu();
	}

	protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl()
	{
		return new CustomsBrokerageUserControl();
	}
}
