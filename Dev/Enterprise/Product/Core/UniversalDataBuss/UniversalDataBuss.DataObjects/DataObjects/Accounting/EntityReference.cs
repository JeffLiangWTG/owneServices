using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class EntityReference : IDataObject
	{
		[MaxLength(40), Mandatory, CandidateKey]
		public ZString? Type { get; set; }
		[MaxLength(35)]
		public ZString? Key { get; set; }
	}
}

