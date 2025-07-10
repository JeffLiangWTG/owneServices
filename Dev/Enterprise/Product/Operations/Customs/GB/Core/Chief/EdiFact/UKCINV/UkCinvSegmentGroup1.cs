using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCINV
{
	public class UkCinvSegmentGroup1 : SegmentGroup
	{
		public UkCinvSegmentGroup1()
		{
			LOC = new SegmentMessageSection<LOCSegment>(1);
			RFF = new SegmentMessageSection<RFFSegment>(2);
			GEI = new SegmentMessageSection<GEISegment>(4);
			CNT = new SegmentMessageSection<CNTSegment>(1);
			MEA = new SegmentMessageSection<MEASegment>(1);
			CST = new SegmentMessageSection<CSTSegment>(1);
			AUT = new SegmentMessageSection<AUTSegment>(1);
			DTM = new SegmentMessageSection<DTMSegment>(1);
		}

		public readonly SegmentMessageSection<LOCSegment> LOC;
		public readonly SegmentMessageSection<RFFSegment> RFF;
		public readonly SegmentMessageSection<GEISegment> GEI;
		public readonly SegmentMessageSection<CNTSegment> CNT;
		public readonly SegmentMessageSection<MEASegment> MEA;
		public readonly SegmentMessageSection<CSTSegment> CST;
		public readonly SegmentMessageSection<AUTSegment> AUT;
		public readonly SegmentMessageSection<DTMSegment> DTM;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { LOC, RFF, GEI, CNT, MEA, CST, AUT, DTM };
		}
	}
}
