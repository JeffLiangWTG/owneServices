using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D04A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CUKG2GSegmentGroup2 : SegmentGroup
	{
		public CUKG2GSegmentGroup2()
		{
			GID = new SegmentMessageSection<Edifact.D00A.Segments.GIDSegment>(1);
			RFF = new SegmentMessageSection<RFFSegment>(2);
			Group3 = new SegmentGroupMessageSection<CUKG2GSegmentGroup3>(99);
		}

		public readonly SegmentMessageSection<RFFSegment> RFF;
		public readonly SegmentMessageSection<Edifact.D00A.Segments.GIDSegment> GID; // there is no D04A GID segment, we'll use the D00A instead. 
		public readonly SegmentGroupMessageSection<CUKG2GSegmentGroup3> Group3;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { GID, RFF, Group3 };
		}
	}
}
