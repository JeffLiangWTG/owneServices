using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSMCAR
{
	public class SegmentGroup9 : SegmentGroup
	{
		public SegmentGroup9()
		{
			TDT = new SegmentMessageSection<TDTSegment>(1);
			Group10 = new SegmentGroupMessageSection<SegmentGroup10>(9999);
		}

		public readonly SegmentMessageSection<TDTSegment> TDT;
		public readonly SegmentGroupMessageSection<SegmentGroup10> Group10;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { TDT, Group10 };
		}
	}
}
