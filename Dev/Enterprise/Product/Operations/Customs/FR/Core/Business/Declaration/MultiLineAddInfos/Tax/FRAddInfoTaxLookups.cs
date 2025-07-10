using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class FRAddInfoTaxLookups : EUAddInfoTaxLookups
	{
		public FRAddInfoTaxLookups(Tax_OnlyForPivot parent) : base(parent)
		{
		}

		public new Tax_OnlyForPivot Parent => (Tax_OnlyForPivot)base.Parent;

		protected override TaxLookupsCommon GetNewCommonLookupsHelper() => new FRTaxLookupsCommon(Parent);
	}
}
