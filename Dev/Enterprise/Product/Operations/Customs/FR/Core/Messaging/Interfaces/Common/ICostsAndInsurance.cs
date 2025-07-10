namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	public interface ICostsAndInsurance
	{
		///<summary>
		/// Xml Tag:Frais
		///</summary>
		IAmountAndCurrency Costs { get; }

		///<summary>
		/// Xml Tag:Assurance
		///</summary>
		IAmountAndCurrency Insurance { get; }
	}
}
