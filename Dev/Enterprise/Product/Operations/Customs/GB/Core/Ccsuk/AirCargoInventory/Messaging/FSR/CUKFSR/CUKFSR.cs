using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public partial class CUKFSR : SegmentGroup
	{
		public CUKFSR()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegmentWithReferenceAndDatesAnd4343>(1);
			LOC = new SegmentMessageSection<LOCSegmentWithLotsOfLocations>(1);
			COM = new SegmentMessageSection<COMSegment>(1);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<BGMSegmentWithReferenceAndDatesAnd4343> BGM;
		public readonly SegmentMessageSection<LOCSegmentWithLotsOfLocations> LOC;
		public readonly SegmentMessageSection<COMSegment> COM;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, LOC, COM, UNT };
		}
	}
}
