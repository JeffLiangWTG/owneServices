using CargoWise.Types;

namespace Enterprise.Registry.Business.Warehouse
{
	public interface IPutawaySequence
	{
		ZByte ClientArea { get; }
		ZByte Column { get; }
		ZByte Location { get; }
		ZByte ProductArea { get; }
		ZByte Level { get; }
		ZByte PickFace { get; }
		ZByte Row { get; }
	}
}
