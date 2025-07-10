//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusExitItemValidation
//
//    This class should be used for overriding validation in AutoCusExitItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitItemValidation : AutoCusExitItemValidation
	{
		public CusExitItemValidation(AutoCusExitItem parent) : base(parent)
		{
		}

		protected override void CheckCXI_GrossMassUQ()
		{
			base.CheckCXI_GrossMassUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.CXI_GrossMassUQInfo);

			if (Parent.CXI_GrossMass != 0m)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CXI_GrossMassUQInfo);
			}
		}

		protected override void CheckCXI_NetMassUQ()
		{
			base.CheckCXI_NetMassUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.CXI_NetMassUQInfo);

			if (Parent.CXI_NetMass != 0m)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CXI_NetMassUQInfo);
			}
		}

		protected override void CheckCXI_Status()
		{
			base.CheckCXI_Status();

			var parent = Parent;
			if (parent.CXI_Status.Length != 3)
			{
				parent.CXI_StatusInfo.AddError(Res.GetString("CB692C18-65D0-4CCB-8746-083346059690", "Status length must be 3"));
			}
			else
			{
				CheckCXI_StatusIsValidCode();
			}
		}

		protected virtual void CheckCXI_StatusIsValidCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CXI_StatusInfo);
		}
	}
}
