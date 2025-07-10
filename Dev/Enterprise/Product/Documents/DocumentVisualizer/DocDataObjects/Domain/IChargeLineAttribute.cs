using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IChargeLineAttribute
	{
		ZString Name { get; set; }
		ZString Value { get; set; }
		ZDecimal Amount { get; set; }
	}
}
