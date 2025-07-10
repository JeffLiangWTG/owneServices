using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class VehicleNumberCollection : CusCodeDataCollection<VehicleNumber>
	{
		public VehicleNumberCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine, CusCodeDataTypeList.Codes.VehicleNumber)
		{
		}
	}
}
