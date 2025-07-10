using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CUSRESSegmentGroup2 : SegmentGroup
	{
		public CUSRESSegmentGroup2()
		{
			PAC = new SegmentMessageSection<PACSegment>(1);
		}

		public readonly SegmentMessageSection<PACSegment> PAC;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { PAC };
		}
	}
}
