using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class CUKFSASegmentGroup6 : SegmentGroup
	{
		public CUKFSASegmentGroup6()
		{
			CST = new SegmentMessageSection<CSTSegment>(1);
			FTX = new SegmentMessageSection<FTXSegment>(2);
			DTM = new SegmentMessageSection<DTMSegment>(1);
			RFF = new SegmentMessageSection<RFFSegmentWithDatetime>(5);
			QTY = new SegmentMessageSection<QTYSegment>(1);
		}

		public readonly SegmentMessageSection<CSTSegment> CST;
		public readonly SegmentMessageSection<QTYSegment> QTY;
		public readonly SegmentMessageSection<DTMSegment> DTM;
		public readonly SegmentMessageSection<FTXSegment> FTX;
		public readonly SegmentMessageSection<RFFSegmentWithDatetime> RFF;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { CST, FTX, DTM, RFF, QTY };
		}
	}
}
