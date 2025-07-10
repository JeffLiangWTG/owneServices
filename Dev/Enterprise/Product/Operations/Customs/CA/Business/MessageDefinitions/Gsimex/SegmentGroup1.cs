using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class SegmentGroup1 : SegmentGroup
	{
		public SegmentGroup1()
		{
			RFF = new SegmentMessageSection<RFFSegment>(1);
			PAC = new SegmentMessageSection<PACSegment>(1);
		}

		public readonly SegmentMessageSection<RFFSegment> RFF;
		public readonly SegmentMessageSection<PACSegment> PAC;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { RFF, PAC };
		}
	}
}
