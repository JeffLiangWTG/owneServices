using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: UniSpe
	///</summary>
	public interface ISupplementaryUnit
	{
		///<summary>
		/// Xml Tag: UniSpe
		///</summary>
		ZString Code { get; }

		///<summary>
		/// Xml Tag: qualifunispe
		///</summary>
		ZString Qualif { get; }

		///<summary>
		/// Xml Tag: nbrunispe
		///</summary>
		ZDecimal Qty { get; }
	}
}
