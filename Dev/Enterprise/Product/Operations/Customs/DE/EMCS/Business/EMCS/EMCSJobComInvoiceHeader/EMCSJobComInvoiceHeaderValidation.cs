namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSJobComInvoiceHeaderValidation : EU.EMCS.Business.EMCSJobComInvoiceHeaderValidation
	{
		public EMCSJobComInvoiceHeaderValidation(EU.EMCS.Business.EMCSJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override void CheckJZ_InvoiceNumberIsMandatory()
		{
			var declaration = (EMCSJobDeclaration)Parent.JobDeclaration;

			if (!declaration.IsConsolidatedDocument())
			{
				base.CheckJZ_InvoiceNumberIsMandatory();
			}
		}
	}
}
