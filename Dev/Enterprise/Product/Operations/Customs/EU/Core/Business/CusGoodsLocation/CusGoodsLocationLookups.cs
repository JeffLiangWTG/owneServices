using System.Collections;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusGoodsLocationLookups : Customs.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(CusGoodsLocation parent)
			: base(parent)
		{
		}

		public virtual ICollection UnlocodeList => new RefUNLOCOCollection(Factory);

		public virtual ZZRefCusCodeListCombinedCollection CustomsOfficeList => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory);
	}
}
