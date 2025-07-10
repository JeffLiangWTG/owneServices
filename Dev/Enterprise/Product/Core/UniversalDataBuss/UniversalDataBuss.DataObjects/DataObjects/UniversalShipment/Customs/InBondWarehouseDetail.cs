using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public class InBondWarehouseDetail : IDataObject
	{
		[MaxLength(20)]
		public ZString? EntryNumber { get; set; }
		public ZDecimal? BondedQuantity { get; set; }
		public ZDecimal? WithdrawQuantity { get; set; }
	}
}
