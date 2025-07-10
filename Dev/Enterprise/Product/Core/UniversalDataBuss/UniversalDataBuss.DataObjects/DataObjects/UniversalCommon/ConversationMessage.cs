using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class ConversationMessage : IDataObject
	{
		[Mandatory, MaxLength(UniversalXmlInfo.MaxStringLength)]
		public ZString? Text { get; set; }

		[Mandatory, MaxLength(256)]
		public ZString? ParticipantName { get; set; }

		public ZDateTime? CreatedTime { get; set; }

		public ZBool? IsInternal { get; set; }
		public ZBool? IsSystem { get; set; }
	}
}
