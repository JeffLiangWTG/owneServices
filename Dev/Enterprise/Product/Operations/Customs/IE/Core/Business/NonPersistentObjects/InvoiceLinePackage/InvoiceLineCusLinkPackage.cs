using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class InvoiceLineCusLinkPackage : Customs.Business.BaseCusLinkPackage
	{
		public InvoiceLineCusLinkPackage(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
			InvoiceLine = invoiceLine;
		}
		JobComInvoiceLine InvoiceLine { get; }

		public new Package Package => (Package)base.Package;

		public override ZBool IsLinked
		{
			get => base.IsLinked;
			set
			{
				var oldValue = IsLinked;
				base.IsLinked = value;
				if (!IsCopying)
				{
					var newValue = IsLinked;

					if (newValue)
					{
						if (InvoiceLine is JobComInvoiceLine invoiceLine && invoiceLine.OverallPackageType == PackageType.Packed)
						{
							var isFirstLink = !invoiceLine.Declaration?.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.PK != invoiceLine.PK).Any(x => x.PackagesPivot.Cast<Customs.Business.InvoiceLinePackagePivot>().Any(p => p.CHC_CW == PackagePk)) ?? false;
							if (isFirstLink)
							{
								invoiceLine.ZG_IsMainPack = true;
							}
						}
					}
					else
					{
						if (oldValue != newValue && Pivot is Customs.Business.ICusPackagePivot pivot && pivot.NumberOfPacks > ZInt.Zero)
						{
							pivot.NumberOfPacks = ZInt.Zero;
						}
					}
				}
			}
		}

		protected override bool GetIsPackQty_ReadOnlyCore() => base.GetIsPackQty_ReadOnlyCore() || Package is Package package && package.IsExportDeclaration && package.IsBulk;
	}
}
