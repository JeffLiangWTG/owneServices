using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: LignePrecalc
	///</summary>
	public interface IPreCalcEntryLine
	{
		///<summary>
		/// Xml Tag: UniSpe
		///</summary>
		ISupplementaryUnit SuppUnit { get; }
		///<summary>
		/// Xml Tag: 
		///</summary>
		ITax Tax { get; }

		ZBool IsVAT { get; }
	}
}
