using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC044CPackagingProvider : IPackaging
	{
		public CC044CPackagingProvider(CusInvPack package)
		{
			this.package = Argument.NotNull(package, nameof(package));
		}

		public int SequenceNumber => package.B5_SequenceNumber;

		public string TypeOfPackages => StatusIsNew
			? package.B5_UnitType.ToString()
			: string.Empty;

		public int NumberOfPackages => StatusIsNew
			? package.B5_UnitCount.ToZInt()
			: ZInt.Zero;

		public string ShippingMarks => StatusIsNew
			? package.B5_MarksAndNumbers.ToString()
			: string.Empty;

		bool StatusIsNew => package.B5_TypeOfDifference == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;

		readonly CusInvPack package;
	}
}
