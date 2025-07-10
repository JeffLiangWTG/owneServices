using CargoWise.Types;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface ICusSCAOceanBill : IBaseCusSCAOceanBill
			{
				ZInt CB_NoOfBills { get; }

				ICusSCAHouseCollection HouseBills { get; }

				ICusSCAContainerCollection Containers { get; }
			}
		}
	}
}
