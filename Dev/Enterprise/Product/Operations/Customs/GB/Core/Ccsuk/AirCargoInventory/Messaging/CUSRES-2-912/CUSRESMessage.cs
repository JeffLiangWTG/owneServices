using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSRES_2_912
{
	public partial class CUSRESMessage : SegmentGroup
	{
		public CUSRESMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegmentWithReferenceAndDates>(1);
			NAD = new SegmentMessageSection<NADSegment>(1);
			LOC = new SegmentMessageSection<LOCSegmentWithLotsOfLocations>(3);
			Group2 = new SegmentGroupMessageSection<CUSRESSegmentGroup2>(1);
			Group4 = new SegmentGroupMessageSection<CUSRESSegmentGroup4>(3);
			GIS = new SegmentMessageSection<GISSegment>(1);
			FTX = new SegmentMessageSection<FTXSegment>(2);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<BGMSegmentWithReferenceAndDates> BGM;
		public readonly SegmentMessageSection<NADSegment> NAD;
		public readonly SegmentMessageSection<LOCSegmentWithLotsOfLocations> LOC;
		public readonly SegmentGroupMessageSection<CUSRESSegmentGroup2> Group2;
		public readonly SegmentGroupMessageSection<CUSRESSegmentGroup4> Group4;
		public readonly SegmentMessageSection<GISSegment> GIS;
		public readonly SegmentMessageSection<FTXSegment> FTX;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, NAD, LOC, Group2, Group4, GIS, FTX, UNT };
		}
	}
}
