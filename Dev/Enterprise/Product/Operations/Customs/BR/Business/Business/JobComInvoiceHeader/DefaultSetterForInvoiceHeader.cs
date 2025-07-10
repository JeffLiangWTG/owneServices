using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	class DefaultSetterForInvoiceHeader : Customs.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader child, BaseJobDeclaration declaration)
			: base(child, declaration)
		{
		}

		protected override void DefaultForNewElementCore()
		{
			base.DefaultForNewElementCore();
			if (newElement.JZ_OH_Buyer.IsEmpty && declaration.IsExport)
			{
				newElement.JZ_OH_Buyer = declaration.JE_OH_Importer;
			}
		}
	}
}
