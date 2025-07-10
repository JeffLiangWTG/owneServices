using System.Collections;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business
{
	public class CusGoodsLocationAddressLookups : EU.Business.CusGoodsLocationAddressLookups
	{
		public CusGoodsLocationAddressLookups(CusGoodsLocationAddress parent) : base(parent)
		{
		}

		public override ICollection AuthorisationNumberList
		{
			get
			{
				var goodsLocation = Parent.GoodsLocation;
				var isQualifierEqY = goodsLocation.CGL_Qualifier.Equals(CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
				var isTypeEqB = goodsLocation.CGL_Type.Equals(CusGoodsLocationTypeList.Codes.AuthorizedPlace);

				return isQualifierEqY && isTypeEqB ? LocationsHelper.GetESLocationsCusCodeList(Factory) : base.AuthorisationNumberList;
			}
		}
	}
}
