using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Export5ASItem : IExport5ASItem
	{
		public string EntryLineNo { get; set; }
		public string LineAmendType { get; set; }
		public string AmendDataItemID { get; set; }
		public string LineDetailNo { get; set; }
		public string ContainerSequenceNo { get; set; }
		public int SequenceNo { get; set; }
		public string VINSequenceNo { get; set; }
		public string BeforeDescription { get; set; }
		public string AfterDescription { get; set; }
		public string RegulationCategorySequnceNo { get; set; }

		ZString IExport5ASItem.EntryLineNo => EntryLineNo;
		ZString IExport5ASItem.LineAmendType => LineAmendType;
		ZString IExport5ASItem.AmendDataItemID => AmendDataItemID;
		ZString IExport5ASItem.LineDetailNo => LineDetailNo;
		ZString IExport5ASItem.ContainerSequenceNo => ContainerSequenceNo;
		ZInt IExport5ASItem.SequenceNo => SequenceNo;
		ZString IExport5ASItem.VINSequenceNo => VINSequenceNo;
		ZString IExport5ASItem.BeforeDescription => BeforeDescription;
		ZString IExport5ASItem.AfterDescription => AfterDescription;
		ZString IExport5ASItem.RegulationCategorySequnceNo => RegulationCategorySequnceNo;
	}
}
