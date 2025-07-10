using CargoWise.Types;

namespace Enterprise.Accounting.Export.Business
{
	public class GLPeriodRow
	{
		public ZGuid CompanyPK { get; set; }
		public ZDateTime? StartDate { get; set; }
		public ZDateTime? EndDate { get; set; }
		public ZInt? Period { get; set; }
	}
}
