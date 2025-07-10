using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CUSCARSegmentGroup1 : SegmentGroup
	{
		public CUSCARSegmentGroup1()
		{
			RFF = new SegmentMessageSection<RFFSegment>(1);
		}

		public readonly SegmentMessageSection<RFFSegment> RFF;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { RFF };
		}
	}
}
