using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class IExciseDutyEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, exciseDutyEqualityComparer.GetHashCode(originalExciseDuty));
		}

		public void TestEquals()
		{
			AssertEquals(true, exciseDutyEqualityComparer.Equals(originalExciseDuty, ExciseDutyMock.Object));
		}

		public void TestEquals_Code()
		{
			var exciseDutyMock = ExciseDutyMock;
			exciseDutyMock.Setup(x => x.Code).Returns("B234");
			AssertEquals(false, exciseDutyEqualityComparer.Equals(originalExciseDuty, exciseDutyMock.Object));
		}

		public void TestEquals_DegreePercentage()
		{
			var exciseDutyMock = ExciseDutyMock;
			exciseDutyMock.Setup(x => x.DegreePercentage).Returns(0.03m);
			AssertEquals(false, exciseDutyEqualityComparer.Equals(originalExciseDuty, exciseDutyMock.Object));
		}

		public void TestEquals_Value()
		{
			var exciseDutyMock = ExciseDutyMock;
			exciseDutyMock.Setup(x => x.Value).Returns(234.03m);
			AssertEquals(false, exciseDutyEqualityComparer.Equals(originalExciseDuty, exciseDutyMock.Object));
		}

		public void TestEquals_Amount()
		{
			var amountMock = IAmountEqualityComparerTest.AmountMock;
			amountMock.Setup(x => x.Quantity).Returns(9.987m);
			var exciseDutyMock = ExciseDutyMock;
			exciseDutyMock.Setup(x => x.Amount).Returns(amountMock.Object);
			AssertEquals(false, exciseDutyEqualityComparer.Equals(originalExciseDuty, exciseDutyMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();

			originalExciseDuty = ExciseDutyMock.Object;
			exciseDutyEqualityComparer = new IExciseDutyEqualityComparer();
		}
		IExciseDuty originalExciseDuty;
		IExciseDutyEqualityComparer exciseDutyEqualityComparer;

		Mock<IExciseDuty> ExciseDutyMock
		{
			get
			{
				var exciseDutyMock = new Mock<IExciseDuty>();
				exciseDutyMock.Setup(x => x.Code).Returns("A123");
				exciseDutyMock.Setup(x => x.DegreePercentage).Returns(0.10m);
				exciseDutyMock.Setup(x => x.Value).Returns(1634.28m);
				exciseDutyMock.Setup(x => x.Amount).Returns(IAmountEqualityComparerTest.AmountMock.Object);
				return exciseDutyMock;
			}
		}
	}
}
