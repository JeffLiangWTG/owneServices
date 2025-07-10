using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag:HorsValeur
	///</summary>
	public interface IAlternateCalcValue
	{
		///<summary>
		/// Xml Tag:horsvaleur
		///</summary>
		ZString CalcValue { get; }

		///<summary>
		/// Xml Tag:motiv
		///</summary>
		ZString Motivation { get; }
	}
}
