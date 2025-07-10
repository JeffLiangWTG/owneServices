using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public class JobComInvoiceHeaderValidation : AutoAEJobComInvoiceHeaderValidation
{
	public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
		: base(invoiceHeader)
	{
	}

	protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

	protected override void CheckJZ_OH_Supplier()
	{
		base.CheckJZ_OH_Supplier();
		if (Parent.JobDeclaration != null && Parent.JobDeclaration.Consignor == null)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_SupplierInfo, "supplier. Please fill in either the 'Main Supplier' at the job declaration level or the supplier for each invoice header.");
		}
	}

	protected override void CheckJZ_RX_NKInvoice_Currency()
	{
		base.CheckJZ_RX_NKInvoice_Currency();
		if (Parent.Invoice_Currency == null)
		{
			Parent.JZ_RX_NKInvoice_CurrencyInfo.AddMessageError(InvoiceCurrencyMissing);
		}
	}

	internal const string InvoiceCurrencyMissing = "An invoice currency must be entered";

	protected override void CheckJZ_OH_Buyer()
	{
		base.CheckJZ_OH_Buyer();
		if (Parent.JobDeclaration != null && Parent.JobDeclaration.Importer == null)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_BuyerInfo, "importer. Please fill in either the 'Importer' at the job declaration level or the importer for each invoice header.");
		}
		if (Parent.JobDeclaration != null && Parent.JobDeclaration.IsHighValue && Parent.Buyer != null && Parent.Buyer.LocalCustomsClientCode.IsEmpty)
		{
			Parent.JZ_OH_BuyerInfo.AddMessageError("A local customs client code needs to be added for this importer.");
		}
	}

	protected override void CheckJZ_InvoiceType()
	{
		base.CheckJZ_InvoiceType();

		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_InvoiceTypeInfo, Parent.Lookups.InvoiceTypeList, Res.GetString("9219D4D8-48CF-4CCF-8423-2D0555460382", "Invoice Type"));
	}

	protected override void CheckJZ_TotNoOfInvPages()
	{
		base.CheckJZ_TotNoOfInvPages();

		MandatoryValidation.MessageErrorIfIsZero(Parent.JZ_TotNoOfInvPagesInfo, Res.GetString("5761A051-51EC-45FB-8C12-75F25D545D15", "Total No of Pages"));
	}
}
