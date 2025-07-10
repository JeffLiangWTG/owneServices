using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(Placement.Outer)]
	public class GLAccount : IDataObject
	{
		[MaxLength(10), Mandatory]
		public ZString? AccountCode { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
		public CodeDescriptionPair AccountType { get; set; }
		public DebitCredit? DebitCredit { get; set; }
		[MaxLength(10)]
		public ZString? ConsolidationAccountCode { get; set; }
		[MaxLength(20)]
		public ZString? LocalComplianceAccountCode { get; set; }
		[MaxLength(80)]
		public ZString? LocalComplianceAccountDescription { get; set; }
		public CodeDescriptionPair StatisticalUnit { get; set; }
		public ZDateTime? CreationDate { get; set; }
	}
}

