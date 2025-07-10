using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class TransportChargesWrapper : ITransportCharges
	{
		TransportChargesWrapper(string methodOfPayment)
		{
			this.methodOfPayment = Argument.NotNullOrEmpty(methodOfPayment, nameof(methodOfPayment));
		}

		readonly string methodOfPayment;

		public static TransportChargesWrapper New(string methodOfPayment) => string.IsNullOrEmpty(methodOfPayment) ? null : new TransportChargesWrapper(methodOfPayment);

		public string MethodOfPayment => methodOfPayment;
	}
}
