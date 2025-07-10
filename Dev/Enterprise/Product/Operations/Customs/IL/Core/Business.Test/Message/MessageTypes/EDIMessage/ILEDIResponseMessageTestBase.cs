namespace Enterprise.Customs.IL.Business.Testing
{
	abstract class ILEDIResponseMessageTestBase<T> : ILEDIMessageTestBase<T> where T : ILEDIResponseMessage
	{
		protected override string GetMessageReceiveTransmit() => "RCV";
	}
}
