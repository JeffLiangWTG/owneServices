using CargoWise.EntityFramework;

namespace Enterprise.ReportWriter
{
	public interface IReportBizObjProvider
	{
		BusinessObjectFactory Factory { get; }
		ReportBizObj GetReportBizObj();
	}
}
