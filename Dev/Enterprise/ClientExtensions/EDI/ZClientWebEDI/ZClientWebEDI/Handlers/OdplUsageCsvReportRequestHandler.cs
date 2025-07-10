using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class OdplUsageCsvReportRequestHandler : CsvReportDataRequestHandler<OdplUsageReportRequestHelper>
	{
		protected override ICsvReportingBusinessObject GetReportingBizO()
		{
			return ReportingBusinessObjectCreator.CreateOdplReportingBusinessObject(SecureQueryString, Factory);
		}
	}
}