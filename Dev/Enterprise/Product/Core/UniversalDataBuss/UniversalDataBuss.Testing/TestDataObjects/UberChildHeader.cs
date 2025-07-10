using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Inner)]
	public class UberChildHeader : IDataObject
	{
		[MaxLength(10), Mandatory]
		public ZString? HeaderReference { get; set; }
		public ZDecimal? InvoicedValue { get; set; }
		public ZDateTime? InvoicedDate { get; set; }

		public UberOrganization Importer { get; set; }
		public UberOrganization Exporter { get; set; }
		public UberUNLOCO Origin { get; set; }
		public UberUNLOCO Destination { get; set; }

		public List<UberChildLine> LineCollection { get; set; }
	}
}

