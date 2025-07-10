using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class DeltaIEMessageDataObjectTest : TestCaseWithFactory
	{
		public void TestPrettierType()
		{
			var message = Factory.New<DeltaIEFREDIMessage>();
			var messageObject = new DeltaIEMessageDataObject(message);
			AssertType<DeltaIEMessagePrettier>(messageObject.Prettier);
		}
	}

	public abstract class DeltaIEMessageDataObjectTest<T, TMessage> : FREDIMessageDataObjectTest<T, TMessage, DeltaIEFREDIMessage>
		where T : DeltaIEMessageDataObject<TMessage>
		where TMessage : class
	{
	}
}
