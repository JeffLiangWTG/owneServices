using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public abstract class PNTSMessageDataObjectTest<T, TMessage> : FREDIMessageDataObjectTest<T, TMessage, PNTSEDIMessage>
		where T : PNTSMessageDataObject<TMessage>
		where TMessage : class
	{
		public void TestCustomsStatus()
		{
			AssertEquals($"New Customs Status of {nameof(T)}", ExpectedNewCustomsStatus, messageDataObject.CustomsStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();
			PNTSMessageTestHelper.SetUpStatusAndDescriptionMaps(Factory);
		}

		protected abstract ZString ExpectedNewCustomsStatus { get; }
	}
}
