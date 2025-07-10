using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ESAddInfoTaxLookups : EUAddInfoTaxLookups
	{
		public ESAddInfoTaxLookups(Tax_OnlyForPivot parent) : base(parent)
		{
		}

		public new Tax_OnlyForPivot Parent => (Tax_OnlyForPivot)base.Parent;

		protected override TaxLookupsCommon GetNewCommonLookupsHelper() => new ESTaxLookupsCommon(Parent);
	}
}
