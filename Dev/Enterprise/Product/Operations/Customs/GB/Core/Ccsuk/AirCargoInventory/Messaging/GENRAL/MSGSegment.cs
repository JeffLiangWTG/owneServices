namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL
{
	public class MSGSegment : Edifact.Auto.Segment
	{
		public MSGSegment()
		{
			MaximumNumberOfValues = 2;
		}

		public string OriginOfMessage;

		#region Implementation
		protected override void GetValues()
		{
			ValueItems = new object[]
			{
				OriginOfMessage
			};
		}

		protected override void SetValues()
		{
			OriginOfMessage = Values(1);
		}

		#endregion
	}
}
