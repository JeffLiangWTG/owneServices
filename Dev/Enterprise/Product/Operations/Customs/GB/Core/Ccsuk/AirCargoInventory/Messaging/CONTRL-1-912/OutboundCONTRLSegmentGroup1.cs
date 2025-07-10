using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D04A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	/// <summary>
	/// NB. DJC 2011-06-30. We do not include Group2 (UCR & UCD) because we'll send only a free-text explanation.  No pointing to bad inbound data elements.
	/// </summary>
	public class OutboundCONTRLSegmentGroup1 : SegmentGroup
	{
		public OutboundCONTRLSegmentGroup1()
		{
			UCM = new SegmentMessageSection<UCMSegment>(1);
			UCX = new SegmentMessageSection<UCXSegment>(1);
		}

		public readonly SegmentMessageSection<UCMSegment> UCM;
		public readonly SegmentMessageSection<UCXSegment> UCX;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { UCM, UCX };
		}
	}
}
