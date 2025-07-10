using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSMCAR
{
	public class SegmentGroup7 : SegmentGroup
	{
		public SegmentGroup7()
		{
			CNI = new SegmentMessageSection<CNISegment>(1);
			DOC = new SegmentMessageSection<DOCSegment>(1);
			Group8 = new SegmentGroupMessageSection<SegmentGroup8>();
		}

		public readonly SegmentMessageSection<CNISegment> CNI;
		public readonly SegmentMessageSection<DOCSegment> DOC;
		public readonly SegmentGroupMessageSection<SegmentGroup8> Group8;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { CNI, DOC, Group8 };
		}
	}
}
