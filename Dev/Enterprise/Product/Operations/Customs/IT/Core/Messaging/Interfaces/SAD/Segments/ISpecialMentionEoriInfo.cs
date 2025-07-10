using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface ISpecialMentionEoriInfo
{
	ZString FirstEoriCode { get; }
	ZString SecondEoriCode { get; }
	ZDecimal? PreviousInvoiceAmount { get; }
}
