using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class MonthlyClosingSnapshotLinePreferentialTreatmentProviderTest : TestCaseWithFactory
	{
		public void TestRequestedPreferentialTreatment()
		{
			var line = new Mock<ISCIRECLine>();
			line.Setup(l => l.RequestedPreferentialTreatment).Returns("testRequestedPreferentialTreatment");
			var provider = new MonthlyClosingSnapshotLinePreferentialTreatmentProvider(line.Object.RequestedPreferentialTreatment);
			AssertEquals("testRequestedPreferentialTreatment", provider.RequestedPreferentialTreatment);
		}

		public void TestContingentNumber()
		{
			var line = new Mock<ISCIRECLine>();
			line.Setup(l => l.RequestedPreferentialTreatment).Returns("testRequestedPreferentialTreatment");
			var provider = new MonthlyClosingSnapshotLinePreferentialTreatmentProvider(line.Object.RequestedPreferentialTreatment);
			AssertEquals(false, provider.ContingentNumber.Any());
		}

		public void TestQuantity()
		{
			var line = new Mock<ISCIRECLine>();
			line.Setup(l => l.RequestedPreferentialTreatment).Returns("testRequestedPreferentialTreatment");
			var provider = new MonthlyClosingSnapshotLinePreferentialTreatmentProvider(line.Object.RequestedPreferentialTreatment);
			AssertNull(provider.Quantity);
		}
	}
}

