using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDeparturePayInfoCollection : EU.NCTS.Business.NctsDeparturePayInfoCollection
{
	public NctsDeparturePayInfoCollection(NctsDepartureMovementHeader master) : base(master)
	{
	}

	public new NctsDeparturePayInfo AddNew() => (NctsDeparturePayInfo)base.AddNew();

	public new NctsDeparturePayInfo this[int index] => (NctsDeparturePayInfo)base[index];

	public NctsDeparturePayInfo InsertOrUpdatePayInfo(Func<NctsDeparturePayInfo, bool> predicate, ZDecimal totalAmount, ZString transactionType, ZDateTime expirationDate, ZString incomingPayResponseNo, ZString methodOfPayment)
	{
		var departurePayInfo = this.Cast<NctsDeparturePayInfo>().FirstOrDefault(predicate);
		if (departurePayInfo == null)
		{
			departurePayInfo = AddNew();
			departurePayInfo.BPI_PaymentStatus = Customs.Business.CusEntryPayInfoStatusList.Codes.Pending;
		}
		departurePayInfo.BPI_IncomingPayResponseNo = incomingPayResponseNo;
		departurePayInfo.BPI_MethodOfPayment = methodOfPayment;
		departurePayInfo.BPI_PaymentAmount = totalAmount;
		departurePayInfo.BPI_TransactionType = transactionType;
		departurePayInfo.BPI_PaymentDate = expirationDate;
		return departurePayInfo;
	}
}
