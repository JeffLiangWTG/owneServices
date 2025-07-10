using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;

namespace Enterprise.Customs.CA.Business
{
	public class ConsolidateByProvinceOfClearanceStrategy : ConsolidationStrategy
	{
		#region Constructors

		public ConsolidateByProvinceOfClearanceStrategy(IConsolidationOptionsWrapper wrapper)
			: base(wrapper)
		{
		}

#if DEBUG
		public ConsolidateByProvinceOfClearanceStrategy(IConsolidationOptionsWrapper wrapper, bool hasAcknowledged)
			: base(wrapper, hasAcknowledged)
		{
		}
#endif

		#endregion

		#region IConsolidationStrategy Members

		public override void AddMatchingFilter(ZQuery query)
		{
			if (HasAcknowledged)
			{
				var province = Wrapper.Province;
				if (!province.IsEmpty)
				{
					query.AddToFilter(GetProvinceOfClearanceQuery(province));
				}
			}
		}

		public static ZQuery GetProvinceOfClearanceQuery(ZString value)
		{
			return LVXJobsConsolidateHelper.ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, LVXJobsConsolidateHelper.ModelViewPK, LVXJobsConsolidateHelper.ModelView, "JE_ProvinceOfClearance", SQLComparisonOperator.Equal, value);
		}

		public override bool IsMatching(JobDeclaration declaration)
		{
			var province = Wrapper.Province;
			return !HasAcknowledged || declaration.CA_ProvinceOfClearance.IsEmpty || province.IsEmpty || declaration.CA_ProvinceOfClearance == province;
		}

		public override void FillDataForNewDeclaration(JobDeclaration declaration)
		{
			if (HasAcknowledged && declaration.CA_ProvinceOfClearance.IsEmpty)
			{
				var province = Wrapper.Province;
				if (!province.IsEmpty)
				{
					declaration.CA_ProvinceOfClearance = province;
				}
			}
		}

		#endregion

		#region GetEffectiveSetting

		protected override bool GetSettingFromRegistry()
		{
			return CACustomsDataRegistry.Instance.ConsolidateByProvinceOfClearance.GetFallBackValueAtAllLevels(Wrapper.CompanyPK.ToGuid(), Wrapper.BranchPK.ToGuid(), Guid.Empty);
		}

		protected override bool GetSettingOfImporterCore(OrgImpAddInfo importerAddInfo)
		{
			return importerAddInfo.ZO_IsConsolidateByProvinceofClearance;
		}

		#endregion
	}
}
