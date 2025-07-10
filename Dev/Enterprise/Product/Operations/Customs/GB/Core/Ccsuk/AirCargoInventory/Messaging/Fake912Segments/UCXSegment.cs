using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class UCXSegment : Segment
	{
		public UCXSegment()
		{
			MaximumNumberOfValues = 3;
		}

		public string ActionCode;
		public string ErrorCode;

		protected override void GetValues()
		{
			ValueItems = new object[]
			{
				ActionCode,
				ErrorCode
			};
		}

		protected override void SetValues()
		{
			ActionCode = Values(1);
			ErrorCode = Values(2);
		}

		protected override string SegmentNameOverride
		{
			get { return "UCX"; }
		}
	}
}
