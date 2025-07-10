using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class NegativeAcknowledgementErrorProviderTest : TestCaseWithFactory
	{
		public void TestLineNumber()
		{
			AssertEquals("1", provider.LineNumber);
		}

		public void TestReason()
		{
			AssertEquals("reason", provider.Reason);
		}

		public void TestColumnNumber()
		{
			AssertEquals("2", provider.ColumnNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var mock = new Mock<IXmlNegativeAcknowledgement>();
			mock.Setup(o => o.ErrorLineNumber).Returns("1");
			mock.Setup(o => o.ErrorReason).Returns("reason");
			mock.Setup(o => o.ErrorColumnNumber).Returns("2");
			provider = new NegativeAcknowledgementErrorProvider(mock.Object);
		}
		INegativeAcknowledgementError provider;
	}
}
