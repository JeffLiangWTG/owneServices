using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments; // CCSUK use 912:2, bot D:04:A or anything like that. So I have to guess at thge 

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public partial class CUSCARMessage : SegmentGroup
	{
		public CUSCARMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegmentWithReferenceAndDates>(1);
			Group1 = new SegmentGroupMessageSection<CUSCARSegmentGroup1>(1);
			GIS = new SegmentMessageSection<GISSegment>(9);
			Group2 = new SegmentGroupMessageSection<CUSCARSegmentGroup2>(1);
			Group3 = new SegmentGroupMessageSection<CUSCARSegmentGroup3>(4);
			Group5 = new SegmentGroupMessageSection<CUSCARSegmentGroup5>(99);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<BGMSegmentWithReferenceAndDates> BGM;
		public readonly SegmentGroupMessageSection<CUSCARSegmentGroup1> Group1;
		public readonly SegmentMessageSection<GISSegment> GIS;
		public readonly SegmentGroupMessageSection<CUSCARSegmentGroup2> Group2;
		public readonly SegmentGroupMessageSection<CUSCARSegmentGroup3> Group3;
		public readonly SegmentGroupMessageSection<CUSCARSegmentGroup5> Group5;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, Group1, GIS, Group2, Group3, Group5, UNT };
		}
	}
}
