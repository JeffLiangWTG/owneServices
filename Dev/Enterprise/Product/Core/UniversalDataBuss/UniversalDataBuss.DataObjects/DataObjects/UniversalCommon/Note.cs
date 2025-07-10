using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class Note : IDataObject
	{
		[CandidateKey, MaxLength(50), Mandatory]
		public ZString? Description { get; set; }
		[Mandatory]
		public ZBool? IsCustomDescription { get; set; }
		[Mandatory, MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? NoteText { get; set; }
		public NoteContext NoteContext { get; set; }
		public CodeDescriptionPair Visibility { get; set; }
		public CodeDescriptionPair VisibleCompany { get; set; }
	}
}

