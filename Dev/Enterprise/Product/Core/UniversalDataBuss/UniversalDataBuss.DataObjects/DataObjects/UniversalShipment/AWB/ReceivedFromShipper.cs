using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public sealed class ReceivedFromShipper : IDataObject
	{
		[MaxLength(2), Mandatory]
		public ZString? Code { get; set; }

		[MaxLength(80)]
		public ZString? Description { get; set; }

		[MaxLength(15)]
		public ZString? Number { get; set; }

		public ZDateTime? ExpiryDate { get; set; }
	}
}
