using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public abstract class PNTSMessagePrettierTest<T, TMessageDataObject> : TestCaseWithFactory
		where T : class
		where TMessageDataObject : MessageDataObject<T>
	{
		public void TestMessageInterpretation()
		{
			PrepareTestData();
			var message = Factory.New<PNTSEDIMessage>();
			message.EM_MessageText = GetMessageText();
			var messageObject = Activator.CreateInstance(typeof(TMessageDataObject), message) as TMessageDataObject;
			AssertEquals("Message should be human readable.", GetExpectedMessageInterpretation(), messageObject.Prettier.GetMessageInterpretation());
		}

		protected abstract ZString GetMessageText();

		protected abstract ZString GetExpectedMessageInterpretation();

		protected readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected virtual void PrepareTestData()
		{
			PNTSMessageTestHelper.SetUpStatusAndDescriptionMaps(Factory);
		}
	}
}
