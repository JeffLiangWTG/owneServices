using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D04A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL
{
	public class GenralSegmentGroup1 : SegmentGroup
	{
		public GenralSegmentGroup1()
		{
			MSG = new SegmentMessageSection<MSGSegment>(1);
			FTX = new SegmentMessageSection<FTXSegment>(4);
		}

		public readonly SegmentMessageSection<MSGSegment> MSG;
		public readonly SegmentMessageSection<FTXSegment> FTX;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { MSG, FTX };
		}
	}
}
