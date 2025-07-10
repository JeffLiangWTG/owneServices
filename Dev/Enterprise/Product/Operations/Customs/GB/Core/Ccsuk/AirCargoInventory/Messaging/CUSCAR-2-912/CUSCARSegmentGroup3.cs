using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CUSCARSegmentGroup3 : SegmentGroup
	{
		public CUSCARSegmentGroup3()
		{
			NAD = new SegmentMessageSection<NADSegment>(1);
		}

		public readonly SegmentMessageSection<NADSegment> NAD;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { NAD };
		}
	}
}
