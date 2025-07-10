using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExport5ASItem
	{
		ZString EntryLineNo { get; }
		ZString LineAmendType { get; }
		ZString AmendDataItemID { get; }
		ZString LineDetailNo { get; }
		ZString ContainerSequenceNo { get; }
		ZInt SequenceNo { get; }
		ZString VINSequenceNo { get; }
		ZString BeforeDescription { get; }
		ZString AfterDescription { get; }
		ZString RegulationCategorySequnceNo { get; }
	}
}
