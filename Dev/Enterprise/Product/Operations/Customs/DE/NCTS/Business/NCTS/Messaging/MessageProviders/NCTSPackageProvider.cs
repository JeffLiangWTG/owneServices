using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSPackageProvider : INCTSPackage
	{
		public NCTSPackageProvider(NctsPackage package)
		{
			this.package = Argument.NotNull(package, nameof(package));
		}
		protected readonly NctsPackage package;

		public string Kind => package.B5_UnitType.ValueOrNullIfEmpty();

		public long? Quantity => package.B5_UnitCount;

		public string MarksNumber => package.B5_MarksAndNumbers.ValueOrNullIfEmpty();
	}
}
