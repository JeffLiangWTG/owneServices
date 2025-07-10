using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(Placement.Outer)]
	public class LinkedTransactionID : IDataObject
	{
		[MaxLength(35)]
		public ZString? Type { get; set; }
		[MaxLength(50)]
		public ZString? Key { get; set; }
	}
}
