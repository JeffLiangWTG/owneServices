using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class SegmentGroup4 : SegmentGroup
	{
		public SegmentGroup4()
		{
			MOA = new SegmentMessageSection<MOASegment>(1);
		}

		public readonly SegmentMessageSection<MOASegment> MOA;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { MOA };
		}
	}
}
