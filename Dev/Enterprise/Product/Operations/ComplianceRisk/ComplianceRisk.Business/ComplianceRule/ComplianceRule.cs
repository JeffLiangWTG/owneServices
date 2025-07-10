using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRule : AutoComplianceRule
	{
		public ComplianceRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString CurrentCountryCode { get; set; }

		public RefCountry CurrentCountry => Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CurrentCountryCode);

		[List(nameof(Lookups) + "." + nameof(ComplianceRuleLookups.RefCountry_List))]
		[MaxLength(2)]
		public override ZString CRU_Origin
		{
			get => base.CRU_Origin;
			set
			{
				base.CRU_Origin = value;
				Validation.ValidateCRU_Destination();
			}
		}

		[List(nameof(Lookups) + "." + nameof(ComplianceRuleLookups.RefCountry_List))]
		[MaxLength(2)]
		public override ZString CRU_Destination
		{
			get => base.CRU_Destination;
			set
			{
				base.CRU_Destination = value;
				Validation.ValidateCRU_Origin();
			}
		}

		[List(nameof(Lookups) + "." + nameof(ComplianceRuleLookups.ComplianceRuleRiskStatusCodeList))]
		public override ZString CRU_RiskStatus
		{
			get => base.CRU_RiskStatus;
			set
			{
				base.CRU_RiskStatus = value;
			}
		}
	}
}
