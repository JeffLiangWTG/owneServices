using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public partial class TransportMeans : IDataObject
	{
		public ZInt? Order { get; set; }
		public CodeDescriptionPair2Char TypeOfIdentification { get; set; }
		[MaxLength(35)]
		public ZString? IdentificationNumber { get; set; }
		public CodeDescriptionPair2Char Nationality { get; set; }
		public List<SealNumber> SealNumberCollection { get; private set; }
		public TransportTypeCode? TransportType { get; set; }
		public CodeDescriptionPair1Char ModeOfTransport { get; set; }
	}
}
