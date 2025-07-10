using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class PackedItem : IDataObject
	{
		public ZDecimal? PackedQuantity { get; set; }
		public PackageType UnitOfQuantity { get; set; }
		public Product Product { get; set; }
		public ZDecimal? NetWeight { get; set; }
		public UnitOfWeight NetWeightUnit { get; set; }
		public ZDecimal? GrossWeight { get; set; }
		public UnitOfWeight GrossWeightUnit { get; set; }
		public ZDecimal? GoodsValue { get; set; }

		[MaxLength(512), AllowLineControlWhiteSpace]
		public ZString? Description { get; set; }
		public ZInt? CommercialInvoiceLineLink { get; set; }
		public ZInt? OrderLineLink { get; set; }

		[MaxLength(512)]
		public ZString? ItemSpecificationUrl { get; set; }
		public ZDecimal? CIFValue { get; set; }
		public ZInt? InBondMoveLineItemLink { get; set; }
	}
}
