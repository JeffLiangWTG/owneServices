using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using RefCusCodeBulkPackageUnitType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType;
using RefCusCodeUnPackedPackageUnitType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType;

namespace Enterprise.Customs.DE.Business
{
	public class ImportPackageProvider : IImportPackage
	{
		public static ImportPackageProvider NewOrNull(IEnumerable<InvoiceLinePackagePivot> pivots) => pivots.Any() ? new ImportPackageProvider(pivots) : null;

		ImportPackageProvider(IEnumerable<InvoiceLinePackagePivot> pivots)
		{
			this.pivots = pivots;
			randomPackage = pivots.First().Package;
			packTypes = pivots.Select(x => x.Package.CW_PackType.ToString());
		}
		readonly IEnumerable<InvoiceLinePackagePivot> pivots;
		readonly BasePackage randomPackage;
		readonly IEnumerable<string> packTypes;

		public int? Quantity => AllPackTypesAreCountable(packTypes) ? pivots.Sum(x => x.CHC_NumberOfPacks) : null;

		public string Kind => randomPackage.CW_PackType;

		public string MarksNumbers => AllPackTypesAreCountable(packTypes) ? randomPackage.CW_MarksAndNos.ToString() : string.Empty;

		bool AllPackTypesAreCountable(IEnumerable<string> packTypes) => CachedValueHelper.GetValue(ref allPackTypesAreCountable, () => packTypes.All(x => !notCountableTypes.Contains(x)));
		CachedValue<bool> allPackTypesAreCountable;

		readonly static ImmutableHashSet<string> notCountableTypes = ImmutableHashSet.Create(RefCusCodeUnPackedPackageUnitType.Unpacked, RefCusCodeUnPackedPackageUnitType.UnpackedSingle, RefCusCodeUnPackedPackageUnitType.UnpackedMultiple
			, RefCusCodeBulkPackageUnitType.BulkGas, RefCusCodeBulkPackageUnitType.BulkLiquid, RefCusCodeBulkPackageUnitType.BulkNodules, RefCusCodeBulkPackageUnitType.BulkLiquidGas
			, RefCusCodeBulkPackageUnitType.BulkGrains, RefCusCodeBulkPackageUnitType.BulkScrap, RefCusCodeBulkPackageUnitType.BulkPowders);
	}
}
