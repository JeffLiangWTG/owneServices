using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IComplianceReportsProvider
	{
		CodeDescriptionPairList GetReportTypeList(Guid compantPK);
	}
}
