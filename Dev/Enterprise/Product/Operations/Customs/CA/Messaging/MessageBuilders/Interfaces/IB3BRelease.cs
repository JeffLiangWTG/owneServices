using CargoWise.Types;

namespace Enterprise.Customs.CA.Messaging
{
	public interface IB3BRelease
	{
		ZString CargoControlNumber { get; }
		ZDateTime DateOfRelease { get; }
	}
}
