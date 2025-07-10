using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Messages.CUSDEC;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSDEC_2_912
{
	public partial class CUSDECMessage : SegmentGroup
	{
		public CUSDECMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegmentWithReferenceAndDates>(1);
			Group1 = new SegmentGroupMessageSection<SegmentGroup1>(2);
			LOC = new SegmentMessageSection<LOCSegmentWithLotsOfLocations>(4);
			TDT = new SegmentMessageSection<TDTSegmentWithDatetime>(2);
			GIS = new SegmentMessageSection<GISSegment>(1);
			Group6 = new SegmentGroupMessageSection<CUSDECSegmentGroup6>(1);
			UNS1 = new SegmentMessageSection<UNSSegment>(1);
			UNS2 = new SegmentMessageSection<UNSSegment>(1);
			CNT = new SegmentMessageSection<CNTSegment>(1);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<BGMSegmentWithReferenceAndDates> BGM;
		public readonly SegmentGroupMessageSection<SegmentGroup1> Group1;
		public readonly SegmentMessageSection<GISSegment> GIS;
		public readonly SegmentMessageSection<TDTSegmentWithDatetime> TDT;
		public readonly SegmentMessageSection<LOCSegmentWithLotsOfLocations> LOC;
		public readonly SegmentMessageSection<CNTSegment> CNT;
		public readonly SegmentMessageSection<UNSSegment> UNS1;
		public readonly SegmentMessageSection<UNSSegment> UNS2;
		public readonly SegmentGroupMessageSection<CUSDECSegmentGroup6> Group6;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, Group1, LOC, TDT, GIS, Group6, UNS1, UNS2, CNT, UNT };
		}
	}
}
