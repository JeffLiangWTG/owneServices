using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration;

public class InvoiceLinePackageValidation : Customs.Business.InvoiceLinePackageValidation
{
	public InvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine)
		: base(package, invoiceLine)
	{
	}

	protected new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	protected sealed override void CheckPackQty()
	{
		base.CheckPackQty();
		if (Parent is BaseCusLinkPackage package && package.IsLinked)
		{
			CheckPackQtyCore();
		}
	}

	protected virtual void CheckPackQtyCore()
	{
		CheckRuleR0219();
		CheckRuleR0220();
		CheckRuleR0364();
	}

	public void CheckRuleR0220()
	{
		var parent = Parent;
		if (parent.PackQty.IsEmpty && parent.Package is Package package && package.IsBreakBulk && (ValidationDecider?.IsRuleR0220Active ?? false))
		{
			parent.PackQtyInfo.AddMessageError(Res.GetString("F3D4F25F-0067-4837-B969-D8D967CD1FB8", "[R0220] If packages are 0 (zero) the package type cannot be NE, NF or NG"));
		}
	}

	void CheckRuleR0219()
	{
		var parent = Parent;
		var packagePivots = InvoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>();
		if ((ValidationDecider?.IsRuleR0219Active ?? false) && !parent.PackQty.IsEmpty && packagePivots.Any(x => x.CHC_NumberOfPacks == 0))
		{
			parent.PackQtyInfo.AddMessageError(Res.GetString("B74F55A7-5CBE-4834-9C48-E6EDEB2E01D0", "[R0219] If any of the package qty lines = 0 (zero) no other package quantity greater than 0 (zero) is allowed for the item"));
		}
	}

	void CheckRuleR0364()
	{
		var parent = Parent;
		if ((ValidationDecider?.IsRuleR0364Active ?? false) && parent.Package is Package package && !package.IsBreakBulk && InvoiceLine.Declaration.InvoiceLines.Cast<JobComInvoiceLine>()
						.SelectMany(line => line.PackagesPivot.Cast<InvoiceLinePackagePivot>()).Where(c => c.Package.CW_MarksAndNos == parent.Package.CW_MarksAndNos).Sum(c => c.CHC_NumberOfPacks) == 0)
		{
			parent.PackQtyInfo.AddMessageError(Res.GetString("B74F55A7-5CBE-4834-9C48-E6EDEB2E01D8", "[R0364] At least one other line item must have a number of packages greater than 0 for the same package number when the type of packages are not NE, NF or NG"));
		}
	}

	IInvoiceLinePackageValidationDecider ValidationDecider => Parent.Factory.GetValue(ref validationDeciderCached, GetValidationDecider);
	CachedProperty<IInvoiceLinePackageValidationDecider> validationDeciderCached;

	IInvoiceLinePackageValidationDecider GetValidationDecider() => InvoiceLine.Declaration.Configuration.GetInvoiceLinePackageValidationDecider();
}
