using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IChargesIn5WN
	{
		public ZShort VersionNumber { get; }
		public ZString VersionDescription { get; }
		public ICharges RefundAmounts { get; }
	}
}
