using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	class B2AddInfoJobComInvoiceHeaderValidation : CommonImportAddInfoJobComInvoiceHeaderValidation
	{
		public B2AddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		#region CheckCA_RN_NKExport

		protected override void CheckCA_RN_NKExport()
		{
			base.CheckCA_RN_NKExport();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(InvoiceHeader.CA_RN_NKExportInfo);
		}

		#endregion
	}
}
