using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class InvoiceLinePackageValidation : Customs.Business.InvoiceLinePackageValidation
{
	public InvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine) : base(package, invoiceLine)
	{
	}

	protected new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	protected override void CheckIsLinked()
	{
		base.CheckIsLinked();
		PlausiValidation.CheckNS30003_TransportEquipment(Parent, InvoiceLine);
	}

	PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New(InvoiceLine));
	PlausiValidation plausiValidation;
}
