using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Inner)]
	public class UberChildLine : IDataObject
	{
		[Mandatory]
		public ZInt? LineNumber { get; set; }
		public ZDecimal? InvoicedValue { get; set; }

		public UberUnitOfWeight WeightUnit { get; set; }
		public UberUnitOfVolume VolumeUnit { get; set; }
		public UberPackingUnit PackageUnit { get; set; }
	}
}

