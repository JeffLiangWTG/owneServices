using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CUSRESSegmentGroup4 : SegmentGroup
	{
		public CUSRESSegmentGroup4()
		{
			RFF = new SegmentMessageSection<RFFSegmentWithDatetime>(1);
		}

		public readonly SegmentMessageSection<RFFSegmentWithDatetime> RFF;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { RFF };
		}
	}
}
