using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class PackagingProvider : IPackaging
	{
		readonly CusInvPack package;
		public PackagingProvider(CusInvPack package)
		{
			this.package = Argument.NotNull(package, nameof(package));
		}

		public int SequenceNumber => package.B5_SequenceNumber;

		public string TypeOfPackages => package.B5_UnitType;

		public int NumberOfPackages => package.B5_UnitCount.ToZInt();

		public string ShippingMarks => package.B5_MarksAndNumbers;
	}
}
