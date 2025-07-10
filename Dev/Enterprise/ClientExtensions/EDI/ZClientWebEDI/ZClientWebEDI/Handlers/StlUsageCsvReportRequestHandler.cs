using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class StlUsageCsvReportRequestHandler : CsvReportDataRequestHandler<StlUsageReportRequestHelper>
	{
		protected override ICsvReportingBusinessObject GetReportingBizO()
		{
			return ReportingBusinessObjectCreator.CreateStlReportingBusinessObject(SecureQueryString, Factory);
		}
	}
}