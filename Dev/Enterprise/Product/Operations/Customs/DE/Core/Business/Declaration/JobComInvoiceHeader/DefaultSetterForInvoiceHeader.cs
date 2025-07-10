namespace Enterprise.Customs.DE.Business.Declaration
{
	public class DefaultSetterForInvoiceHeader : EU.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(JobComInvoiceHeader child, JobDeclaration declaration)
			: base(child, declaration)
		{
		}

		protected override void DefaultForNewElementCore()
		{
			base.DefaultForNewElementCore();

			var jobComInvoiceHeader = (JobComInvoiceHeader)newElement;
			jobComInvoiceHeader.CopyConsigneeAddressFromDeclarationImporter();
		}
	}
}
