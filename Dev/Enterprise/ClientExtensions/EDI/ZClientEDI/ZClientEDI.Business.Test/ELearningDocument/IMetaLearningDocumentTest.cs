using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Business.IncidentManager.ELearningDocument.Business.Test
{
	public class MetaLearningDocumentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIMetaLearningDocumentTitleGetSetEmpty()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentDescription>();
			mock.Setup(foo => foo.Title).Returns(string.Empty);
			IELearningDocumentDescription testInstance = mock.Object;
			// Act
			var title = testInstance.Title;
			// Assert
			AssertNullOrEmpty("IELearningDocumentDescription Title property should be empty", title);
		}

		[ExpectNoExceptions]
		public void TestIMetaLearningDocumentTitleGetSetValue()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentDescription>();
			mock.Setup(foo => foo.Title).Returns("value");
			IELearningDocumentDescription testInstance = mock.Object;
			// Act
			var title = testInstance.Title;
			// Assert
			Assert("IELearningDocumentDescription Title property should have a value", title == "value");
		}

		[ExpectNoExceptions]
		public void TestIMetaLearningDocumentTitleGetSetNull()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentDescription>();
			mock.Setup(foo => foo.Title).Returns((string)(null));
			IELearningDocumentDescription testInstance = mock.Object;
			// Act
			var title = testInstance.Title;
			// Assert
			Assert("IELearningDocumentDescription Title property should be null", title == null);
		}

		[ExpectNoExceptions]
		public void TestIMetaLearningDocumentUrlGetSetEmpty()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentDescription>();
			mock.Setup(foo => foo.Url).Returns(string.Empty);
			IELearningDocumentDescription testInstance = mock.Object;
			// Act
			var url = testInstance.Url;
			// Assert
			AssertNullOrEmpty("IELearningDocumentDescription Url property should be empty", url);
		}

		[ExpectNoExceptions]
		public void TestIMetaLearningDocumentUrlGetSetValue()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentDescription>();
			mock.Setup(foo => foo.Url).Returns("value");
			IELearningDocumentDescription testInstance = mock.Object;
			// Act
			var url = testInstance.Url;
			// Assert
			Assert("IELearningDocumentDescription Url property should have a value", url == "value");
		}

		[ExpectNoExceptions]
		public void TestIMetaLearningDocumentUrlGetSetNull()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentDescription>();
			mock.Setup(foo => foo.Url).Returns((string)(null));
			IELearningDocumentDescription testInstance = mock.Object;
			// Act
			var url = testInstance.Url;
			// Assert
			Assert("IELearningDocumentDescription Title property should be null", url == null);
		}
	}
}
