using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class CUKFSASegmentGroup5 : SegmentGroup
	{
		public CUKFSASegmentGroup5()
		{
			GDS = new SegmentMessageSection<GDSSegment>(1);
			QTY = new SegmentMessageSection<QTYSegment>(2);
			MEA = new SegmentMessageSection<MEASegment>(1);
			DTM = new SegmentMessageSection<DTMSegment>(1);
			FTX = new SegmentMessageSection<FTXSegment>(1);
			RFF = new SegmentMessageSection<RFFSegmentWithDatetime>(2);
			MOA = new SegmentMessageSection<MOASegment>(1);
			DTM2 = new SegmentMessageSection<DTMSegment>(1);
		}

		public readonly SegmentMessageSection<GDSSegment> GDS;
		public readonly SegmentMessageSection<QTYSegment> QTY;
		public readonly SegmentMessageSection<MEASegment> MEA;
		public readonly SegmentMessageSection<DTMSegment> DTM;
		public readonly SegmentMessageSection<FTXSegment> FTX;
		public readonly SegmentMessageSection<RFFSegmentWithDatetime> RFF;
		public readonly SegmentMessageSection<MOASegment> MOA;
		public readonly SegmentMessageSection<DTMSegment> DTM2;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { GDS, QTY, MEA, DTM, FTX, RFF, MOA, DTM2 };
		}
	}
}
