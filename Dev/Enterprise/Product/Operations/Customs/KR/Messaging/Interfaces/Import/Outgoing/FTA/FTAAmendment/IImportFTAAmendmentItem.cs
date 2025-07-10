using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportFTAAmendmentItem
	{
		ZInt SequenceNo { get; }
		ZInt EntryLineNo { get; }
		ZInt InvoiceLineNo { get; }
		ZString AmendType { get; }
		ZString DataItemID { get; }
		ZString BeforeDescription { get; }
		ZString AfterDescription { get; }
	}
}
