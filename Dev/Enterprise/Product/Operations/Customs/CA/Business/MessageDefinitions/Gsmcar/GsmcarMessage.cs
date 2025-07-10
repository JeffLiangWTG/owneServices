using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSMCAR
{
	public class GSMCARMessage : SegmentGroup
	{
		public SegmentMessageSection<UNHSegment> UNH;
		public SegmentMessageSection<BGMSegment> BGM;
		public SegmentMessageSection<CSTSegment> CST;
		public SegmentGroupMessageSection<SegmentGroup4> Group4;
		public SegmentGroupMessageSection<SegmentGroup7> Group7;
		public SegmentMessageSection<AUTSegment> AUT;
		public SegmentMessageSection<UNTSegment> UNT;

		public GSMCARMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegment>(1);
			CST = new SegmentMessageSection<CSTSegment>(1);
			Group4 = new SegmentGroupMessageSection<SegmentGroup4>();
			Group7 = new SegmentGroupMessageSection<SegmentGroup7>();
			AUT = new SegmentMessageSection<AUTSegment>(1);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, CST, Group4, Group7, AUT, UNT };
		}
	}
}
