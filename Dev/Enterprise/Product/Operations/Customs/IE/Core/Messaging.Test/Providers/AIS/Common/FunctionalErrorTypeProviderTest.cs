using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(FunctionalErrorTypeProvider))]
	sealed class FunctionalErrorTypeProviderTest : TestCaseWithFactory
	{
		public void TestErrorReason()
		{
			AssertEquals("Some reason", provider.ErrorReason);
		}

		public void TestErrorType()
		{
			AssertEquals("Some type", provider.ErrorType);
		}

		public void TestErrorMessage()
		{
			AssertEquals("Some message", provider.ErrorMessage);
		}

		public void TestOriginalAttributeValue()
		{
			AssertEquals("Some original value", provider.OriginalAttributeValue);
		}

		public void TestErrorPointer()
		{
			AssertEquals("Some pointer", provider.ErrorPointer);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var mock = new Mock<IFunctionalErrorType>();
			mock.Setup(m => m.ErrorReason).Returns("Some reason");
			mock.Setup(m => m.ErrorType).Returns("Some type");
			mock.Setup(m => m.ErrorMessage).Returns("Some message");
			mock.Setup(m => m.OriginalAttributeValue).Returns("Some original value");
			mock.Setup(m => m.ErrorPointer).Returns("Some pointer");

			provider = new FunctionalErrorTypeProvider(mock.Object);
		}

		FunctionalErrorTypeProvider provider;
	}
}
