using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSMCAR
{
	public class SegmentGroup14 : SegmentGroup
	{
		public SegmentGroup14()
		{
			EQD = new SegmentMessageSection<EQDSegment>(1);
		}

		public readonly SegmentMessageSection<EQDSegment> EQD;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { EQD };
		}
	}
}
