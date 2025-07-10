using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceHeaderValidation : Customs.Business.JobComInvoiceHeaderValidation
	{
		public EMCSJobComInvoiceHeaderValidation(EMCSJobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new EMCSJobComInvoiceHeader Parent => (EMCSJobComInvoiceHeader)base.Parent;
		protected override void CheckJZ_InvoiceNumber()
		{
			CheckJZ_InvoiceNumberIsMandatory();
			if (!Parent.JZ_InvoiceNumber.IsEmpty && Parent.InvoiceLines.Count == 0)
			{
				var message = Res.GetString("4f915f4f-a8a8-4a4d-95f7-5a74cbc5588a", "Invoice Lines should have at least one row.");
				Parent.JZ_InvoiceNumberInfo.AddMessageError(message);
			}
		}

		protected virtual void CheckJZ_InvoiceNumberIsMandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceNumberInfo);
		}
	}
}

