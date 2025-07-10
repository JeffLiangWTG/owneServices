using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CUSCARSegmentGroup5 : SegmentGroup
	{
		public CUSCARSegmentGroup5()
		{
			GID = new SegmentMessageSection<GIDSegment>(1);
			FTX = new SegmentMessageSection<FTXSegment>(1);
			Group6 = new SegmentGroupMessageSection<CUSCARSegmentGroup6>(2);
			MEA = new SegmentMessageSection<MEASegment>(1);
			RFF = new SegmentMessageSection<RFFSegment>(2);
		}

		public readonly SegmentMessageSection<GIDSegment> GID;
		public readonly SegmentMessageSection<FTXSegment> FTX;
		public readonly SegmentGroupMessageSection<CUSCARSegmentGroup6> Group6;
		public readonly SegmentMessageSection<MEASegment> MEA;
		public readonly SegmentMessageSection<RFFSegment> RFF;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { GID, FTX, Group6, MEA, RFF };
		}
	}
}
