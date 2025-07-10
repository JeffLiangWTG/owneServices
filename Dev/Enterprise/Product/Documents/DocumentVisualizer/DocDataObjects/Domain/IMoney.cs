using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IMoney
	{
		ZDecimal Amount { get; set; }
		ICodeDescription Currency { get; }
	}
}