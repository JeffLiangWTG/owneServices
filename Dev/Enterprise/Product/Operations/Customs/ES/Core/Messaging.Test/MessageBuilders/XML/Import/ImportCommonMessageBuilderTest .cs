using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class ImportCommonMessageBuilderTest<TMessageBuilder, TProvider, T, THeaderProvider, TLineProvider> : ImportAbstractMessageBuilderTest<TMessageBuilder, TProvider, T>
		where TProvider : class, IImportCommonDataProvider
		where TMessageBuilder : ImportCommonMessageBuilder<TProvider, T>
		where THeaderProvider : IImportCommonHeader
		where TLineProvider : IImportCommonLine
	{
		#region CommonTests

		public abstract void TestPopulateImportador();

		public abstract void TestPopulateDeclarante();

		#endregion

		#region Structures Common SetUp

		protected IImportImporterProvider SetUpImporter()
		{
			var mockImporter = new Mock<IImportImporterProvider>();
			mockImporter.Setup(m => m.Id).Returns("ESA78587268");
			mockImporter.Setup(m => m.IsIndividual).Returns(false);
			mockImporter.Setup(m => m.Name).Returns("importer name ªº aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbb");
			mockImporter.Setup(m => m.Address).Returns("importer address complete aaaaaaaaabbb");
			mockImporter.Setup(m => m.City).Returns("MADRID");
			mockImporter.Setup(m => m.PostCode).Returns("12345");
			mockImporter.Setup(m => m.Country).Returns("ES");
			return mockImporter.Object;
		}

		protected IImportDeclarantPartyIdProvider SetUpImportDeclarant()
		{
			var mockDeclarant = new Mock<IImportDeclarantPartyIdProvider>();
			mockDeclarant.Setup(m => m.Type).Returns("1");
			mockDeclarant.Setup(m => m.Id).Returns("ESA78587268");
			mockDeclarant.Setup(m => m.Name).Returns("Declarant name aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbb");
			mockDeclarant.Setup(m => m.IsAuthorized).Returns(true);
			return mockDeclarant.Object;
		}

		#endregion
	}
}
