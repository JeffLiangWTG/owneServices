using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class ExportAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public ExportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		#region CheckCA_ConveyanceIdentificationNumber

		protected override void CheckCA_ConveyanceIdentificationNumber()
		{
			base.CheckCA_ConveyanceIdentificationNumber();
			if (Parent.InvoiceLine?.Tariff?.ConveyanceIDRequired ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_ConveyanceIdentificationNumberInfo);
			}
			Parent.InvoiceLine.Validation.ValidateJI_CustomsQuantity();
		}

		#endregion
	}
}
