//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoInvestigationItemValidation
//
//    This class should be used for overriding validation in AutoInvestigationItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class InvestigationItemValidation : AutoInvestigationItemValidation
	{
		public InvestigationItemValidation(AutoInvestigationItem parent) : base(parent)
		{
		}

		new InvestigationItem Parent
		{
			get { return (InvestigationItem)base.Parent; }
		}

		protected override void CheckINV_Description()
		{
			base.CheckINV_Description();
			MandatoryValidation.CheckEntered(Parent.INV_DescriptionInfo);
		}

		protected override void CheckINV_ItemText()
		{
			base.CheckINV_ItemText();
			MandatoryValidation.CheckEntered(Parent.INV_ItemTextInfo);
		}

		protected override void CheckINV_Type()
		{
			base.CheckINV_Type();
			MandatoryValidation.CheckEntered(Parent.INV_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.INV_TypeInfo);
		}
	}
}

