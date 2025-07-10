using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes(nameof(Code), flattenEvenWithOldNamespace: true)]
	public class UberUnitOfWeight : IDataObject
	{
		[MaxLength(2), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
	}
}

