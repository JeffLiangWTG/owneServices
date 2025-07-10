using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class CUKFSASegmentGroup1 : SegmentGroup
	{
		public CUKFSASegmentGroup1()
		{
			DOC = new SegmentMessageSection<DOCSegmentWith8Elements>(1);
			Group2 = new SegmentGroupMessageSection<CUKFSASegmentGroup2>(99);
			Group3 = new SegmentGroupMessageSection<CUKFSASegmentGroup3>(2);
			Group4 = new SegmentGroupMessageSection<CUKFSASegmentGroup4>(2);
			Group5 = new SegmentGroupMessageSection<CUKFSASegmentGroup5>(1);
			Group6 = new SegmentGroupMessageSection<CUKFSASegmentGroup6>(1);
		}

		public readonly SegmentMessageSection<DOCSegmentWith8Elements> DOC;
		public readonly SegmentGroupMessageSection<CUKFSASegmentGroup2> Group2;
		public readonly SegmentGroupMessageSection<CUKFSASegmentGroup3> Group3;
		public readonly SegmentGroupMessageSection<CUKFSASegmentGroup4> Group4;
		public readonly SegmentGroupMessageSection<CUKFSASegmentGroup5> Group5;
		public readonly SegmentGroupMessageSection<CUKFSASegmentGroup6> Group6;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { DOC, Group2, Group3, Group4, Group5, Group6 };
		}
	}
}
