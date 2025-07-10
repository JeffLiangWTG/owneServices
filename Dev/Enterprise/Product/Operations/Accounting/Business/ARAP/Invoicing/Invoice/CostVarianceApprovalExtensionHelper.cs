using System;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.CostVarianceComparisonOption;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.CostVarianceApprovalExtension
{
	static class CostVarianceApprovalExtensionHelper
	{
		static string GetCostVarianceApprovalKey(GlbCompany company) => string.Format(CultureInfo.InvariantCulture, "CostVarianceApproval_{0}", company.GC_Code);

		public static CostVarianceApproval GetCostVarianceApproval(this BusinessObjectFactory factory, GlbCompany company) => factory.GetCachedValue(GetCostVarianceApprovalKey(company), () => AccountingConfigurationRegistry.Instance.CostVarianceApproval.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty));

		public static void ClearCachedCostVarianceApproval(this BusinessObjectFactory factory, GlbCompany company) => factory.ClearCachedValue<CostVarianceApproval>(GetCostVarianceApprovalKey(company));

		public static bool VarianceByImportedCharge(this BusinessObjectFactory factory, GlbCompany company)
		{
			var costVarianceComparisonOption = factory.GetCostVarianceApproval(company).VarianceComparisonOption;

			return costVarianceComparisonOption == ImportedChargeOrJob
				|| costVarianceComparisonOption == ImportedChargeOrJobChargeCode
				|| costVarianceComparisonOption == ImportedChargeOrCreditor;
		}

		public static bool VarianceByCreditor(this BusinessObjectFactory factory, GlbCompany company)
		{
			var costVarianceComparisonOption = factory.GetCostVarianceApproval(company).VarianceComparisonOption;

			return costVarianceComparisonOption == JobAndCreditor
				|| costVarianceComparisonOption == ImportedChargeOrCreditor;
		}

		public static bool VarianceByJob(this BusinessObjectFactory factory, GlbCompany company)
		{
			var costVarianceComparisonOption = factory.GetCostVarianceApproval(company).VarianceComparisonOption;

			return costVarianceComparisonOption == Job
				|| costVarianceComparisonOption == ImportedChargeOrJob;
		}

#if DEBUG
		internal static string GetCostVarianceApprovalKey_ForTestOnly(GlbCompany company) => GetCostVarianceApprovalKey(company);
#endif
	}
}