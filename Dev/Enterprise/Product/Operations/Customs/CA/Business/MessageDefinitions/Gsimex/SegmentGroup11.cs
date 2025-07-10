using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class SegmentGroup11 : SegmentGroup
	{
		public SegmentGroup11()
		{
			IMD = new SegmentMessageSection<IMDSegment>(1);
			RFF = new SegmentMessageSection<RFFSegment>(1);
		}

		public readonly SegmentMessageSection<IMDSegment> IMD;
		public readonly SegmentMessageSection<RFFSegment> RFF;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { IMD, RFF };
		}
	}
}
