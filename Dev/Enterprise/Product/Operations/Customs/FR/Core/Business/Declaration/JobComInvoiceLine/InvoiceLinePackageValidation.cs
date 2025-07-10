using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class InvoiceLinePackageValidation : Customs.Business.InvoiceLinePackageValidation
	{
		public InvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine) : base(package, invoiceLine)
		{
		}

		protected override void CheckIsLinked()
		{
			base.CheckIsLinked();

			if (!Parent.IsLinkedInfo.ReadOnly && !Parent.IsLinked)
			{
				Parent.IsLinkedInfo.AddWarning(Res.GetString("8DFF17E2-DAAA-4AD9-9542-BAEE5552C37D", "Packages line is not selected."));
			}
		}
	}
}
