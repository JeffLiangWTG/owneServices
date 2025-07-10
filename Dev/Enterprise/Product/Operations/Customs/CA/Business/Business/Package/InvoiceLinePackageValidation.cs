using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class InvoiceLinePackageValidation : Customs.Business.InvoiceLinePackageValidation
	{
		public InvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine)
			: base(package, invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine)) as JobComInvoiceLine;
		}
		readonly JobComInvoiceLine invoiceLine;

		protected override void CheckIsLinked()
		{
			base.CheckIsLinked();
			var package = Parent.Package;
			if (package != null && Parent.IsLinked)
			{
				var packType = package.CW_PackType;
				var headerPackTypes = invoiceLine.InvoiceHeader.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().Select(y => y.Package?.CW_PackType ?? ZString.Empty);
				if (headerPackTypes.Any() && !headerPackTypes.Contains(packType))
				{
					Parent.IsLinkedInfo.AddWarning(Res.GetString("E2DFF767-48B9-4502-BAC0-D619B4E53839", "The packages indicated on this line will override the packages indicated on the invoice header."));
				}
			}
		}

		protected override void CheckPackQty()
		{
			Parent.PackQtyInfo.ClearAllNotifications();
			base.CheckPackQty();
			var package = Parent.Package;
			if(package != null && Parent.IsLinked)
			{
				if (invoiceLine.InvoiceHeader.PackagesForInvoicesForBindingOnly.Cast<InvoiceHeaderCusLinkPackage>().Any(x => x.Package.PK == package.PK && x.IsLinked))
				{
					Parent.PackQtyInfo.AddMessageError(Res.GetString("438590A6-78BA-42DF-8E41-C46D946AF6C3", "Packages cannot be entered at both Invoice Header and Invoice Line levels. Please remove packages at the Invoice Line level if being entered for the entire invoice at the Invoice Header level."));
				}
			}
		}
	}
}
