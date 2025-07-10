using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.Declaration;

public class AddInfoJobComInvoiceHeaderValidation : EU.Business.Declaration.AddInfoJobComInvoiceHeaderValidation
{
	public AddInfoJobComInvoiceHeaderValidation(EU.Business.Declaration.AddInfoJobComInvoiceHeader parent) : base(parent)
	{
	}

	protected new AddInfoJobComInvoiceHeader Parent => (AddInfoJobComInvoiceHeader)base.Parent;

	protected override void CheckZG_TransportChargesMethodOfPayment()
	{
		base.CheckZG_TransportChargesMethodOfPayment();

		if (Parent.Parent.IsExport)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_TransportChargesMethodOfPaymentInfo);
		}
	}
}
