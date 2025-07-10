//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiOrgOpportunityExValidation
//
//    This class should be used for overriding validation in AutoEdiOrgOpportunityExValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	using CargoWise.EntityFramework;

	public class EdiOrgOpportunityExValidation : AutoEdiOrgOpportunityExValidation
	{
		public EdiOrgOpportunityExValidation(AutoEdiOrgOpportunityEx parent) : base(parent)
		{
		}

		protected override void CheckEOM_GlobalPotential()
		{
			base.CheckEOM_GlobalPotential();
			MandatoryValidation.CheckNotNegative(Parent.EOM_GlobalPotentialInfo);
		}

		protected override void CheckEOM_RX_NKLifetimeValueCurrency()
		{
			base.CheckEOM_RX_NKLifetimeValueCurrency();
			MandatoryValidation.CheckEntered(Parent.EOM_RX_NKLifetimeValueCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.EOM_RX_NKLifetimeValueCurrencyInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEOM_Calc_LifetimeValueOver3YearsLocalCurrency();
		}

		new EdiOrgOpportunityEx Parent
		{
			get
			{
				return base.Parent as EdiOrgOpportunityEx;
			}
		}

		public void ValidateEOM_Calc_LifetimeValueOver3YearsLocalCurrency()
		{
			ValidateCalculatedProperty(Parent.EOM_Calc_LifetimeValueOver3YearsLocalCurrencyInfo);
		}

		protected void CheckEOM_Calc_LifetimeValueOver3YearsLocalCurrency()
		{
			EDIOrgOpportunityValidation.CheckOpportunityExchangeRateCompany(Parent.EOM_Calc_LifetimeValueOver3YearsLocalCurrencyInfo, Parent.EOM_Calc_LifetimeValueOver3YearsLocalCurrency);
		}
	}
}


