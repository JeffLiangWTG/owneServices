using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class JobDeclaration : ICusGoodsLocationProvider
{
	public void DeleteGoodsLocation()
	{
		if (goodsLocation != null)
		{
			goodsLocation.Delete();
			goodsLocation = null;
		}
	}

	public override void ValidateGoodsLocationDescriptionCore()
	{
		if (Validation is ExportJobDeclarationValidation exportValidation)
		{
			exportValidation.ValidateGoodsLocationDescription();
		}
	}

	ZString EU.Business.ICusGoodsLocationProvider.ProviderKey => CountryCode + GoodsLocationProviderApplications.Codes.JobDeclaration;

		public void ClearGoodsLocation()
		{
			var goodsLocation = GoodsLocation;
			goodsLocation.CGL_Qualifier = ZString.Empty;
			goodsLocation.CGL_Type = ZString.Empty;
			goodsLocation.CGL_CustomsOffice = ZString.Empty;
		}

	protected override EU.Business.CusGoodsLocation GetGoodsLocation() => Customs.Business.CusGoodsLocation.LoadOrCreate<EU.Business.CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Departure);
}
