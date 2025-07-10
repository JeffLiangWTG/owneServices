using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public sealed class ConsignmentItemPackagingDataProvider : IPackaging
{
	public static IEnumerable<ConsignmentItemPackagingDataProvider> NewCollection(CusEntryLine entryLine) => entryLine?.InvoiceLines.Cast<JobComInvoiceLine>()?.SelectMany(x => x.PackagesPivot)?
		.Cast<InvoiceLinePackagePivot>()?.Where(x => x.CHC_CW.IsValid)?.GroupBy(x => x.CHC_CW)?.Select((package, index) => New(package, index + 1))
		?? Enumerable.Empty<ConsignmentItemPackagingDataProvider>();

	public static ConsignmentItemPackagingDataProvider New(IEnumerable<Customs.Business.InvoiceLinePackagePivot> packagePivots, int sequenceNumber)
	{
		return packagePivots == null || !packagePivots.Any() || packagePivots.First().Package == null ? null : new ConsignmentItemPackagingDataProvider(packagePivots, sequenceNumber);
	}

	ConsignmentItemPackagingDataProvider(IEnumerable<Customs.Business.InvoiceLinePackagePivot> packagePivots, int sequenceNumber)
	{
		this.packagePivots = Argument.NotNull(packagePivots, nameof(packagePivots));
		package = (Package)this.packagePivots.First().Package;
		this.sequenceNumber = sequenceNumber;
	}

	readonly IEnumerable<Customs.Business.InvoiceLinePackagePivot> packagePivots;
	readonly Package package;

	public int SequenceNumber => sequenceNumber;

	readonly int sequenceNumber;

	public string TypeOfPackages => package.CW_PackType;

	public int? NumberOfPackages => GetNumberOfPackages();

	public string ShippingMarks => package.CW_MarksAndNos;

	int? GetNumberOfPackages()
	{
		var numberOfPackages = packagePivots.Sum(x => x.CHC_NumberOfPacks);
		return package.IsBulkOnlyPackaging && numberOfPackages == 0 ? null : numberOfPackages;
	}

	public string BypackCorrelationIdentifier => null;

	public string BypackType => null;
}
