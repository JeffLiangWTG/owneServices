using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class InwardProcessingProductCollection
		: CusSupportingInfoCollection<InwardProcessingProduct>
	{
		public InwardProcessingProductCollection(JobComInvoiceLine parent)
			: base(parent, InwardProcessingProduct.CusSupportingInfoType)
		{
			MaxCountValidationEnable(999);
		}
	}
}
