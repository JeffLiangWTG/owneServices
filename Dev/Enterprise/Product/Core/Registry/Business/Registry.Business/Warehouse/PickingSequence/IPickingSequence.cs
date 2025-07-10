using CargoWise.Types;

namespace Enterprise.Registry.Business.Warehouse
{
	public interface IPickingSequence
	{
		ZByte BrokenPallets { get; }
		ZByte Column { get; }
		ZByte ConsolidatedPallets { get; }
		ZByte ExpiryDate { get; }
		ZByte FifoFallback { get; }
		ZByte FifoBulkOnly { get; }
		ZByte FifoOption { get; }
		ZBool IsPickfaceEmptyPreventPickingFromBulk { get; }
		ZByte FullPallets { get; }
		ZByte Level { get; }
		ZByte PalletOverflow { get; }
		ZByte PickFaces { get; }
		ZByte Row { get; }
		ZByte HighPriorityLocations { get; }
	}
}
