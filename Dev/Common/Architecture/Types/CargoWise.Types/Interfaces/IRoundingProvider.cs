
namespace CargoWise.Types
{
	/// <summary>
	/// Interface used by ZDecimal for rounding
	/// </summary>
	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	public interface IRoundingProvider
	{
		/// <summary>
		/// Called when rounding a ZDecimal
		/// </summary>
		/// <param name="value"></param>
		/// <param name="decimalPlaces"></param>
		/// <returns></returns>
		decimal Round(decimal value, int decimalPlaces);
	}
}
