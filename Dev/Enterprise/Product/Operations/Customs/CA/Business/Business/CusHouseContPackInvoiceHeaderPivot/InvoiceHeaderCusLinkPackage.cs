using System;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class InvoiceHeaderCusLinkPackage : Customs.Business.BaseCusLinkPackage
	{
		public InvoiceHeaderCusLinkPackage(JobComInvoiceHeader invoice)
			: base(invoice)
		{
			Invoice = invoice;
		}

		public readonly JobComInvoiceHeader Invoice;

		public override ZBool IsLinked
		{
			get { return base.IsLinked; }
			set
			{
				if (IsLinked != value)
				{
					var previousPackQty = PackQty;
					var oldValue = IsLinked;

					base.IsLinked = value;
					ValidatePackage();
					RefreshBindingOfFirstPackagePropertyIfNeeded(oldValue, () => IsLinked, JobComInvoiceHeader.Schema.FirstPackageIsLinked);

					if (previousPackQty != 0 || PackQty != 0)
					{
						Invoice.MarkDeclarationRequiresMerge();
					}
				}
			}
		}

		void ValidatePackage()
		{
			var declaration = Invoice.JobDeclaration;
			declaration?.Packages?.MarkAsNeedingValidation();
		}

		public override ZInt PackQty
		{
			get => base.PackQty;
			set
			{
				if (PackQty != value)
				{
					var oldValue = PackQty;
					base.PackQty = value;
					RefreshBindingOfFirstPackagePropertyIfNeeded(oldValue, () => PackQty, JobComInvoiceHeader.Schema.FirstPackageQty);
					Invoice.MarkDeclarationRequiresMerge();
				}
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("DA31207C-1FF9-482C-9223-98C8BE8AB6FB", "Package"); }
		}

		void RefreshBindingOfFirstPackagePropertyIfNeeded(IZType oldValue, Func<IZType> getNewValue, string propertyName)
		{
			if (!IsCopying)
			{
				var newValue = getNewValue();
				if (oldValue != newValue)
				{
					var item = Invoice.FirstCusLinkPackage;
					if (object.ReferenceEquals(item, this))
					{
						var info = Invoice.ZPropertyInfoHash.GetPropertySafe(propertyName);
						info?.RefreshBinding(oldValue);
					}
				}
			}
		}
	}
}
