using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class ParcelCollection : CusSupportingInfoCollection<Parcel>
	{
		public ParcelCollection(JobComInvoiceHeader parent)
		: base(parent, CusSupportingInfoTypeList.Codes.Parcel)
		{
		}
	}
}
