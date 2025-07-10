using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class InvoiceLineCusLinkPackage : BaseCusLinkPackage
	{
		public InvoiceLineCusLinkPackage(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
			InvoiceLine = invoiceLine;
		}

		public readonly JobComInvoiceLine InvoiceLine;

		[ReadOnlyMember(nameof(IsLinked_ReadOnly))]
		public override ZBool IsLinked
		{
			get { return base.IsLinked; }
			set
			{
				if (IsLinked != value)
				{
					var previousPackQty = PackQty;

					base.IsLinked = value;
					ValidatePackage();

					if (previousPackQty != 0 || PackQty != 0)
					{
						InvoiceLine.InvoiceHeader.MarkDeclarationRequiresMerge();
					}
				}
			}
		}

		protected override bool IsLinked_ReadOnly => InvoiceLine.InvoiceHeader.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().Any(p => p.Package.PK == Package.PK);

		void ValidatePackage()
		{
			var declaration = InvoiceLine.Declaration;
			declaration?.Packages?.MarkAsNeedingValidation();
		}

		public override ZInt PackQty
		{
			get => base.PackQty;
			set
			{
				if (PackQty != value)
				{
					base.PackQty = value;
					InvoiceLine.InvoiceHeader.MarkDeclarationRequiresMerge();
				}
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("19B5A1D1-BC29-4456-B8DB-A1687DA6DEC4", "Package"); }
		}
	}
}
