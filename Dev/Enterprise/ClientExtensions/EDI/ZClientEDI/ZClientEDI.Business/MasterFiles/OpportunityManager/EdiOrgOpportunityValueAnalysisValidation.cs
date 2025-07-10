//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiOrgOpportunityValueAnalysisValidation
//
//    This class should be used for overriding validation in AutoEdiOrgOpportunityValueAnalysisValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiOrgOpportunityValueAnalysisValidation : AutoEdiOrgOpportunityValueAnalysisValidation
	{
		public EdiOrgOpportunityValueAnalysisValidation(AutoEdiOrgOpportunityValueAnalysis parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEOV_Calc_LocalValueCurrency();
		}

		new EdiOrgOpportunityValueAnalysis Parent
		{
			get
			{
				return base.Parent as EdiOrgOpportunityValueAnalysis;
			}
		}

		public void ValidateEOV_Calc_LocalValueCurrency()
		{
			ValidateCalculatedProperty(Parent.EOV_Calc_LocalValueCurrencyInfo);
		}

		protected void CheckEOV_Calc_LocalValueCurrency()
		{
			EDIOrgOpportunityValidation.CheckOpportunityExchangeRateCompany(Parent.EOV_Calc_LocalValueCurrencyInfo, Parent.EOV_Calc_LocalValueCurrency);
		}
	}
}


