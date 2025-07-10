using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Integration
{
	[PropertyDescriptorCollection(typeof(BusinessObjectPropertyDescriptorCollection))]
	public interface IJobProfitLoss
	{
		IProfitLossFilterProvider Filter { get; }

		ProfitLossCollectionBase ProfitLossDetails { get; }
		ProfitLossDetailCollectionBaseView ProfitLossFilteredDetails { get; }
		ProfitLossSummaryCollectionBase ProfitLossSummaryDetails { get; }
		ProfitLossSummaryCollectionBaseView ProfitLossSummaryFilteredDetails { get; }
		ProfitLossCollectionBase GlobalJobCostingProfitLoss { get; }

		ZDecimal TotalAccrual { get; }
		ZPropertyInfo TotalAccrualInfo { get; }
		ZDecimal TotalAccrualRecognized { get; }
		ZPropertyInfo TotalAccrualRecognizedInfo { get; }
		ZDecimal TotalAccrualNotRecognized { get; }
		ZPropertyInfo TotalAccrualNotRecognizedInfo { get; }

		ZDecimal TotalWIP { get; }
		ZPropertyInfo TotalWIPInfo { get; }
		ZDecimal TotalWIPRecognized { get; }
		ZPropertyInfo TotalWIPRecognizedInfo { get; }
		ZDecimal TotalWIPNotRecognized { get; }
		ZPropertyInfo TotalWIPNotRecognizedInfo { get; }

		ZDecimal TotalCost { get; }
		ZPropertyInfo TotalCostInfo { get; }
		ZDecimal TotalCostRecognized { get; }
		ZPropertyInfo TotalCostRecognizedInfo { get; }
		ZDecimal TotalCostNotRecognized { get; }
		ZPropertyInfo TotalCostNotRecognizedInfo { get; }

		ZDecimal TotalRevenue { get; }
		ZPropertyInfo TotalRevenueInfo { get; }
		ZDecimal TotalRevenueRecognized { get; }
		ZPropertyInfo TotalRevenueRecognizedInfo { get; }
		ZDecimal TotalRevenueNotRecognized { get; }
		ZPropertyInfo TotalRevenueNotRecognizedInfo { get; }

		ZDecimal TotalTaxExpenseRevenue { get; }
		ZPropertyInfo TotalTaxExpenseRevenueInfo { get; }
		ZDecimal TotalTaxExpenseCost { get; }
		ZPropertyInfo TotalTaxExpenseCostInfo { get; }

		ZDecimal TotalLineAmount { get; }
		ZPropertyInfo TotalLineAmountInfo { get; }
		ZDecimal TotalLineAmountRecognized { get; }
		ZPropertyInfo TotalLineAmountRecognizedInfo { get; }
		ZDecimal TotalLineAmountNotRecognized { get; }
		ZPropertyInfo TotalLineAmountNotRecognizedInfo { get; }

		ZString MarginProfitRev { get; }
		ZPropertyInfo MarginProfitRevInfo { get; }
		ZString MarginProfitRevRecognized { get; }
		ZPropertyInfo MarginProfitRevRecognizedInfo { get; }
		ZString MarginProfitRevNotRecognized { get; }
		ZPropertyInfo MarginProfitRevNotRecognizedInfo { get; }

		ZString MarginProfitCost { get; }
		ZPropertyInfo MarginProfitCostInfo { get; }
		ZString MarginProfitCostRecognized { get; }
		ZPropertyInfo MarginProfitCostRecognizedInfo { get; }
		ZString MarginProfitCostNotRecognized { get; }
		ZPropertyInfo MarginProfitCostNotRecognizedInfo { get; }

		BusinessObjectFactory Factory { get; }
		ZInt Decimals { get; }

		void RefreshBinding();
	}

	[PropertyDescriptorCollection(typeof(BusinessObjectPropertyDescriptorCollection))]
	public interface IProfitLossFilterProvider
	{
		[List("ChargeCodes")]
		ZString ChargeCodeFilter { get; set; }
		ZPropertyInfo ChargeCodeFilterInfo { get; }

		[List("Branches")]
		ZGuid BranchFilter { get; set; }
		ZPropertyInfo BranchFilterInfo { get; }

		[List("Departments")]
		ZGuid DepartmentFilter { get; set; }
		ZPropertyInfo DepartmentFilterInfo { get; }

		ZBool ShowReversedFilter { get; set; }
		ZPropertyInfo ShowReversedFilterInfo { get; }

		[List("Jobs")]
		ZGuid JobNumberFilter { get; set; }
		ZPropertyInfo JobNumberFilterInfo { get; }

		[List("Companies")]
		ZGuid CompanyFilter { get; set; }
		ZPropertyInfo CompanyFilterInfo { get; }

		[List("RecognizedChargesList")]
		ZString RecognizedChargesFilter { get; set; }
		CodeDescriptionPairList RecognizedChargesList { get; }

		void ClearFilterValues();

		AccChargeCodeCollection ChargeCodes { get; }
		GlbBranchCollection Branches { get; }
		GlbDepartmentCollection Departments { get; }
		JobHeaderCollection Jobs { get; }
		GlbCompanyCollection Companies { get; }
		ZPropertyInfo RecognizedChargesFilterInfo { get; }
	}
}
