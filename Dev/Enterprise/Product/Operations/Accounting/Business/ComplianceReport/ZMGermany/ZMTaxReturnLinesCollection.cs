using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport.ZMGermany
{
	public class ZMTaxReturnLinesCollection : BusinessObjectCollection<AccTaxReturnLine>
	{
		public ZMTaxReturnLinesCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
