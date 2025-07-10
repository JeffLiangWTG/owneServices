using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class LocationOfGoodsFromAuthorisationDefaulterConfiguration
	{
		public bool IsDefaultingEnabled => IsDefaultingEnabledCore();
		protected virtual bool IsDefaultingEnabledCore() => true;

		public string QualifierCode => QualifierCodeCore();
		protected virtual string QualifierCodeCore() => CusGoodsLocationQualifierList.Codes.AuthorizationNumber;

		public string TypeCode => TypeCodeCore();
		protected virtual string TypeCodeCore() => CusGoodsLocationTypeList.Codes.AuthorizedPlace;
	}
}
