using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiOrgOpportunityEx : AutoEdiOrgOpportunityEx
	{
		public EdiOrgOpportunityEx(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EOM_RX_NKLifetimeValueCurrency = EDIOrgOpportunityConstants.OpportunityDefaultCurrency;
		}

		[DecimalPlaces(2)]
		public ZDecimal EOM_Calc_LifetimeValueOver3YearsLocal
		{
			get { return Opportunity.GetLocalCurrencyValue(EOM_RX_NKLifetimeValueCurrency, EOM_LifetimeValueOver3Years, EOM_Calc_LifetimeValueOver3YearsLocalCurrency); }
		}

		public ZPropertyInfo EOM_Calc_LifetimeValueOver3YearsLocalInfo
		{
			get { return GetZPropertyInfo(nameof(EOM_Calc_LifetimeValueOver3YearsLocal)); }
		}

		public ZString EOM_Calc_LifetimeValueOver3YearsLocalCurrency
		{
			get { return EDIOrgOpportunityConstants.OpportunityDefaultLocalCurrency; }
		}

		public ZPropertyInfo EOM_Calc_LifetimeValueOver3YearsLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(EOM_Calc_LifetimeValueOver3YearsLocalCurrency)); }
		}

		[DecimalPlaces(2)]
		public override ZDecimal EOM_LifetimeValueOver3Years
		{
			get
			{
				return base.EOM_LifetimeValueOver3Years;
			}

			set
			{
				base.EOM_LifetimeValueOver3Years = value;
			}
		}

		public new EDIOrgOpportunity Opportunity
		{
			get { return Factory.Load<EDIOrgOpportunity>(EOM_P8); }
		}

		[ResourceStringData("07667a4f-b55c-4330-8ca5-b40b23cd7c52", Caption = "Global Reach")]
		public override ZInt EOM_GlobalPotential
		{
			get
			{
				return base.EOM_GlobalPotential;
			}

			set
			{
				base.EOM_GlobalPotential = value;
			}
		}
	}
}


