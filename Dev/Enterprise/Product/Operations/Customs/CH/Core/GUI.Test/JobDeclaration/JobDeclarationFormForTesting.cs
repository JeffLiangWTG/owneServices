using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

public class JobDeclarationFormForTesting : JobDeclarationForm
{
	public JobDeclarationFormForTesting(JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected override Customs.GUI.IEDIMenu GetNewTopLevelMenuCore() => EDIMenu;

	EDIMenuForTesting ediMenu;
	public EDIMenuForTesting EDIMenu => ediMenu ?? (ediMenu = new EDIMenuForTesting());
}
