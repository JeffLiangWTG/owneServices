using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D04A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CUKG2GSegmentGroup3 : SegmentGroup
	{
		public CUKG2GSegmentGroup3()
		{
			RFF1 = new SegmentMessageSection<RFFSegment>(1);
			LOC1 = new SegmentMessageSection<LOCSegment>(1);
			RFF2 = new SegmentMessageSection<RFFSegment>(1);
			DTM = new SegmentMessageSection<DTMSegment>(1);
			GEI = new SegmentMessageSection<GEISegment>(1);
			LOC2 = new SegmentMessageSection<LOCSegment>(1);
			RFF3 = new SegmentMessageSection<RFFSegment>(1);
		}

		public readonly SegmentMessageSection<RFFSegment> RFF1;
		public readonly SegmentMessageSection<LOCSegment> LOC1;
		public readonly SegmentMessageSection<RFFSegment> RFF2;
		public readonly SegmentMessageSection<DTMSegment> DTM;
		public readonly SegmentMessageSection<GEISegment> GEI;
		public readonly SegmentMessageSection<LOCSegment> LOC2;
		public readonly SegmentMessageSection<RFFSegment> RFF3;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { RFF1, LOC1, RFF2, DTM, GEI, LOC2, RFF3 };
		}
	}
}
