using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportFTAAmendmentItem : IImportFTAAmendmentItem
	{
		public int SequenceNo { get; set; }
		public int EntryLineNo { get; set; }
		public int InvoiceLineNo { get; set; }
		public string AmendType { get; set; }
		public string DataItemID { get; set; }
		public string BeforeDescription { get; set; }
		public string AfterDescription { get; set; }

		ZInt IImportFTAAmendmentItem.SequenceNo => SequenceNo;
		ZInt IImportFTAAmendmentItem.EntryLineNo => EntryLineNo;
		ZInt IImportFTAAmendmentItem.InvoiceLineNo => InvoiceLineNo;
		ZString IImportFTAAmendmentItem.AmendType => AmendType;
		ZString IImportFTAAmendmentItem.DataItemID => DataItemID;
		ZString IImportFTAAmendmentItem.BeforeDescription => BeforeDescription;
		ZString IImportFTAAmendmentItem.AfterDescription => AfterDescription;
	}
}
