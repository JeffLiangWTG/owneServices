using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI;

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

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();

	protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

	protected override IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategies()
	{
		var preSaveStrategies = new List<PreSaveDialogStrategy>(base.GetPreSaveDialogStrategies());

		var declaration = Declaration;
		if (declaration != null)
		{
			preSaveStrategies.Add(new DeclarationOfIntentPreSaveDialogStrategy(declaration.DeclarationOfIntentRefresher));
		}
		return preSaveStrategies;
	}
}
