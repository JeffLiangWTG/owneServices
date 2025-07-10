using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class CarrierAccount : IDataObject
	{
		[MaxLength(10)]
		public ZString? AccountNumber { get; set; }
		[MaxLength(20)]
		public ZString? DepotID { get; set; }
		[MaxLength(20)]
		public ZString? MerchantNumber { get; set; }
		[MaxLength(12)]
		public ZString? BillToParty { get; set; }
		[MaxLength(40)]
		public ZString? CarrierAccountType { get; set; }
	}
}

