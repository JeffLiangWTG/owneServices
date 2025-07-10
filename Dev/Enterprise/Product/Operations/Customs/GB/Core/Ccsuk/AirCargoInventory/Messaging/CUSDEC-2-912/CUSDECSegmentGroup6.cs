using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSDEC_2_912
{
	public class CUSDECSegmentGroup6 : SegmentGroup
	{
		public CUSDECSegmentGroup6()
		{
			MOA = new SegmentMessageSection<MOASegmentWithFunction>(1);
		}

		public readonly SegmentMessageSection<MOASegmentWithFunction> MOA;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { MOA };
		}
	}
}
