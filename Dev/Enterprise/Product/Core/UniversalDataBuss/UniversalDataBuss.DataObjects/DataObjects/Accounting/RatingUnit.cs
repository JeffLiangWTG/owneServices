using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class RatingUnit : IDataObject
	{
		[MaxLength(100)]
		public ZString? Code { get; set; }
		[MaxLength(80)]
		public ZString? Description { get; set; }
		public RatingUnitClass Class { get; set; }
	}
}
