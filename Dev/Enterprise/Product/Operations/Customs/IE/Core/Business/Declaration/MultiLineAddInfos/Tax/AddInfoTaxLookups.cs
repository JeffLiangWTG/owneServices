using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class AddInfoTaxLookups : EUAddInfoTaxLookups
	{
		public AddInfoTaxLookups(Tax_OnlyForPivot parent) : base(parent)
		{
		}

		public new Tax_OnlyForPivot Parent => (Tax_OnlyForPivot)base.Parent;

		protected override EU.Business.Declaration.MultiLineAddInfos.TaxLookupsCommon GetNewCommonLookupsHelper() => new TaxLookupsCommon(Parent);
	}
}
