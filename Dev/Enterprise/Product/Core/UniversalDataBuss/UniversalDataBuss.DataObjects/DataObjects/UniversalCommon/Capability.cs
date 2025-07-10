using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class Capability : ICodeNameDataObject
	{
		public static Capability New(IGlbCapability capabilityBusinessObject)
		{
			return capabilityBusinessObject == null ? null : new Capability
			{
				Code = capabilityBusinessObject.G4_Code,
				Name = capabilityBusinessObject.G4_Description,
			};
		}

		[MaxLength(3), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(50)]
		public ZString? Name { get; set; }
	}
}
