using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class CashAdvanceRequestHeader : IDataObject
	{
		public CashAdvanceRequestHeader()
		{
		}

		public CashAdvanceRequestHeader(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(8)]
		public ZString? RequestReferenceNumber { get; set; }
		[MaxLength(3)]
		public ZString? Status { get; set; }
		[MaxLength(2)]
		public ZString? Ledger { get; set; }
		public OrganizationReference OrgHeader { get; set; }
		public Currency Currency { get; set; }
		public ZDecimal? OSAmount { get; set; }
		public ZDecimal? OSPaidAmount { get; set; }
		public ZDecimal? LocalAmount { get; set; }
		public ZDecimal? LocalPaidAmount { get; set; }
	}
}
