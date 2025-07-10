using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class ConsignmentItemPackagingDataProvider : IPackaging
{
	public static IEnumerable<ConsignmentItemPackagingDataProvider> NewCollection(INctsPackageCollection<EU.NCTS.Business.NctsPackage, NctsCommonCargoDesc> packages)
		=> packages?.Cast<NctsPackage>().Select((package, index) => new ConsignmentItemPackagingDataProvider(package, index + 1));

	protected ConsignmentItemPackagingDataProvider(NctsPackage package, int sequenceNumber)
	{
		this.package = package;
		SequenceNumber = sequenceNumber;
	}
	protected readonly NctsPackage package;

	public int SequenceNumber { get; }

	public string TypeOfPackages => GetPackageValue(p => p.B5_UnitType);

	public int? NumberOfPackages =>  IsBulk ? null : GetPackageValue(p =>  p.B5_UnitCount)?.ToZInt();

	public string ShippingMarks => GetPackageValue(p => p.B5_MarksAndNumbers)?.ReturnNullIfEmpty();

	protected virtual T? GetPackageValue<T>(Func<NctsPackage, T> getter) where T : struct, IZType => getter(package);

	bool IsBulk => isBulk ??= IsBulkCore;
	bool? isBulk;

	protected virtual bool IsBulkCore => package.IsBulk;

	public string BypackCorrelationIdentifier => null;
	public string BypackType => null;
}
