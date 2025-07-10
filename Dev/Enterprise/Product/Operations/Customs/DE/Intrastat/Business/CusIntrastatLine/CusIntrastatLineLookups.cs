using CargoWise.Integration;
using Enterprise.Customs.EU.Intrastat.Business;

namespace Enterprise.Customs.DE.Intrastat.Business
{
	public class CusIntrastatLineLookups : EU.Intrastat.Business.CusIntrastatLineLookups
	{
		public CusIntrastatLineLookups(AutoCusIntrastatLine parent) : base(parent)
		{
		}

		new CusIntrastatLine Parent => (CusIntrastatLine)base.Parent;

		protected override ICodeDescriptionPairList GetRegions() => Parent.IsImport ? FederalStateList.Import : FederalStateList.Export;
	}
}
