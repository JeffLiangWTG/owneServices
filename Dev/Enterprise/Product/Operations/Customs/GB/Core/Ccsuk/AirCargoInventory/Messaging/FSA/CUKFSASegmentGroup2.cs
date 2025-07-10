using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class CUKFSASegmentGroup2 : SegmentGroup
	{
		public CUKFSASegmentGroup2()
		{
			GIS = new SegmentMessageSection<GISSegment>(1);
			FTX = new SegmentMessageSection<FTXSegment>(1);
		}

		public readonly SegmentMessageSection<GISSegment> GIS;
		public readonly SegmentMessageSection<FTXSegment> FTX;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { GIS, FTX };
		}
	}
}
