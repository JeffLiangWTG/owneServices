using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class CusGoodsLocation : EU.H7.Business.CusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
	{
	}

	[ResourceStringData("Enterprise.Customs.IT.H7.Business.CusGoodsLocation|CGL_Authorization", Caption = "Authorization")]
	public ZString CGL_Authorization
	{
		get
		{
			return cGL_Authorization;
		}
		set
		{
			cGL_Authorization = value;
		}
	}

	ZString cGL_Authorization;
}
