using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class SegmentGroup8 : SegmentGroup
	{
		public SegmentGroup8()
		{
			LIN = new SegmentMessageSection<LINSegment>(1);
			LOC = new SegmentMessageSection<LOCSegment>(1);
			RFF = new SegmentMessageSection<RFFSegment>(1);
			Group10 = new SegmentGroupMessageSection<SegmentGroup10>(9999);
			Group11 = new SegmentGroupMessageSection<SegmentGroup11>(9999);
			Group12 = new SegmentGroupMessageSection<SegmentGroup12>(9999);
		}

		public readonly SegmentMessageSection<LINSegment> LIN;
		public readonly SegmentMessageSection<LOCSegment> LOC;
		public readonly SegmentMessageSection<RFFSegment> RFF;
		public readonly SegmentGroupMessageSection<SegmentGroup10> Group10;
		public readonly SegmentGroupMessageSection<SegmentGroup11> Group11;
		public readonly SegmentGroupMessageSection<SegmentGroup12> Group12;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { LIN, LOC, RFF, Group10, Group11, Group12 };
		}
	}
}
