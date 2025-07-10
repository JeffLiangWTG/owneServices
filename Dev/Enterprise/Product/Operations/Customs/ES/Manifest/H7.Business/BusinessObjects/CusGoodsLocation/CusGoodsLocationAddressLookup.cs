using System.Collections;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CusGoodsLocationAddressLookups : EU.Business.CusGoodsLocationAddressLookups
	{
		public CusGoodsLocationAddressLookups(CusGoodsLocationAddress parent) : base(parent)
		{
		}

		public override ICollection AuthorisationNumberList => LocationsHelper.GetESLocationsCusCodeList(Factory);
	}
}
