using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.Shared.MessageContracts;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class PackagingProvider : IPackaging
	{
		readonly AsycudaPack pack;
		public PackagingProvider(AsycudaPack pack)
		{
			this.pack = Argument.NotNull(pack, nameof(pack));
		}

		static readonly ImmutableHashSet<string> ExcludedPackageTypes = new HashSet<string>
		{
			RefCusCodeBulkPackageUnitType.BulkGas,
			RefCusCodeBulkPackageUnitType.BulkLiquid,
			RefCusCodeBulkPackageUnitType.BulkNodules,
			RefCusCodeBulkPackageUnitType.BulkLiquidGas,
			RefCusCodeBulkPackageUnitType.BulkGrains,
			RefCusCodeBulkPackageUnitType.BulkScrap,
			RefCusCodeBulkPackageUnitType.BulkPowders,
			RefCusCodeUnPackedPackageUnitType.Unpacked,
			RefCusCodeUnPackedPackageUnitType.UnpackedSingle,
			RefCusCodeUnPackedPackageUnitType.UnpackedMultiple
		}.ToImmutableHashSet();

		public string ShippingMarks => !ExcludedPackageTypes.Contains(pack.APA_PackUQ) ? pack.APA_MarksAndNumbers : null;
		public string TypeOfPackages => pack.APA_PackUQ;
		public int? NumberOfPackages => !ExcludedPackageTypes.Contains(pack.APA_PackUQ) ? pack.APA_PackQty : null;
	}
}
