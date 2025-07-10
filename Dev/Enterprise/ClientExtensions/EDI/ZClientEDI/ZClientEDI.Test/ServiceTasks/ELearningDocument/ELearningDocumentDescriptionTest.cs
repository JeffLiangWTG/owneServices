using System;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.Test
{
	public class ELearningDocumentDescriptionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionIdGetSetEmpty()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Id).Returns(string.Empty);
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var id = iIeLearningDocumentFileDescription.Id;
			// Assert
			AssertNullOrEmpty("IELearningDocumentFileDescription Id property should be empty", id);
		}

		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionIdGetSetNull()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Id).Returns((string)(null));
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var id = iIeLearningDocumentFileDescription.Id;
			// Assert
			Assert("IELearningDocumentFileDescription Id property should be null", id == null);
		}

		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionIdGetSetValue()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Id).Returns("value");
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var id = iIeLearningDocumentFileDescription.Id;
			// Assert
			Assert("IELearningDocumentFileDescription Id property should have a value", id == "value");
		}

		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionTitleGetSetEmpty()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Title).Returns(string.Empty);
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var title = iIeLearningDocumentFileDescription.Title;
			// Assert
			AssertNullOrEmpty("IELearningDocumentFileDescription Title property should be empty", title);
		}

		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionTitleGetSetNull()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Title).Returns((string)(null));
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var title = iIeLearningDocumentFileDescription.Title;
			// Assert
			Assert("IELearningDocumentFileDescription Title property should be null", title == null);
		}

		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionTitleGetSetValue()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Title).Returns("value");
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var title = iIeLearningDocumentFileDescription.Title;
			// Assert
			Assert("IELearningDocumentFileDescription Title property should have a value", title == "value");
		}

		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionUrlGetSetEmpty()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Url).Returns(string.Empty);
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var url = iIeLearningDocumentFileDescription.Url;
			// Assert
			AssertNullOrEmpty("IELearningDocumentFileDescription Url property should be empty", url);
		}

		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionUrlGetSetNull()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Url).Returns((string)(null));
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var url = iIeLearningDocumentFileDescription.Url;
			// Assert
			Assert("IELearningDocumentFileDescription Url property should be null", url == null);
		}

		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionUrlGetSetValue()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Url).Returns("value");
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var url = iIeLearningDocumentFileDescription.Url;
			// Assert
			Assert("IELearningDocumentFileDescription url property should have a value", url == "value");
		}
		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionTypeGetSetEmpty()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Type).Returns(string.Empty);
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var typeVal = iIeLearningDocumentFileDescription.Type;
			// Assert
			AssertNullOrEmpty("IELearningDocumentFileDescription Type property should be empty", typeVal);
		}

		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionTypeGetSetNull()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Type).Returns((string)(null));
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var typeVal = iIeLearningDocumentFileDescription.Type;
			// Assert
			Assert("IELearningDocumentFileDescription Type property should be null", typeVal == null);
		}

		[ExpectNoExceptions]
		public void TestIeLearningDocumentDescriptionTypeGetSetValue()
		{
			// Arrange
			var mock = new Mock<IELearningDocumentFileDescription>();
			mock.Setup(foo => foo.Type).Returns("value");
			IELearningDocumentFileDescription iIeLearningDocumentFileDescription = mock.Object;
			// Act
			var typeVal = iIeLearningDocumentFileDescription.Type;
			// Assert
			Assert("IELearningDocumentFileDescription Type property should have a value", typeVal == "value");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestELearningDocumentDescriptionIdGetSetEmpty()
		{
			var obj = new ELearningDocumentFileDescription { Id = "" };
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestELearningDocumentDescriptionIdGetSetNull()
		{
			var obj = new ELearningDocumentFileDescription { Id = null };
		}

		[ExpectNoExceptions]
		public void TestELearningDocumentDescriptionIdGetSetCorrectValue()
		{
			// Arrange
			var obj = new ELearningDocumentFileDescription { Id = "value" };

			// Act
			var actual = obj.Id;
			// Assert
			Assert("ELearningDocumentFileDescription Id property should have a value", actual == "value");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestELearningDocumentDescriptionTitleGetSetEmpty()
		{
			var obj = new ELearningDocumentFileDescription { Title = "" };
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestELearningDocumentDescriptionTitleGetSetNull()
		{
			var obj = new ELearningDocumentFileDescription { Title = null };
		}

		[ExpectNoExceptions]
		public void TestELearningDocumentDescriptionTitleGetSetCorrectValue()
		{
			// Arrange
			var obj = new ELearningDocumentFileDescription { Title = "value" };

			// Act
			var actual = obj.Title;
			// Assert
			Assert("ELearningDocumentFileDescription Title property should have a value", actual == "value");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestELearningDocumentDescriptionUrlGetSetEmpty()
		{
			var obj = new ELearningDocumentFileDescription { Url = "" };
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestELearningDocumentDescriptionUrlGetSetNull()
		{
			var obj = new ELearningDocumentFileDescription { Url = null };
		}

		[ExpectNoExceptions]
		public void TestELearningDocumentDescriptionUrlGetSetCorrectValue()
		{
			// Arrange
			var obj = new ELearningDocumentFileDescription { Url = "value" };

			// Act
			var actual = obj.Url;
			// Assert
			Assert("ELearningDocumentFileDescription Url property should have a value", actual == "value");
		}
		[ExpectException(typeof(ArgumentException))]
		public void TestELearningDocumentDescriptionTypeGetSetEmpty()
		{
			var obj = new ELearningDocumentFileDescription { Type = "" };
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestELearningDocumentDescriptionTypeGetSetNull()
		{
			var obj = new ELearningDocumentFileDescription { Type = null };
		}

		[ExpectNoExceptions]
		public void TestELearningDocumentDescriptionTypeGetSetCorrectValue()
		{
			// Arrange
			var obj = new ELearningDocumentFileDescription { Type = "value" };

			// Act
			var actual = obj.Type;
			// Assert
			Assert("ELearningDocumentFileDescription Type property should have a value", actual == "value");
		}

		[ExpectNoExceptions]
		public void TestELearningDocumentDescriptionToMetaDocumentCorrectValue()
		{
			// Arrange
			string id = Guid.NewGuid().ToString();
			string documentType = Guid.NewGuid().ToString();
			string title = Guid.NewGuid().ToString();
			string url = Guid.NewGuid().ToString();
			var obj = new ELearningDocumentFileDescription
			{
				Url = url,
				Title = title,
				Type = documentType,
				Id = id
			};

			// Act
			var actual = obj;
			// Assert
			Assert("ELearningDocumentFileDescription DocumentType property should have been converted correctly", actual.Type == documentType);
			Assert("ELearningDocumentFileDescription MyAccountDocumentId property should have been converted correctly", new Guid(actual.Id) == new Guid(id));
			Assert("ELearningDocumentFileDescription Title property should have been converted correctly", actual.Title == title);
			Assert("ELearningDocumentFileDescription Url property should have been converted correctly", actual.Url == url);
		}
	}
}
