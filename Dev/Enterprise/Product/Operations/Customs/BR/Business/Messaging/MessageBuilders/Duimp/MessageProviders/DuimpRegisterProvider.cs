using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class DuimpRegisterProvider : IDuimpRegister
	{
		public DuimpRegisterProvider(DuimpMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}
		readonly DuimpMessageSendingObject sendingObject;

		public int TotalItem => sendingObject.Header.MergedLines.Count;

		public IEnumerable<IPayment> Payments => (fPayments ?? (fPayments = CreateNewPayments().ToArray()));
		IPayment[] fPayments;

		IEnumerable<IPayment> CreateNewPayments() => sendingObject.Header.AllMergedLinesFees.GroupBy(x => x.CF_ChargeType).Select(fees => PaymentProvider.New(fees)).Where(x => !x.TaxType.IsNullOrEmpty() && x.TaxAmount > 0 );
	}
}
