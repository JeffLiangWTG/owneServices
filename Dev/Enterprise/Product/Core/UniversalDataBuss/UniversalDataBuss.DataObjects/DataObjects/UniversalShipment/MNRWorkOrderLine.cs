using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class MNRWorkOrderLine : IDataObject
	{
		public CodeGroupPair ComponentCode { get; set; }
		public CodeGroupPair UnitSection { get; set; }
		public CodeGroupPair RepairCode { get; set; }
		public CodeGroupPair Material { get; set; }
		public CodeGroupPair Damage { get; set; }
		[MaxLength(2)]
		public ZString? UnitOfDimension { get; set; }
		[MaxLength(3)]
		public ZString? ResponsibleParty { get; set; }
		[MaxLength(100)]
		public ZString? Description { get; set; }
		public ZDecimal? Width { get; set; }
		public ZDecimal? Length { get; set; }
		public ZInt? MaterialQuantity { get; set; }
		public ZDecimal?  LaborHours { get; set; }
	}
}
