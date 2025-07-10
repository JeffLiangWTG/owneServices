using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IControlResult
	{
		ZString Description { get; }
		ZString DescriptionLNG { get; }
		ZString ControlIndicator { get; }
		ZString PointerToTheAttribute { get; }
		ZString CorrectedValue { get; }
	}
}
