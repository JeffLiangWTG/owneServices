using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public sealed class LocationOfGoodsFromAuthorisationDefaulterConfiguration : EU.NCTS.Business.LocationOfGoodsFromAuthorisationDefaulterConfiguration
	{
		protected override bool IsDefaultingEnabledCore() => true;

		protected override string QualifierCodeCore() => CusGoodsLocationQualifierList.Codes.UnLocode;

		protected override string TypeCodeCore() => CusGoodsLocationTypeList.Codes.AuthorizedPlace;
	}
}
