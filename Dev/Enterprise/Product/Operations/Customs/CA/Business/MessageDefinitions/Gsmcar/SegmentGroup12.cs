using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSMCAR
{
	public class SegmentGroup12 : SegmentGroup
	{
		public SegmentGroup12()
		{
			CTA = new SegmentMessageSection<CTASegment>(1);
			COM = new SegmentMessageSection<COMSegment>(1);
		}

		public readonly SegmentMessageSection<CTASegment> CTA;
		public readonly SegmentMessageSection<COMSegment> COM;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { CTA, COM };
		}
	}
}
