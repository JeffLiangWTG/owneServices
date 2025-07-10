using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DEPDATPackageProvider : IDEPDATPackage
	{
		public static DEPDATPackageProvider NewOrNull(NctsPackage package) => package != null ? new DEPDATPackageProvider(package) : null;

		DEPDATPackageProvider(NctsPackage package)
		{
			this.package = Argument.NotNull(package, nameof(package));
		}

		public int? GoodsItemNumber => Quantity == 0 && !package.IsBulk && !package.IsUnpacked ? NCTSProviderHelpers.FindGoodsItemWithMainPack(package) : null;

		public string Kind => package.B5_UnitType;

		public long? Quantity => !package.IsBulk ? (long?)package.B5_UnitCount : null;

		public string MarksNumber => package.B5_MarksAndNumbers;

		readonly NctsPackage package;
	}
}
