using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class SegmentGroup12 : SegmentGroup
	{
		public SegmentGroup12()
		{
			GID = new SegmentMessageSection<GIDSegment>(1);
			CST = new SegmentMessageSection<CSTSegment>(1);
			MEA = new SegmentMessageSection<MEASegment>(1);
			MOA = new SegmentMessageSection<MOASegment>(1);
		}

		public readonly SegmentMessageSection<GIDSegment> GID;
		public readonly SegmentMessageSection<CSTSegment> CST;
		public readonly SegmentMessageSection<MEASegment> MEA;
		public readonly SegmentMessageSection<MOASegment> MOA;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { GID, CST, MEA, MOA };
		}
	}
}
