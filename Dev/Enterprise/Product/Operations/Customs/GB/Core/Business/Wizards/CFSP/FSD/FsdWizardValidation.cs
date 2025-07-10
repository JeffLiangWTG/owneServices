using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business.Wizards.CFSP
{
	public class FsdWizardValidation : AutoFsdWizardValidation
	{
		public FsdWizardValidation(AutoFsdWizard parent)
			: base(parent) { }

		protected override void CheckConsignee()
		{
			base.CheckConsignee();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ConsigneeInfo, Res.GetString("05876c36-9bee-4312-b3df-8e4f4f15fdad", "Consignee"));
		}

		protected override void CheckProcedure()
		{
			MandatoryValidation.CheckEntered(Parent.ProcedureInfo, Res.GetString("ba630b36-cdb9-4e63-92df-38e0e9d7806e", "Procedure"));
		}
	}
}
