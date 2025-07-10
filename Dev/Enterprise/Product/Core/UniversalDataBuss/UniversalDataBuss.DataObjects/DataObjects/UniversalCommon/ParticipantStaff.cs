using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class ParticipantStaff : IDataObject
	{
		[Mandatory]
		public Staff Staff { get; set; }

		public UNLOCO Location { get; set; }

		[MaxLength(128)]
		public ZString? JobTitle { get; set; }

		public ZBool? IsActive { get; set; }
		public ZBool? IsSubscribed { get; set; }
	}
}
