using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSMCAR
{
	public class SegmentGroup8 : SegmentGroup
	{
		public SegmentGroup8()
		{
			RFF = new SegmentMessageSection<RFFSegment>(1);
			LOC = new SegmentMessageSection<LOCSegment>(1);
			GEI = new SegmentMessageSection<GEISegment>();
			FTX = new SegmentMessageSection<FTXSegment>(1);
			Group9 = new SegmentGroupMessageSection<SegmentGroup9>();
			Group11 = new SegmentGroupMessageSection<SegmentGroup11>();
			Group14 = new SegmentGroupMessageSection<SegmentGroup14>();
			Group15 = new SegmentGroupMessageSection<SegmentGroup15>();
		}

		public readonly SegmentMessageSection<RFFSegment> RFF;
		public readonly SegmentMessageSection<LOCSegment> LOC;
		public readonly SegmentMessageSection<GEISegment> GEI;
		public readonly SegmentMessageSection<FTXSegment> FTX;
		public readonly SegmentGroupMessageSection<SegmentGroup9> Group9;
		public readonly SegmentGroupMessageSection<SegmentGroup11> Group11;
		public readonly SegmentGroupMessageSection<SegmentGroup14> Group14;
		public readonly SegmentGroupMessageSection<SegmentGroup15> Group15;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { RFF, LOC, GEI, FTX, Group9, Group11, Group14, Group15 };
		}
	}
}
