using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public class EntryHeaderCharge : IDataObject
	{
		[CandidateKey, Mandatory]
		public CodeDescriptionPair Type { get; set; }
		[Mandatory]
		public ZDecimal? Amount { get; set; }
	}
}

