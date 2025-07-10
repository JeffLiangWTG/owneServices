using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class JobComInvoiceHeaderToPrint : Customs.Business.LineToPrint
	{
		public JobComInvoiceHeaderToPrint(JobComInvoiceHeader header)
			: base(header)
		{
		}

		internal JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)bizObj; }
		}

		public override ZString Identifier
		{
			get { return InvoiceHeader.JZ_InvoiceNumber; }
		}

		public override ZDateTime LastPrintDate
		{
			get { return InvoiceHeader.CA_LVSLastPrintDate; }
		}

		public override ZString Organisation
		{
			get
			{
				var importer_effective = Factory.Load<OrgHeader>(InvoiceHeader.JZ_OH_Buyer_Effective);
				return importer_effective != null ? importer_effective.OH_Code : ZString.Empty;
			}
		}
	}
}
