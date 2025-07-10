using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	internal class ExportInvoiceLinePackageValidation : InvoiceLinePackageValidation
	{
		public ExportInvoiceLinePackageValidation(Customs.Business.BaseCusLinkPackage package, JobComInvoiceLine invoiceLine) : base(package, invoiceLine)
		{
		}

		protected override void CheckPackQty()
		{
			base.CheckPackQty();
			if (Parent.PackQty > ZInt.Zero && Parent.IsLinked && InvoiceLine.EntryInstruction is CusEntryInstruction instruction)
			{
				var invoiceLinePK = InvoiceLine.PK;
				var packagePK = Parent.PackagePk;
				if (instruction.MainPackInvoiceLines.FirstOrDefault(line => line.PK != invoiceLinePK && line.PackagesPivot.Cast<Customs.Business.InvoiceLinePackagePivot>().Any(c => c.CHC_CW == packagePK)) is JobComInvoiceLine otherInvoiceLine)
				{
					Parent.PackQtyInfo.AddMessageError(Res.GetString("{086D6551-C5CC-4C41-9A22-05472300B51E}", "There is an Invoice Line ({0}) marked as ‘Is Main Pack’ linked to this package, this requires all other Pack quantities linked to this package to be 0.", otherInvoiceLine.InvoiceAndLineReference));
				}
			}
		}
	}
}
