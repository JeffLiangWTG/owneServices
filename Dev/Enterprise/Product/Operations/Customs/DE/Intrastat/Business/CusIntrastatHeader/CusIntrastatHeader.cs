using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Intrastat.Business;

namespace Enterprise.Customs.DE.Intrastat.Business
{
	public class CusIntrastatHeader : EU.Intrastat.Business.CusIntrastatHeader
	{
		public CusIntrastatHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusIntrastatLineCollection<CusIntrastatLine> CusIntrastatLines => (CusIntrastatLineCollection<CusIntrastatLine>)base.CusIntrastatLines;

		protected override ICusIntrastatLineCollection<EU.Intrastat.Business.CusIntrastatLine> CreateNewCusIntrastatLineCollection() => new CusIntrastatLineCollection<CusIntrastatLine>(this);
	}
}
