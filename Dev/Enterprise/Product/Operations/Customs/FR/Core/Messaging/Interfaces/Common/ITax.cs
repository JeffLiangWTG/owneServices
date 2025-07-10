using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	public interface ITax
	{
		///<summary>
		/// Xml Tag: codtax
		///</summary>
		ZString TaxCode { get; }

		///<summary>
		/// Xml Tag: typtax
		///</summary>
		ZString TaxType { get; }

		///<summary>
		/// Xml Tag: quotax
		///</summary>
		ZDecimal TaxRate { get; }

		///<summary>
		/// Xml Tag: asstax
		///</summary>
		ZDecimal TaxAssessed { get; }

		///<summary>
		/// Xml Tag: montanttax
		///</summary>
		ZDecimal TaxAmount { get; }

		///<summary>
		/// Xml Tag: statutLiquidation
		///</summary>
		ZString TaxMethodOfPayment { get; }

		///<summary>
		/// Xml Tag: codeport
		///</summary>
		ZString ChargePaymentOrDestinationID { get; }

		/// <summary>
		/// Xml Tag: statutliquidation
		/// </summary>
		ZString LiquidationStatus { get; }

		/// <summary>
		/// Xml Tag: codtaxeeu
		/// </summary>
		ZString EUTaxCode { get; }
	}
}
