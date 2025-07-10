
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D04A.Segments;
namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public partial class CUKG2GMessage : SegmentGroup
	{
		public CUKG2GMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegment>(1);
			Group1 = new SegmentGroupMessageSection<CUKG2GSegmentGroup1>(1);
			Group2 = new SegmentGroupMessageSection<CUKG2GSegmentGroup2>(999);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<BGMSegment> BGM;
		public readonly SegmentGroupMessageSection<CUKG2GSegmentGroup1> Group1;
		public readonly SegmentGroupMessageSection<CUKG2GSegmentGroup2> Group2;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, Group1, Group2, UNT };
		}
	}
}
