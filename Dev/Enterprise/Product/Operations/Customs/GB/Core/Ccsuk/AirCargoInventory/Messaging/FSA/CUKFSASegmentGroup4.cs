using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class CUKFSASegmentGroup4 : SegmentGroup
	{
		public CUKFSASegmentGroup4()
		{
			NAD = new SegmentMessageSection<NADSegment>(1);
			COM = new SegmentMessageSection<COMSegment>(9);
		}

		public readonly SegmentMessageSection<NADSegment> NAD;
		public readonly SegmentMessageSection<COMSegment> COM;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { NAD, COM };
		}
	}
}
