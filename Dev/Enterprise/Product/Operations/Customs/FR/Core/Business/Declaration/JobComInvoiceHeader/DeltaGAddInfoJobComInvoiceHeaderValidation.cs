using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGAddInfoJobComInvoiceHeaderValidation : AddInfoJobComInvoiceHeaderValidation
	{
		public DeltaGAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override void CheckZG_AgreedPlaceCode()
		{
			base.CheckZG_AgreedPlaceCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_AgreedPlaceCodeInfo);
		}
	}
}
