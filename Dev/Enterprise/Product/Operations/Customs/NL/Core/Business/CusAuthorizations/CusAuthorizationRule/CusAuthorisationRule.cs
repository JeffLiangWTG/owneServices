using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business;

public class CusAuthorisationRule : Customs.Business.CusAuthorisationRule, ICusGoodsLocationProvider
{
	public CusAuthorisationRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
		goodsLocation = (CusGoodsLocation)GoodsLocation;
	}

	public EU.Business.CusGoodsLocation GoodsLocation
	{
		get
		{
			if (goodsLocation == null)
			{
				goodsLocation = GetGoodsLocation();
				RegisterEditableChildObject(goodsLocation);	
			}
			return goodsLocation;
		}
	}
	CusGoodsLocation goodsLocation;

	CusGoodsLocation GetGoodsLocation() => Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.CustomsPermitRule);

	public ZString GoodsLocationDescription => GoodsLocation.DisplayText;

	public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

	ZString ICusGoodsLocationProvider.ProviderKey => ZString.Empty;

	public void ValidateGoodsLocationDescription()
	{
	}

	protected override void OnFactorySaving()
	{
		base.OnFactorySaving();
		CPR_ValueFrom = GoodsLocationDescription;
	}

	public override void Delete()
	{
		goodsLocation.Delete();
		base.Delete();
	}
}
