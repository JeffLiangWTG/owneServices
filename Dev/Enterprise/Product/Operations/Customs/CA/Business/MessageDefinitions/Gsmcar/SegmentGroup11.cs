using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSMCAR
{
	public class SegmentGroup11 : SegmentGroup
	{
		public SegmentGroup11()
		{
			NAD = new SegmentMessageSection<NADSegment>(1);
			Group12 = new SegmentGroupMessageSection<SegmentGroup12>();
		}

		public readonly SegmentMessageSection<NADSegment> NAD;
		public readonly SegmentGroupMessageSection<SegmentGroup12> Group12;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { NAD, Group12 };
		}
	}
}
