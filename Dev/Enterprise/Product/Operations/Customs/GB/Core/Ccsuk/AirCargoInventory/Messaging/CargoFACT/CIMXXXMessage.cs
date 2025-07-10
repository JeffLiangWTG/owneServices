using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public partial class CIMXXXMessage : SegmentGroup
	{
		public CIMXXXMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			FTX = new SegmentMessageSection<FTXSegment>(999);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<FTXSegment> FTX;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, FTX, UNT };
		}
	}
}
