using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D04A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CUKG2GSegmentGroup1 : SegmentGroup
	{
		public CUKG2GSegmentGroup1()
		{
			RFF = new SegmentMessageSection<RFFSegment>(3);
			GEI1 = new SegmentMessageSection<GEISegment>(1);
			LOC = new SegmentMessageSection<LOCSegment>(1);
			NAD = new SegmentMessageSection<NADSegment>(1);
			GEI2 = new SegmentMessageSection<GEISegment>(1);
		}

		public readonly SegmentMessageSection<RFFSegment> RFF;
		public readonly SegmentMessageSection<GEISegment> GEI1;
		public readonly SegmentMessageSection<LOCSegment> LOC;
		public readonly SegmentMessageSection<NADSegment> NAD;
		public readonly SegmentMessageSection<GEISegment> GEI2;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { RFF, GEI1, LOC, NAD, GEI2 };
		}
	}
}
