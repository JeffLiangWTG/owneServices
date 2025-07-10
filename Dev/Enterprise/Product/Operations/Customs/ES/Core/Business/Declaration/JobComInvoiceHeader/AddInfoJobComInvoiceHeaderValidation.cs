namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AddInfoJobComInvoiceHeaderValidation : EU.Business.Declaration.AddInfoJobComInvoiceHeaderValidation
	{
		public AddInfoJobComInvoiceHeaderValidation(EU.Business.Declaration.AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected JobComInvoiceHeader Header => (JobComInvoiceHeader)Parent.Parent;

		protected override bool IsMandatoryZG_AgreedPlaceCodeCore => !Header.JZ_IncoTerm.IsEmpty && Header.HasAnyDiffEXSEntry();
	}
}
