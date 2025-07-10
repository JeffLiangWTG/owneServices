using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Inner)]
	public class UberInnerRelatedObject : IDataObject
	{
		public ZDecimal? ExtraDecimal { get; set; }
		public ZDateTime? ExtraDateTime { get; set; }
		[MaxLength(50)]
		public ZString? ExtraField { get; set; }
		public ZBool? ExtraBoolean { get; set; }
		public ZInt? ExtraInteger { get; set; }
	}
}

