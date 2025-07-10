using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CUSCARSegmentGroup6 : SegmentGroup
	{
		public CUSCARSegmentGroup6()
		{
			QTY = new SegmentMessageSection<QTYSegment>(1);
		}

		public readonly SegmentMessageSection<QTYSegment> QTY;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { QTY };
		}
	}
}
