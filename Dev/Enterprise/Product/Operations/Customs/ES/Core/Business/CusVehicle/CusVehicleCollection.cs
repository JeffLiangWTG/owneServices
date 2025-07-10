using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business
{
	public class CusVehicleCollection : CusVehicleCollection<CusVehicle, JobComInvoiceLine>
	{
		public CusVehicleCollection(JobComInvoiceLine master) : base(master)
		{
		}
	}
}
