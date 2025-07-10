using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IMeasurement
	{
		ZDecimal Value { get; set; }
		ICodeDescription Unit { get; }
	}
}