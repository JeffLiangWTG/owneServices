using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class T2LExpeditionCommonMessageBuilderTest<TMessageBuilder, TProvider, T, THeaderProvider, TLineProvider> : T2LCommonMessageBuilderTest<TMessageBuilder, TProvider, T, THeaderProvider, TLineProvider>
		where TProvider : class, IESEDIMessageCollectionProvider
		where TMessageBuilder : T2LCommonMessageBuilder<TProvider, T>
		where THeaderProvider : IExpeditionHeader
		where TLineProvider : IExpeditionLine
	{
		#region CommonTests
		public abstract void TestPopulateDeclarante();

		public abstract void TestPopulateExpedidor();

		public abstract void TestPopulateDestinatario();

		public abstract void TestPopulateCommunications();
		#endregion

		#region Structures Common SetUp
		protected Mock<IExpeditionDocumentSubmitted> SetUpDocument()
		{
			var mockDocument = new Mock<IExpeditionDocumentSubmitted>();
			mockDocument.Setup(m => m.Number).Returns("N380");
			mockDocument.Setup(m => m.Code).Returns("Documento 1");
			mockDocument.Setup(m => m.Date).Returns(ZDateTime.BrettsBirthday);
			return mockDocument;
		}
		#endregion
	}
}
