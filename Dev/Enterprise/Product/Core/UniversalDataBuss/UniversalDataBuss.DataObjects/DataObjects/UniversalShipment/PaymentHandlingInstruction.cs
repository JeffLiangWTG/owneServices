using CargoWise.ComponentModel;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class PaymentHandlingInstruction : IDataObject
	{
		[Mandatory]
		public CodeDescriptionPair Category { get; set; }
		[Mandatory]
		public CodeDescriptionPair PaymentMethod { get; set; }
	}
}
