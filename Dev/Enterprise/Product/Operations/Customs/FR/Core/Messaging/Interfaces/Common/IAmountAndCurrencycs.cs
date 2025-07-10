
using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	public interface IAmountAndCurrency
	{
		///<summary>
		/// Xml Tag:montant
		///</summary>
		ZDecimal Amount { get; }

		///<summary>
		/// Xml Tag:devfac
		///</summary>
		ZString Currency { get; }
	}
}
