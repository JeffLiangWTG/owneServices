using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCINV
{
	public partial class UkCinvMessage : SegmentGroup
	{
		public UkCinvMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegment>(1);
			AUT = new SegmentMessageSection<AUTSegment>(1);
			GEI = new SegmentMessageSection<GEISegment>(3);
			RFF1 = new SegmentMessageSection<RFFSegment>(4);
			CNT = new SegmentMessageSection<CNTSegment>(1);
			LOC = new SegmentMessageSection<LOCSegment>(1);
			DTM = new SegmentMessageSection<DTMSegment>(2);
			TDT = new SegmentMessageSection<TDTSegment>(1);
			UNS1 = new SegmentMessageSection<UNSSegment>(1);
			RFF2 = new SegmentMessageSection<RFFSegment>(100);
			Group1 = new SegmentGroupMessageSection<UkCinvSegmentGroup1>(100);
			UNS2 = new SegmentMessageSection<UNSSegment>(1);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<BGMSegment> BGM;
		public readonly SegmentMessageSection<AUTSegment> AUT;
		public readonly SegmentMessageSection<GEISegment> GEI;
		public readonly SegmentMessageSection<RFFSegment> RFF1;
		public readonly SegmentMessageSection<CNTSegment> CNT;
		public readonly SegmentMessageSection<LOCSegment> LOC;
		public readonly SegmentMessageSection<DTMSegment> DTM;
		public readonly SegmentMessageSection<TDTSegment> TDT;
		public readonly SegmentMessageSection<UNSSegment> UNS1;
		public readonly SegmentMessageSection<RFFSegment> RFF2;
		public readonly SegmentGroupMessageSection<UkCinvSegmentGroup1> Group1;
		public readonly SegmentMessageSection<UNSSegment> UNS2;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, AUT, GEI, RFF1, CNT, LOC, DTM, TDT, UNS1, RFF2, Group1, UNS2, UNT };
		}

		public string GetNthSegmentWhereNIsOneBased(int n)
		{
			return ValueItems.Length >= n ? ValueItems[n - 1].ToString() : "";
		}
	}
}
