namespace Enterprise.Customs.IL.Business.Testing
{
	abstract class ILEDIRequestMessageTestBase<T> : ILEDIMessageTestBase<T> where T : ILEDIRequestMessage
	{
		public void TestEnsureHaveCustomsServiceName()
		{
			var message = (T)GetNewMessage();
			var expectedServiceName = MessageServiceNameProvider.GetServiceName(message.EM_MessageSubType);
			if (message.EM_MessageSubType.IsEmpty)
			{
				AssertNullOrEmpty("When EM_MessageSubType.IsEmpty Service Name must also be empty", expectedServiceName);
				return;
			}
			AssertNotNullOrEmpty($"Please provide Customs Service Name for {message.EM_MessageSubType}", expectedServiceName);
		}

		protected override string GetMessageReceiveTransmit() => "TRX";
	}
}
