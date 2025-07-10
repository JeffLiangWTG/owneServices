using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ExportAddInfoJobComInvoiceHeaderValidation : AddInfoJobComInvoiceHeaderValidation
	{
		public ExportAddInfoJobComInvoiceHeaderValidation(EU.Business.Declaration.AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override bool IsMandatoryZG_AgreedPlaceCodeCore
		{
			get
			{
				var declaration = Header.JobDeclaration;
				return base.IsMandatoryZG_AgreedPlaceCodeCore && declaration.ZG_AgreedPlaceCode.IsEmpty && Header.HasAnyDiffT2CAndT2LAndEXSEntry();
			}
		}

		protected override void CheckZG_TransportChargesMethodOfPayment()
		{
			if (Header.HasAnyDiffEXSEntry())
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZG_TransportChargesMethodOfPaymentInfo);
			}
		}
	}
}
