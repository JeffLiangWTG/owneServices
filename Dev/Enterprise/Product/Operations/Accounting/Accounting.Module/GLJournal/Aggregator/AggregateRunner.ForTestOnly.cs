#if DEBUG

using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Aggregator
{
	public partial class AggregateRunner
	{
		public bool DoesTempTableExist_ForTestOnly()
		{
			return DoesTempTableExist();
		}

		public string GenerateReport_ForTestOnly(GlbCompany company)
		{
			return GenerateReport(company);
		}

		public string GetErrorMsgHeader_ForTestOnly(GlbCompany company)
		{
			return GetErrorMsgHeader(company);
		}

		public bool IsAggregateOnlyForSinglePeriod_ForTestOnly(GlbCompany company)
		{
			return IsAggregateOnlyForSinglePeriod(company);
		}

		public bool IsPeriodTotalEqualsZero_ForTestOnly(Guid companyPK, int period)
		{
			return IsPeriodTotalEqualsZero(companyPK, period);
		}
	}
}

#endif