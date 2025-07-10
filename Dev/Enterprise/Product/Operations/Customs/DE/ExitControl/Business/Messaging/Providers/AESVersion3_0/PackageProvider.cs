using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class PackageProvider : IPackage
	{
		public PackageProvider(CusExitReportItem reportItem, string shipmentType)
		{
			this.reportItem = Argument.NotNull(reportItem, nameof(reportItem));
			package = Argument.NotNull(reportItem.Package, "reportItem.Package");
			packageDetailsNotMissing = shipmentType != A0131ATLASTypeOfShipment.Codes.FP;
		}
		protected readonly CusExitReportItem reportItem;
		readonly CusExitConsignmentPackage package;
		protected readonly bool packageDetailsNotMissing;

		public int? Quantity => packageDetailsNotMissing && IsSupportEmptyPackType ? (int)reportItem.ERI_Quantity : null;

		public bool IsSupportEmptyPackType => !PackageHelper.GetSingleCountPackageTypes(package.Factory).Contains(package.CXP_PackageType);

		public string Kind => packageDetailsNotMissing ? package.CXP_PackageType : null;

		public string MarksNumbers => packageDetailsNotMissing ? package.CXP_MarksAndNumbers : null;

		public int PositionNumber => package.CXP_Sequence;
	}
}
