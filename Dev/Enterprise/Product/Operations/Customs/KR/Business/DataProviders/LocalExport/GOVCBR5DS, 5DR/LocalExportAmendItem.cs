using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportAmendItem : ILocalExportAmendItem
	{
		public string DataItemNo { get; set; }
		public int ItemSequenceNumber { get; set; }
		public string AmendType { get; set; }
		public string BeforeValue { get; set; }
		public string AfterValue { get; set; }

		ZString ILocalExportAmendItem.DataItemNo => DataItemNo;
		ZInt ILocalExportAmendItem.ItemSequenceNumber => ItemSequenceNumber;
		ZString ILocalExportAmendItem.AmendType => AmendType;
		ZString ILocalExportAmendItem.BeforeValue => BeforeValue;
		ZString ILocalExportAmendItem.AfterValue => AfterValue;
	}
}
