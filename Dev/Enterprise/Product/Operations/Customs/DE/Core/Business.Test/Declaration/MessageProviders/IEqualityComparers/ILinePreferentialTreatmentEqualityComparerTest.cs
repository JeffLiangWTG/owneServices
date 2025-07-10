using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ILinePreferentialTreatmentEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, preferentialTreatmentComparer.GetHashCode(originalPreferentialTreatment));
		}

		public void TestEquals()
		{
			AssertEquals(true, preferentialTreatmentComparer.Equals(originalPreferentialTreatment, PreferentialTreatmentMock.Object));
		}

		public void TestEquals_RequestedPreferentialTreatment()
		{
			var modifiedPreferentialTreatmentMock = PreferentialTreatmentMock;
			modifiedPreferentialTreatmentMock.Setup(x => x.RequestedPreferentialTreatment).Returns("Y");
			AssertEquals(false, preferentialTreatmentComparer.Equals(originalPreferentialTreatment, modifiedPreferentialTreatmentMock.Object));
		}

		public void TestEquals_ContingentNumber()
		{
			var modifiedPreferentialTreatmentMock = PreferentialTreatmentMock;
			modifiedPreferentialTreatmentMock.Setup(x => x.ContingentNumber).Returns(new[] { "2" });
			AssertEquals(false, preferentialTreatmentComparer.Equals(originalPreferentialTreatment, modifiedPreferentialTreatmentMock.Object));
		}

		public void TestEquals_Quantity()
		{
			var quantityMock = IAmountEqualityComparerTest.AmountMock;
			quantityMock.Setup(x => x.Quantity).Returns(9.987m);
			var preferentialTreatmentMock = PreferentialTreatmentMock;
			preferentialTreatmentMock.Setup(x => x.Quantity).Returns(quantityMock.Object);
			AssertEquals(false, preferentialTreatmentComparer.Equals(originalPreferentialTreatment, preferentialTreatmentMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalPreferentialTreatment = PreferentialTreatmentMock.Object;
			preferentialTreatmentComparer = new ILinePreferentialTreatmentEqualityComparer();
		}
		ILinePreferentialTreatment originalPreferentialTreatment;
		ILinePreferentialTreatmentEqualityComparer preferentialTreatmentComparer;

		internal static Mock<ILinePreferentialTreatment> PreferentialTreatmentMock
		{
			get
			{
				var preferentialTreatmentMock = new Mock<ILinePreferentialTreatment>();
				preferentialTreatmentMock.Setup(x => x.RequestedPreferentialTreatment).Returns("X");
				preferentialTreatmentMock.Setup(x => x.ContingentNumber).Returns(new[] { "1" });
				preferentialTreatmentMock.Setup(x => x.Quantity).Returns(IAmountEqualityComparerTest.AmountMock.Object);
				return preferentialTreatmentMock;
			}
		}
	}
}
