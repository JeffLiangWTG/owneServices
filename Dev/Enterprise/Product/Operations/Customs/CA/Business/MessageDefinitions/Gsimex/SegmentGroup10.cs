using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class SegmentGroup10 : SegmentGroup
	{
		public SegmentGroup10()
		{
			DOC = new SegmentMessageSection<DOCSegment>(1);
		}

		public SegmentMessageSection<DOCSegment> DOC;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { DOC };
		}
	}
}
