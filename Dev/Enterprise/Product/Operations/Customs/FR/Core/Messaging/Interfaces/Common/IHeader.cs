
using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag:Entete
	///</summary>
	public interface IHeader
	{
		///<summary>
		/// Xml Tag:codact
		///</summary>
		ZString ActionCode { get; }

		///<summary>
		/// Xml Tag:Motivation
		///</summary>
		IMotivation Motivation { get; }

		///<summary>
		/// Xml Tag:References
		///</summary>
		IReferences References { get; }
	}
}
