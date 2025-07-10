using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(Placement.Outer)]
	public class MatchLine : IDataObject
	{
		public List<LinkedTransactionID> LinkedTransactionIDCollection { get; set; }
		public ZDecimal? OSPaidAmount { get; set; }
		public OrganizationAddress OrganizationAddress { get; set; }
		[MaxLength(20)]
		public ZString? MatchGroupNumber { get; set; }
		public ZDate? MatchDate { get; set; }
	}
}
