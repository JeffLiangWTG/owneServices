using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogDownloadObjectLookups : ZLookups
	{
		public GoodsCatalogDownloadObjectLookups(GoodsCatalogDownloadObject parent)
			: base(parent)
		{
		}

		public ConsigneeOrConsignorCollection ConsigneeOrConsignorList => new ConsigneeOrConsignorCollection(Factory);

		public GlbStaffCollection BrokerList => new GlbStaffCollection(Factory);
	}
}
