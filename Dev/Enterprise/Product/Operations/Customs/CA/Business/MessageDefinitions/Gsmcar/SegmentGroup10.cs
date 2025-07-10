using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSMCAR
{
	public class SegmentGroup10 : SegmentGroup
	{
		public SegmentGroup10()
		{
			RFF = new SegmentMessageSection<RFFSegment>(1);
		}

		public readonly SegmentMessageSection<RFFSegment> RFF;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { RFF };
		}
	}
}
