using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class SegmentGroup2 : SegmentGroup
	{
		public SegmentGroup2()
		{
			CST = new SegmentMessageSection<CSTSegment>(1);
		}

		public readonly SegmentMessageSection<CSTSegment> CST;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { CST };
		}
	}
}
