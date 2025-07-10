using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class EdecGoodsItemPackagingDataProvider : IEdecGoodsItemPackaging
{
	public static IEnumerable<EdecGoodsItemPackagingDataProvider> NewCollection(CusEntryLine entryLine)
	{
		if (entryLine != null)
		{
			foreach (var packagePivots in entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PackagesPivot)
				.Cast<InvoiceLinePackagePivot>().Where(x => x.CHC_CW.IsValid).GroupBy(x => x.CHC_CW).ToArray())
			{
				yield return New(packagePivots);
			}
		}
	}

	public static EdecGoodsItemPackagingDataProvider New(IEnumerable<Customs.Business.InvoiceLinePackagePivot> packagePivots)
	{
		return packagePivots == null || !packagePivots.Any() || packagePivots.First().Package == null ? null : new EdecGoodsItemPackagingDataProvider(packagePivots);
	}

	EdecGoodsItemPackagingDataProvider(IEnumerable<Customs.Business.InvoiceLinePackagePivot> packagePivots)
	{
		this.packagePivots = Argument.NotNull(packagePivots, nameof(packagePivots));
		package = this.packagePivots.First().Package;
	}

	readonly IEnumerable<Customs.Business.InvoiceLinePackagePivot> packagePivots;
	readonly BasePackage package;

	public string PackagingType => package.CW_PackType;

	public decimal Quantity => packagePivots.Sum(x => x.CHC_NumberOfPacks);

	public string PackagingReferenceNumber => package.CW_MarksAndNos;
}
