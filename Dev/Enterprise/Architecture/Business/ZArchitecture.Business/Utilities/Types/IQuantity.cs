using CargoWise.Types;

namespace Enterprise.ZArchitecture
{
	public interface IQuantity
	{
		ZDecimal Amount { get; }
		ZString Unit { get; }
		bool IsEmpty { get; }
		ZBool IsValid { get; }  //Used in ZWeight and ZVolume only when unit is of unappropriate type
	}
}
