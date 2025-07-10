using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class AddInfoJobComInvoiceHeader : EU.Business.Declaration.AddInfoJobComInvoiceHeader
	{
		public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoPropertyInfo)
			: base(addInfoPropertyInfo)
		{
		}

		public new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		public new AddInfoJobComInvoiceHeaderValidation Validation => (AddInfoJobComInvoiceHeaderValidation)base.Validation;

		protected override EUAddInfoValidation GetNewValidation()
		{
			if (Parent.IsExport)
			{
				return new ExportAddInfoJobComInvoiceHeaderValidation(this);
			}
			else if (Parent.IsImport)
			{
				return new ImportAddInfoJobComInvoiceHeaderValidation(this);
			}
			else
			{
				return new AddInfoJobComInvoiceHeaderValidation(this);
			}
		}
	}
}
