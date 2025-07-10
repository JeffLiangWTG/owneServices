using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class CUKFSASegmentGroup3 : SegmentGroup
	{
		public CUKFSASegmentGroup3()
		{
			TDT = new SegmentMessageSection<TDTSegmentWithDatetime>(1);
			LOC = new SegmentMessageSection<LOCSegmentWithLotsOfLocations>(9);
			RFF = new SegmentMessageSection<RFFSegmentWithDatetime>(1);
		}

		public readonly SegmentMessageSection<TDTSegmentWithDatetime> TDT;
		public readonly SegmentMessageSection<LOCSegmentWithLotsOfLocations> LOC;
		public readonly SegmentMessageSection<RFFSegmentWithDatetime> RFF;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { TDT, LOC, RFF };
		}
	}
}
