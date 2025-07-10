using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public sealed class CusGoodsLocationAddressValidation : EU.Business.CusGoodsLocationAddressValidation
	{
		public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent) : base(parent)
		{
		}

		new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();
			var parent = Parent;
			if (!parent.E2_GovRegNum.IsEmpty && parent.GoodsLocation?.Parent is TemporaryStorageHeader)
			{
				var type = parent.GoodsLocation.CGL_Type;
				if (type == CusGoodsLocationTypeList.Codes.AuthorizedPlace)
				{
					CheckBR_PN_TS_FR06();
				}
				else if (type == CusGoodsLocationTypeList.Codes.ApprovedPlace)
				{
					CheckBR_PN_TS_FR08();
				}
			}
		}

		void CheckBR_PN_TS_FR06()
		{
			var parent = Parent;

			if (!parent.E2_GovRegNum.StartsWith(UniversalReferenceConstants.AuthorizationNumber.AuthorizedPlaceAuthorizationPrefix))
			{
				parent.E2_GovRegNumInfo.AddMessageError(Res.GetString("E7302B72-5A84-44D6-8E30-BAE86131E8F5", "Authorization No. must start with 'FRTST'."));
			}
		}

		void CheckBR_PN_TS_FR08()
		{
			var parent = Parent;

			if (!parent.E2_GovRegNum.StartsWith(UniversalReferenceConstants.AuthorizationNumber.ApprovedPlaceAuthorizationPrefix))
			{
				parent.E2_GovRegNumInfo.AddMessageError(Res.GetString("0FC4940B-CE8A-43A1-AB9C-59E1C33B2068", "Authorization No. must start with 'LADT'."));
			}
		}
	}
}
