
using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag:Colisage
	///</summary>
	public interface IPacking
	{
		///<summary>
		/// Xml Tag:nbrcol
		///</summary>
		ZInt Count { get; }

		///<summary>
		/// Xml Tag:nbrpieces
		///</summary>
		ZInt ItemsCount { get; }

		///<summary>
		/// Xml Tag:natcol
		///</summary>
		ZString Type { get; }

		///<summary>
		/// Xml Tag:marquecolis
		///</summary>
		ZString MarksAndNos { get; }
	}
}
