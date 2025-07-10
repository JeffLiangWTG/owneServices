using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class ParticipantGroup : IDataObject
	{
		[Mandatory]
		public Group Group { get; set; }

		public UNLOCO Location { get; set; }

		public ZBool? IsActive { get; set; }
		public ZBool? IsSubscribed { get; set; }
	}
}
