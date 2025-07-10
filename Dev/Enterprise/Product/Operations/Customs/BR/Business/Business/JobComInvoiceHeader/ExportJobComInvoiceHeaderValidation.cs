using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class ExportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public ExportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected override void CheckJZ_OH_Buyer()
		{
			base.CheckJZ_OH_Buyer();

			if (Parent.CusEntryInstructions.Any(instruction => instruction.CEI_LegalDocument == LegalDocumentList.Codes.NoInvoice))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_BuyerInfo);
			}
		}
	}
}
