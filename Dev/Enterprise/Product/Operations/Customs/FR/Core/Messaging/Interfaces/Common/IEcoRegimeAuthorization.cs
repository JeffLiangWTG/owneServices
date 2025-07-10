using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	public interface IEcoRegimeAuthorization
	{
		///<summary>
		/// Xml Tag: autorisationeco
		///</summary>	
		ZString EcoRegimeAuthorizationNumber { get; }

		///<summary>
		/// Xml Tag: autorisationpays
		///</summary>	
		ZString EcoRegimeCountryCode { get; }
	}
}
