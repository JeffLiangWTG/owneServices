using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class OutboundCONTRLMessage : SegmentGroup
	{
		public OutboundCONTRLMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			UCI = new SegmentMessageSection<Edifact.D04A.Segments.UCISegment>(1);
			Group1 = new SegmentGroupMessageSection<OutboundCONTRLSegmentGroup1>(99);
			FTX = new SegmentMessageSection<FTXSegment>(5);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<Edifact.D04A.Segments.UCISegment> UCI;
		public readonly SegmentGroupMessageSection<OutboundCONTRLSegmentGroup1> Group1;
		public readonly SegmentMessageSection<FTXSegment> FTX;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, UCI, Group1, FTX, UNT };
		}
	}
}
