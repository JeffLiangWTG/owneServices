
using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	public interface IWeight
	{
		///<summary>
		/// Xml Tag:montant
		///</summary>
		ZDecimal Weight { get; }

		///<summary>
		/// Xml Tag:devfac
		///</summary>
		ZString Unit { get; }
	}
}
