using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5BALine
	{
		[ID()]
		ZInt EntryLineNo { get; }
		[DataItemID("01")]
		ZString HSDescription { get; }
		[DataItemID("02")]
		ZString InvoiceDescription { get; }
		[DataItemID("03")]
		ZString HSCode { get; }
	}
}
