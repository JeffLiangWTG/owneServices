using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceLinePackageValidation : Customs.Business.InvoiceLinePackageValidation
{
	public InvoiceLinePackageValidation(BaseCusLinkPackage package, JobComInvoiceLine invoiceLine) : base(package, invoiceLine)
	{
	}

	new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	protected override void CheckIsLinked()
	{
		base.CheckIsLinked();
		CheckNumberOfDifferentPackageTypes();
	}

	void CheckNumberOfDifferentPackageTypes()
	{
		var declaration = InvoiceLine.Declaration;
		var package = Parent.Package;
		if (package == null || declaration == null || !Parent.IsLinked)
		{
			return;
		}

		if (declaration.Configuration.IsUCC6(declaration))
		{
			var entryLinePackages = InvoiceLine.CusEntryLine?.PackagingDetails.Cast<InvoiceLinePackagePivot>() ?? System.Array.Empty<InvoiceLinePackagePivot>();
			ValidateNumberOfDifferentPackageTypes(entryLinePackages, MaxNumberOfDifferentPackageTypeAllowedUCC6, NotificationType.MessageError, ValidationCaptions.InvoiceLine.YouHaveSelectedMoreThan99PackageType);
		}
		else
		{
			var invoiceLinePackages = InvoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>();
			ValidateNumberOfDifferentPackageTypes(invoiceLinePackages, MaxNumberOfDifferentPackageTypeAllowedNonUCC6, NotificationType.Warning, ValidationCaptions.InvoiceLine.YouHaveSelectedMoreThanOnePackageType);
		}
	}

	void ValidateNumberOfDifferentPackageTypes(IEnumerable<InvoiceLinePackagePivot> packagePivotList, ZInt maxNumberOfDifferentPackageTypeAllowed, CargoWise.ComponentModel.INotificationType validationType, ZString validationMessage)
	{
		if (packagePivotList.Select(x => x.Package?.CW_PackType ?? ZString.Empty).Distinct().Skip(maxNumberOfDifferentPackageTypeAllowed).Any())
		{
			Parent.IsLinkedInfo.AddNotification(validationType, validationMessage);
		}
	}

	const int MaxNumberOfDifferentPackageTypeAllowedUCC6 = 99;
	const int MaxNumberOfDifferentPackageTypeAllowedNonUCC6 = 1;
}
