using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS
{
	public interface IDeclarationWizardManager
	{
		DeclarationWizard DeclarationWizard { get; }
		JobDeclaration Declaration { get; }
		void PopulateDeclaration();
		void NextQuestion();
		void PreviousQuestion();
		void FinishWizard();
	}

	public class DeclarationWizardManager : IDeclarationWizardManager
	{
		public DeclarationWizardManager(JobDeclaration declaration)
		{
			Declaration = declaration;
			DeclarationWizard = new DeclarationWizard(new BusinessObjectFactory());
			DeclarationWizard.IsMultiInstructionDeclaration = declaration.CustomsEntryInstructions.Count > 1;
		}

		public JobDeclaration Declaration { get; private set; }
		public DeclarationWizard DeclarationWizard { get; private set; }

		public void NextQuestion()
		{
			DeclarationWizard.NextQuestion();
		}

		public void PreviousQuestion()
		{
			DeclarationWizard.PreviousQuestion();
		}

		public void FinishWizard()
		{
			DeclarationWizard.FinishWizard();
		}

		public void PopulateDeclaration()
		{
			DeclarationWizard.PopulateDeclaration(Declaration);
		}
	}
}
