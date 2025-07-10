using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(CusGoodsLocation parent) : base(parent)
		{
		}

		protected new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		public override CodeDescriptionPairList QualifierList => Parent.Parent is EnRouteIncident
			? Factory.GetCachedValue("CusGoodsLocationIncidentLookups.QualifierList", () =>
			{
				var qualifierList = new CusGoodsLocationQualifierList();
				qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
				qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
				qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.EoriNumber);
				qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.PostcodeAddress);
				return qualifierList;
			})
			: base.QualifierList;
	}
}
