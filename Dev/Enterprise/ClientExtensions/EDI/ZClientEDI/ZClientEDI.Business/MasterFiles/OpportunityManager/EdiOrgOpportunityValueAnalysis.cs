using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiOrgOpportunityValueAnalysis : AutoEdiOrgOpportunityValueAnalysis
	{
		public EdiOrgOpportunityValueAnalysis(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZInt EOV_UserCount
		{
			get
			{
				return base.EOV_UserCount;
			}

			set
			{
				base.EOV_UserCount = value;
				Opportunity?.UpdateTotalValueBinding();
			}
		}
		public ZString EOV_Calc_ModuleDescription
		{
			get { return RegistryDefault == null ? ZString.Empty : RegistryDefault.Description; }
		}
		public ZPropertyInfo EOV_Calc_ModuleDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(EOV_Calc_ModuleDescription)); }
		}

		public ZDecimal EOV_Calc_ForeignValue
		{
			get { return RegistryDefault == null ? ZDecimal.Zero : (ZDecimal)(this.EOV_UserCount * RegistryDefault.ValueInUSD); }
		}
		public ZPropertyInfo EOV_Calc_ForeignValueInfo
		{
			get { return GetZPropertyInfo(nameof(EOV_Calc_ForeignValue)); }
		}

		public ZDecimal EOV_Calc_LocalValue
		{
			get { return RegistryDefault == null ? ZDecimal.Zero : Opportunity.GetLocalCurrencyValue(EOV_Calc_ForeignValueCurrency, EOV_Calc_ForeignValue, EOV_Calc_LocalValueCurrency);  }
		}
		public ZPropertyInfo EOV_Calc_LocalValueInfo
		{
			get { return GetZPropertyInfo(nameof(EOV_Calc_LocalValue)); }
		}

		public ZString EOV_Calc_ForeignValueCurrency
		{
			get { return EDIOrgOpportunityConstants.OpportunityValueAnalysisDefaultCurrency; }
		}
		public ZPropertyInfo EOV_Calc_ForeignValueCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(EOV_Calc_ForeignValueCurrency)); }
		}

		public ZString EOV_Calc_LocalValueCurrency
		{
			get { return EDIOrgOpportunityConstants.OpportunityDefaultLocalCurrency; }
		}
		public ZPropertyInfo EOV_Calc_LocalValueCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(EOV_Calc_LocalValueCurrency)); }
		}

		OpportunityValueAnalysisDefault RegistryDefault
		{
			get
			{
				return EDIDataRegistry.Instance.OpportunityValueAnalysisDefaults.OfType<OpportunityValueAnalysisDefault>().FirstOrDefault(x => x.Code == this.EOV_ModuleCode);
			}
		}

		public new EDIOrgOpportunity Opportunity
		{
			get { return Factory.Load<EDIOrgOpportunity>(EOV_P8); }
		}
	}
}


