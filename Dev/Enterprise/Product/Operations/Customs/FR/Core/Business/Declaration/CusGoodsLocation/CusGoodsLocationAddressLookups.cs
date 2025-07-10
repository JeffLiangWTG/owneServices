using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public sealed class CusGoodsLocationAddressLookups : EU.Business.CusGoodsLocationAddressLookups
	{
		public CusGoodsLocationAddressLookups(CusGoodsLocationAddress parent) : base(parent)
		{
		}

		protected override ZString GetAuthorisationNumber()
		{
			var parent = Parent;
			var result = base.GetAuthorisationNumber();

			if (parent.GoodsLocation.Parent is TemporaryStorageHeader)
			{
				var type = Parent.GoodsLocation.CGL_Type;
				result = type == CusGoodsLocationTypeList.Codes.AuthorizedPlace ? UniversalReferenceConstants.AuthorizationNumber.AuthorizedPlaceAuthorizationPrefix :
					type == CusGoodsLocationTypeList.Codes.ApprovedPlace ? UniversalReferenceConstants.AuthorizationNumber.ApprovedPlaceAuthorizationPrefix : result;
			}

			return result;
		}
	}
}
