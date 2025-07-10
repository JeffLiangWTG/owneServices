//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoComplianceRiskStatusValidation
//
//    This class should be used for overriding validation in AutoComplianceRiskStatusValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskStatusValidation : AutoComplianceRiskStatusValidation
	{
		public ComplianceRiskStatusValidation(AutoComplianceRiskStatus parent) : base(parent)
		{
		}

		protected override void CheckCOR_LocationRisk()
		{
			base.CheckCOR_LocationRisk();
			MandatoryValidation.CheckEntered(Parent.COR_LocationRiskInfo);
			ListValidation.ErrorIfInvalidCode(Parent.COR_LocationRiskInfo, Parent.Lookups.LocationRiskStatusCodes);
		}

		protected override void CheckCOR_OverallRisk()
		{
			base.CheckCOR_OverallRisk();
			MandatoryValidation.CheckEntered(Parent.COR_OverallRiskInfo);
			ListValidation.ErrorIfInvalidCode(Parent.COR_OverallRiskInfo, Parent.Lookups.OverallRiskStatusCodes);
		}

		protected override void CheckCOR_PartyRisk()
		{
			base.CheckCOR_PartyRisk();
			MandatoryValidation.CheckEntered(Parent.COR_PartyRiskInfo);
			ListValidation.ErrorIfInvalidCode(Parent.COR_PartyRiskInfo, Parent.Lookups.PartyRiskStatusCodes);
		}

		protected override void CheckCOR_JobEndDateIsValidZDateTimeOffsetRange()
		{
			//Don't validate it as it can legitimately be more than 10 years old.
		}
	}
}
