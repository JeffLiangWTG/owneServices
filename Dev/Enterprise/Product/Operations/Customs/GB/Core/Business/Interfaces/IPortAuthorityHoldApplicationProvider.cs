using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.HoldAdderRemoverClearer
{
	public enum AddOrRemove
	{
		HoldAdd,
		HoldRemove,
		Cleared
	}

	public interface IPortAuthorityHoldApplicationProvider
	{
		string HoldType { get; }
		string HoldAuthority { get; }
		AddOrRemove DirectionOfApplication { get; }
		ZDateTime Date { get; }
	}
}
