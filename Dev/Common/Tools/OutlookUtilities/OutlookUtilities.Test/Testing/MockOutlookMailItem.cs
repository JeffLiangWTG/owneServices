namespace Enterprise.Interop.OutlookIntegration.Testing
{
	public class MockOutlookMailItem : OutlookMailItem
	{
		public MockOutlookMailItem(object customData) : base(new MockNativeMailItem(), customData)
		{
		}

		public void SimulateMailItemSendEvent()
		{
			Inner.SimulateMailSend();
		}

		internal new MockNativeMailItem Inner
		{
			get { return (MockNativeMailItem)base.Inner; }
		}
	}
}
