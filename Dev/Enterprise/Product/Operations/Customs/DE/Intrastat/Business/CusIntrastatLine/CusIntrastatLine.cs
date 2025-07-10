using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Intrastat.Business
{
	public class CusIntrastatLine : EU.Intrastat.Business.CusIntrastatLine
	{
		public CusIntrastatLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusIntrastatLineLookups Lookups => (CusIntrastatLineLookups)base.Lookups;

		protected override EU.Intrastat.Business.CusIntrastatLineLookups GetNewLookups() => new CusIntrastatLineLookups(this);
	}
}
