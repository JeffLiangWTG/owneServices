using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class RegistrationNumber : IDataObject
	{
		[CandidateKey]
		public Country CountryOfIssue { get; set; }
		[Mandatory, CandidateKey]
		public RegistrationNumberType Type { get; set; }
		[MaxLength(254), Mandatory]
		public ZString? Value { get; set; }
	}
}

