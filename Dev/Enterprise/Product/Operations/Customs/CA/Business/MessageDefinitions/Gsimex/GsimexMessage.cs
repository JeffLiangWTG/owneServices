
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSIMEX
{
	public class GSIMEXMessage : SegmentGroup
	{
		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<BGMSegment> BGM;
		public readonly SegmentMessageSection<LOCSegment> LOC;
		public readonly SegmentMessageSection<DTMSegment> DTM;
		public readonly SegmentMessageSection<MEASegment> MEA;
		public readonly SegmentMessageSection<EQDSegment> EQD;
		public readonly SegmentMessageSection<TDTSegment> TDT;
		public readonly SegmentGroupMessageSection<SegmentGroup1> Group1;
		public readonly SegmentGroupMessageSection<SegmentGroup2> Group2;
		public readonly SegmentGroupMessageSection<SegmentGroup3> Group3;
		public readonly SegmentGroupMessageSection<SegmentGroup4> Group4;
		public readonly SegmentMessageSection<UNSSegment> UNS1;
		public readonly SegmentGroupMessageSection<SegmentGroup5> Group5;
		public readonly SegmentMessageSection<UNSSegment> UNS2;
		public readonly SegmentMessageSection<AUTSegment> AUT;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		public GSIMEXMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegment>(1);
			LOC = new SegmentMessageSection<LOCSegment>(1);
			DTM = new SegmentMessageSection<DTMSegment>(1);
			MEA = new SegmentMessageSection<MEASegment>(1);
			EQD = new SegmentMessageSection<EQDSegment>(1);
			TDT = new SegmentMessageSection<TDTSegment>(1);
			Group1 = new SegmentGroupMessageSection<SegmentGroup1>();
			Group2 = new SegmentGroupMessageSection<SegmentGroup2>();
			Group3 = new SegmentGroupMessageSection<SegmentGroup3>();
			Group4 = new SegmentGroupMessageSection<SegmentGroup4>();
			UNS1 = new SegmentMessageSection<UNSSegment>(1);
			Group5 = new SegmentGroupMessageSection<SegmentGroup5>();
			UNS2 = new SegmentMessageSection<UNSSegment>(1);
			AUT = new SegmentMessageSection<AUTSegment>(1);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, LOC, DTM, MEA, EQD, TDT, Group1, Group2, Group3, Group4, UNS1, Group5, UNS2, AUT, UNT };
		}
	}
}
