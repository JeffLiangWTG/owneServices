using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADSpecialMentionEoriInfoWrapper : ISpecialMentionEoriInfo
{
	public SADSpecialMentionEoriInfoWrapper(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	public ZString FirstEoriCode => FirstEoriCodeCore;
	protected virtual ZString FirstEoriCodeCore => ZString.Empty;

	public ZString SecondEoriCode => ZString.Empty; //TODO: TO BE IMPLEMENTED

	public ZDecimal? PreviousInvoiceAmount => PreviousInvoiceAmountCore;
	protected virtual ZDecimal? PreviousInvoiceAmountCore => null;

	protected CusEntryHeader entryHeader;
}
