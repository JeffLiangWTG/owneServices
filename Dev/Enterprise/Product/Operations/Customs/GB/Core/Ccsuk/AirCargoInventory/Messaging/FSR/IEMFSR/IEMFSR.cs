using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public partial class IEMFSR : SegmentGroup
	{
		public IEMFSR()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegment>(1);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<BGMSegment> BGM;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, UNT };
		}
	}
}
