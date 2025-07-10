using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business
{
	public class EDIMessageInterpreterValidation : AutoEDIMessageInterpreterValidation
	{
		public EDIMessageInterpreterValidation(AutoEDIMessageInterpreter parent)
			: base(parent)
		{
		}

		protected new EDIMessageInterpreter Parent => (EDIMessageInterpreter)base.Parent;

		protected override void CheckApplicationCode()
		{
			base.CheckApplicationCode();
			MandatoryValidation.CheckEntered(Parent.ApplicationCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ApplicationCodeInfo, Parent.ApplicationCodes);
		}
	}
}
