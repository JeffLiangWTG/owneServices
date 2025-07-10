using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CUSCARSegmentGroup2 : SegmentGroup
	{
		public CUSCARSegmentGroup2()
		{
			TDT = new SegmentMessageSection<TDTSegmentWithDatetime>(1);
			LOC = new SegmentMessageSection<LOCSegmentWithLotsOfLocations>(2);
		}

		public readonly SegmentMessageSection<TDTSegmentWithDatetime> TDT;
		public readonly SegmentMessageSection<LOCSegmentWithLotsOfLocations> LOC;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { TDT, LOC };
		}
	}
}
