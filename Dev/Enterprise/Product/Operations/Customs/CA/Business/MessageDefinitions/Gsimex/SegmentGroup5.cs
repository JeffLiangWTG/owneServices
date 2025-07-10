using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class SegmentGroup5 : SegmentGroup
	{
		public SegmentGroup5()
		{
			SEQ = new SegmentMessageSection<SEQSegment>();
			DMS = new SegmentMessageSection<DMSSegment>(1);
			GEI = new SegmentMessageSection<GEISegment>();
			Group7 = new SegmentGroupMessageSection<SegmentGroup7>(9999);
			Group8 = new SegmentGroupMessageSection<SegmentGroup8>(9999);
		}

		public readonly SegmentMessageSection<SEQSegment> SEQ;
		public readonly SegmentMessageSection<DMSSegment> DMS;
		public readonly SegmentMessageSection<GEISegment> GEI;
		public readonly SegmentGroupMessageSection<SegmentGroup7> Group7;
		public readonly SegmentGroupMessageSection<SegmentGroup8> Group8;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { SEQ, DMS, GEI, Group7, Group8 };
		}
	}
}
