using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class CUKFSAMessage : SegmentGroup
	{
		public CUKFSAMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegmentWithReferenceAndDates>(1);
			FTX = new SegmentMessageSection<FTXSegment>(1);
			Group1 = new SegmentGroupMessageSection<CUKFSASegmentGroup1>(999);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<BGMSegmentWithReferenceAndDates> BGM;
		public readonly SegmentGroupMessageSection<CUKFSASegmentGroup1> Group1;
		public readonly SegmentMessageSection<FTXSegment> FTX;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, FTX, Group1, UNT };
		}
	}
}
