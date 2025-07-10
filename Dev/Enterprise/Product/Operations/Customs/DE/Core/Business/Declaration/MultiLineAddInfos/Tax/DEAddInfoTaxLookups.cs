using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.DE.Business
{
	public class DEAddInfoTaxLookups : EUAddInfoTaxLookups
	{
		public DEAddInfoTaxLookups(Tax_OnlyForPivot parent) : base(parent)
		{
		}

		public new Tax_OnlyForPivot Parent => (Tax_OnlyForPivot)base.Parent;

		protected override TaxLookupsCommon GetNewCommonLookupsHelper() => new DETaxLookupsCommon(Parent);
	}
}
