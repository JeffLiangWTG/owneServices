using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class InvoiceLinePackageValidation : Customs.Business.InvoiceLinePackageValidation
	{
		public InvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine)
			: base(package, invoiceLine)
		{
		}

		new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected override void CheckPackQty()
		{
			const int maxNumberOfPackQtyInProvisionalPeriod = 99999;

			var isTransitionPeriod = InvoiceLine.Declaration?.IsTransitionPeriodAES30 ?? false;

			if (isTransitionPeriod && Parent.PackQty > maxNumberOfPackQtyInProvisionalPeriod)
			{
				Parent.PackQtyInfo.AddMessageError(Res.GetString("67A693E2-FEF1-47BE-8F91-5FC730AF0D1F", "In provisional period, Pack Quantity cannot be greater than 99999."));
			}

			if (InvoiceLine.EntryInstruction?.IsEXS ?? false)
			{
				var package = Parent.Package;
				if (package != null)
				{
					var declaration = InvoiceLine.Declaration;

					if (Parent.PackQty == 0)
					{
						var existsOtherLineLinkedToThisPackage = declaration.InvoiceLines.Cast<JobComInvoiceLine>()
							.Any(line => line.PK != InvoiceLine.PK && (line.EntryInstruction?.IsEXS ?? false) && line.PackagesPivot.Cast<InvoiceLinePackagePivot>()
								.Any(c => c.Package.CW_PackType == package.CW_PackType && c.Package.CW_MarksAndNos == package.CW_MarksAndNos && c.Package.CW_PackQty > 0));
						if (!existsOtherLineLinkedToThisPackage)
						{
							MandatoryValidation.MessageErrorIfNotEntered(Parent.PackQtyInfo);
						}
					}
				}
			}
			else
			{
				base.CheckPackQty();
			}
		}
	}
}
