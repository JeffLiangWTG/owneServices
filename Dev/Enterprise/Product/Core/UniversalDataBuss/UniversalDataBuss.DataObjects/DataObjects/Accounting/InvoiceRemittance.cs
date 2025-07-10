using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class InvoiceRemittance : IDataObject
	{
		public CodeDescriptionPair Type { get; set; }

		[MaxLength(20)]
		public ZString? BillerCode { get; set; }

		[MaxLength(50)]
		public ZString? BillerAccountNumber { get; set; }

		[MaxLength(160)]
		public ZString? Message { get; set; }

		[MaxLength(8)]
		public ZString? InvoiceTransactionReference { get; set; }

		[MaxLength(120)]
		public ZString? InvoiceRemittanceReference { get; set; }

		[MaxLength(20)]
		public ZString? DebtorClientNumber { get; set; }
	}
}

