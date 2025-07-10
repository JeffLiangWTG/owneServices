using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSMCAR
{
	public class SegmentGroup4 : SegmentGroup
	{
		public SegmentGroup4()
		{
			TDT = new SegmentMessageSection<TDTSegment>(1);
		}

		public readonly SegmentMessageSection<TDTSegment> TDT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { TDT };
		}
	}
}
