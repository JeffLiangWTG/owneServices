using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework
{
	[XsdSchema(Placement.Outer)]
	public class TaxLink : IDataObject
	{
		[Mandatory]
		public ZInt? TaxTransactionLink { get; set; }
		public ZDecimal? LocalTaxAmount { get; set; }
	}
}
