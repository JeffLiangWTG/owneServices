using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class SegmentGroup7 : SegmentGroup
	{
		public SegmentGroup7()
		{
			NAD = new SegmentMessageSection<NADSegment>(9999);
		}

		public readonly SegmentMessageSection<NADSegment> NAD;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { NAD };
		}
	}
}
