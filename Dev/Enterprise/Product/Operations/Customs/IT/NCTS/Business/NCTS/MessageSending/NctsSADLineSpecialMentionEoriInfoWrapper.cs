using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADLineSpecialMentionEoriInfoWrapper : ISpecialMentionEoriInfo
{
	public ZString FirstEoriCode => ZString.Empty;

	public ZString SecondEoriCode => ZString.Empty;

	public ZDecimal? PreviousInvoiceAmount => null;
}
