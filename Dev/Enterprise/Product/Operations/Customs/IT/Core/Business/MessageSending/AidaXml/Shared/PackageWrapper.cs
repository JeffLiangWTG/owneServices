using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public class PackageWrapper : IPackage
{
	public PackageWrapper(InvoiceLinePackagePivot invoiceLinePackagePivot)
	{
		this.invoiceLinePackagePivot = Argument.NotNull(invoiceLinePackagePivot, nameof(invoiceLinePackagePivot));
		package = Argument.NotNull(invoiceLinePackagePivot.Package, nameof(invoiceLinePackagePivot.Package));
	}

	readonly InvoiceLinePackagePivot invoiceLinePackagePivot;
	readonly Package package;

	string IPackage.PackageType => package.CW_PackType;

	int? IPackage.NumberOfPacks => package.IsBulk ? null : invoiceLinePackagePivot.CHC_NumberOfPacks;

	string IPackage.MarksAndNumbers => package.CW_MarksAndNos;
}
