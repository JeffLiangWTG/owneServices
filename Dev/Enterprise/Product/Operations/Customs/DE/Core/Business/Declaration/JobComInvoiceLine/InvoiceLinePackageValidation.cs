using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration
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

			var parent = Parent;
			if (invoiceLine.IsImport && parent.IsLinked)
			{
				if (!invoiceLine.AllLinkedPackagesHaveSamePackTypeAndMarks)
				{
					parent.IsLinkedInfo.AddMessageError(Res.GetString("D3486574-8117-43F4-A63B-818055348CDE", "Multiple Package Lines on one Invoice Line require to have same Package Type and Marks."));
				}
			}
		}

		protected override void CheckPackQty()
		{
			base.CheckPackQty();

			if (invoiceLine.IsExport && Pivot != null)
			{
				CheckPackQtyForExport(Parent);
			}
		}

		void CheckPackQtyForExport(BaseCusLinkPackage parent)
		{
			var package = parent.Package;
			if (package != null)
			{
				var declaration = invoiceLine.Declaration;
				var packType = package.CW_PackType;
				var singleCountPackageTypes = PackageHelper.GetSingleCountPackageTypes(parent.Factory);
				var maxPackQty = declaration.IsTransitionPeriodAES30 ? 99999 : 99999999;

				CompareValidation.CheckWithinRange(parent.PackQtyInfo, 0, maxPackQty);

				var singleCountMessage = Res.GetString("9004362E-5E8E-46BE-8BF1-24C00CA60228", "Package Type of {0} requires Pack Quantity to be 1.", packType);
				if (parent.PackQty == 0)
				{
					var existsOtherLineLinkedToThisPackage = declaration.InvoiceLines.Cast<JobComInvoiceLine>()
						.Any(line => line.PK != invoiceLine.PK && line.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(c => c.Package?.PK == package.PK));
					if (!existsOtherLineLinkedToThisPackage)
					{
						if (singleCountPackageTypes.Contains(packType))
						{
							parent.PackQtyInfo.AddMessageError(singleCountMessage);
						}
						else if (PackageHelper.GetMultipleCountPackageTypes(parent.Factory).Contains(packType))
						{
							parent.PackQtyInfo.AddMessageError(Res.GetString("CE0E1235-8AF9-4CF0-AEC1-7EA0793A15C7", "Package Type of {0} requires Pack Quantity to be between 1 and {1}.", packType, maxPackQty));
						}
					}
				}
				else if (parent.PackQty > 0)
				{
					var existsOtherLineLinkedToThisPackageIsMainPack = declaration.InvoiceLines.Cast<JobComInvoiceLine>()
						.Any(line => line.PK != invoiceLine.PK && line.JI_IsMainPack && line.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(c => c.Package?.PK == package.PK));
					if (existsOtherLineLinkedToThisPackageIsMainPack)
					{
						parent.PackQtyInfo.AddMessageError(Res.GetString("DB20C139-0C2D-4049-A79F-A1C47264CF92", "There is an Invoice Line marked as ‘Is Main Pack’ linked to this package, this requires all other Pack quantities linked to this package to be 0."));
					}
					else if (singleCountPackageTypes.Contains(packType) && parent.PackQty != 1)
					{
						parent.PackQtyInfo.AddMessageError(singleCountMessage);
					}
				}
			}
		}

		public override Type AutoValidationType => typeof(InvoiceLinePackageValidation);
	}
}
