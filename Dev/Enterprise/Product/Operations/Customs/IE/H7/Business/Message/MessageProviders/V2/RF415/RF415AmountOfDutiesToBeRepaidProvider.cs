using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415AmountOfDutiesToBeRepaidProvider : IMoney
	{
		public RF415AmountOfDutiesToBeRepaidProvider(RF415MessageSendingObject messageSendingObject)
		{
			this.messageSendingObject = messageSendingObject;
		}

		readonly RF415MessageSendingObject messageSendingObject;

		public decimal Amount => messageSendingObject.Amount;

		public string Currency => Core.Constants.CurrencyCodes.EuropeanUnion;
	}
}
