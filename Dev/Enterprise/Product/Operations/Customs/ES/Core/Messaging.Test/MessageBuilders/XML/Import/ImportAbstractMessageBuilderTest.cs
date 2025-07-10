using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class ImportAbstractMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
		where TProvider : class, IImportCommonDataProvider
		where TMessageBuilder : ImportCommonMessageBuilder<TProvider, T>
	{
		#region CommonTests

		public abstract void TestPopulateSegmentosDeServicioIsTestFalse();

		#endregion

		protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
		protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
		protected abstract ZString GetTestFile();

		#region Structures Common SetUp

		protected IImportCommonC44CertificateDocument SetUpImportC44CertificateDocument(ZString certType, ZString certRef, ZString certQtyUnit, ZDecimal certQty, ZDateTime certDate)
		{
			var mockCertificate = new Mock<IImportCommonC44CertificateDocument>();
			mockCertificate.Setup(m => m.Name).Returns(certType);
			mockCertificate.Setup(m => m.Number).Returns(certRef);
			mockCertificate.Setup(m => m.CertQuantityUnit).Returns(certQtyUnit);
			mockCertificate.Setup(m => m.CertQuantityAmount).Returns(certQty);
			mockCertificate.Setup(m => m.CertDate).Returns(certDate);
			return mockCertificate.Object;
		}

		#endregion
	}
}
