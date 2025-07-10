
using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag:Colisage
	///</summary>
	public interface IDeliveryTerms
	{
		///<summary>
		/// Xml Tag:codliv
		///</summary>
		ZString IncotermCode { get; }

		///<summary>
		/// Xml Tag:lieuliv
		///</summary>
		ZString DeliveryPlace { get; }

		///<summary>
		/// Xml Tag:codelieuincoterm
		///</summary>
		ZString IncotermPlace { get; }
	}
}
