using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class UberCompany : ICodeNameDataObject
	{
		[MaxLength(3), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Name { get; set; }
		public UberCountry Country { get; set; }
	}
}
