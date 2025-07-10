using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class T2LCommonMessageBuilderTest<TMessageBuilder, TProvider, T, THeaderProvider, TLineProvider> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
		where TProvider : class, IESEDIMessageCollectionProvider
		where TMessageBuilder : T2LCommonMessageBuilder<TProvider, T>
		where THeaderProvider : IT2LHeaderCommon
		where TLineProvider : IT2LLineCommon
	{
		#region Structures Common SetUp

		protected IT2LCommunicationsCommon SetupCommunications()
		{
			var mockCommunications = new Mock<IT2LCommunicationsCommon>();
			mockCommunications.Setup(m => m.DeclarationEmail).Returns("lamary@campe.fue");
			mockCommunications.Setup(m => m.OtherEmail).Returns("lalourdes@guindi.fue");
			mockCommunications.Setup(m => m.GreenCircuitIndicator).Returns(true);
			return mockCommunications.Object;
		}

		#endregion
	}
}
