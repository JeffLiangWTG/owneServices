using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Wizards
{
	public static class DeclarationWizardGuiHelper
	{
		public static void WizardClicked(JobDeclaration declaration)
		{
			var decWizardManager = new DeclarationWizardManager(declaration);
			ZFormModaliser.ShowDialogAndDispose(new JobDeclarationWizardForm(decWizardManager));
		}
	}
}
