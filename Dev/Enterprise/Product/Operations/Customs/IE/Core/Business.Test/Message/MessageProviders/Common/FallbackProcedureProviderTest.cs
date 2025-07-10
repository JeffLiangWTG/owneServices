using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Moq;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class FallbackProcedureProviderTest : DataProviderTestCase<FallbackProcedureProvider>
	{
		protected override FallbackProcedureProvider GetProvider()
		{
			var sendingActionMock = new Mock<IFallbackProcedureSendingObject>();
			sendingActionMock.Setup(a => a.AlternativeDateOfAcceptance).Returns(new ZDate(2023, 11, 23));
			sendingActionMock.Setup(a => a.CustomsJustification).Returns("CustomsJustification");
			sendingActionMock.Setup(a => a.CustomsReferenceNumber).Returns("CustomsReferenceNumber");

			return FallbackProcedureProvider.New(sendingActionMock.Object);
		}

		public void TestAlternativeDateOfAcceptance()
		{
			AssertEquals("AlternativeDateOfAcceptance", new ZDate(2023, 11, 23), Provider.AlternativeDateOfAcceptance);
		}

		public void TestCustomsJustification()
		{
			AssertEquals("CustomsJustification", "CustomsJustification", Provider.CustomsJustification);
		}

		public void TestCustomsReferenceNumber()
		{
			AssertEquals("CustomsReferenceNumber", "CustomsReferenceNumber", Provider.CustomsReferenceNumber);
		}
	}
}
