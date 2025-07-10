using CargoWise.EntityFramework;
//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMailDBItemsValidation
//
//    This class should be used for overriding validation in AutoMailDBItemsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MailManager.Business
{
	public class MailDBItemsValidation : AutoMailDBItemsValidation
	{
		public MailDBItemsValidation(AutoMailDBItems parent) : base(parent)
		{
		}

		protected override void CheckMI_From()
		{
			base.CheckMI_From();
			if (!Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.MI_FromInfo);
			}
		}

		protected override void CheckMI_Direction()
		{
			base.CheckMI_Direction();
			if (!Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.MI_DirectionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.MI_DirectionInfo, Parent.Lookups.Directions);
			}
		}

		protected override void CheckMI_Status()
		{
			base.CheckMI_Status();
			if (!Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.MI_StatusInfo);
				ListValidation.ErrorIfInvalidCode(Parent.MI_StatusInfo, Parent.Lookups.StatusCodes);
			}
		}
	}
}
