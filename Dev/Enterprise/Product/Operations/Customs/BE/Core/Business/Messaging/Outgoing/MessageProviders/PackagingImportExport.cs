using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using CusEntryLine = Enterprise.Customs.BE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.BE.Business;

public class PackagingImportExport : ITPackagingImportExport
{
	public PackagingImportExport(InvoiceLinePackagePivot package)
	{
		Argument.NotNull(package, InvoiceLinePackagePivot.Schema.TableName);
		this.package = package;
	}

	readonly InvoiceLinePackagePivot package;

	public static IEnumerable<ITPackagingImportExport> ExtractPackages(CusEntryLine entryLine)
	{
		foreach (var package in entryLine.InvoiceLines.SelectMany(ji => (ji as JobComInvoiceLine).PackagesPivot))
		{
			yield return new PackagingImportExport((InvoiceLinePackagePivot)package);
		}
	}

	public ZString MarksNumber { get => package.Factory.Load<BasePackage>(package.CHC_CW).CW_MarksAndNos; }
	public ZDecimal Packages { get => new ZDecimal(package.CHC_NumberOfPacks); }
	public ZString PackageType { get => package.Factory.Load<BasePackage>(package.CHC_CW).CW_PackType; }
}
