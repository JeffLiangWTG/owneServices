using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D04A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL
{
	public class GenralMessage : SegmentGroup
	{
		public GenralMessage()
		{
			UNH = new SegmentMessageSection<UNHSegment>(1);
			BGM = new SegmentMessageSection<BGMSegment>(1);
			Group1 = new SegmentGroupMessageSection<GenralSegmentGroup1>(100);
			UNT = new SegmentMessageSection<UNTSegment>(1);
		}

		public readonly SegmentMessageSection<UNHSegment> UNH;
		public readonly SegmentMessageSection<BGMSegment> BGM;
		public readonly SegmentGroupMessageSection<GenralSegmentGroup1> Group1;
		public readonly SegmentMessageSection<UNTSegment> UNT;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UNH, BGM, Group1, UNT };
		}
	}
}

