using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IHVLVCustomsStatusPublisher
		{
			ZString HouseBillNumber { get; }

			BusinessObject MasterBill { get; }
		}
	}
}