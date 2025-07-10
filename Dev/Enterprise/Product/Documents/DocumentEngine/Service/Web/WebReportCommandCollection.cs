using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine
{
	public class WebReportCommandCollection : BusinessObjectCollection<ReportCommand>
	{
		public WebReportCommandCollection(BusinessObjectFactory factory, ZQuery additionalQuery)
			: base(factory, additionalQuery)
		{
		}
	}
}
