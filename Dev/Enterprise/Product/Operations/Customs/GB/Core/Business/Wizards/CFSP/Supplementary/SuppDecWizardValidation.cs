

using CargoWise.EntityFramework;
namespace Enterprise.Customs.GB.Business.Wizards.CFSP
{
	public class SuppDecWizardValidation : AutoSuppDecWizardValidation
	{
		public SuppDecWizardValidation(AutoSuppDecWizard parent)
			: base(parent) { }

		protected override void CheckDeclarationType()
		{
			base.CheckDeclarationType();
			MandatoryValidation.CheckEntered(Parent.DeclarationTypeInfo);
		}

		protected override void CheckNumberPackagesToDeclare()
		{
			base.CheckNumberPackagesToDeclare();
			MandatoryValidation.CheckNotZero(Parent.NumberPackagesToDeclareInfo);
		}

		protected override void CheckSupplementaryProcedure()
		{
			base.CheckSupplementaryProcedure();
			MandatoryValidation.CheckEntered(Parent.SupplementaryProcedureInfo);
		}
	}
}
