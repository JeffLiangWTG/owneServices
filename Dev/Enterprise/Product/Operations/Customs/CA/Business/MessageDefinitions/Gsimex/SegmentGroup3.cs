using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class SegmentGroup3 : SegmentGroup
	{
		public SegmentGroup3()
		{
			NAD = new SegmentMessageSection<NADSegment>(1);
		}

		public SegmentMessageSection<NADSegment> NAD;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { NAD };
		}
	}
}
