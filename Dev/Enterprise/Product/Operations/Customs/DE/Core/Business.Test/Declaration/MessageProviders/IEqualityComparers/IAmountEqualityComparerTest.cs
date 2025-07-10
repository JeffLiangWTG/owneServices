using CargoWise.Customs.DE.MessageContracts;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class IAmountEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, amountEqualityComparer.GetHashCode(originalAmount));
		}

		public void TestEquals()
		{
			AssertEquals(true, amountEqualityComparer.Equals(originalAmount, AmountMock.Object));
		}

		public void TestEquals_NullString()
		{
			var modifiedAmountMock = AmountMock;
			modifiedAmountMock.Setup(x => x.Qualifier).Returns((string)null);
			modifiedAmountMock.Setup(x => x.MeasurementUnit).Returns((string)null);
			AssertNoExceptionThrown(() => amountEqualityComparer.Equals(modifiedAmountMock.Object, originalAmount));
		}

		public void TestEquals_Qualifier()
		{
			var modifiedAmountMock = AmountMock;
			modifiedAmountMock.Setup(x => x.Qualifier).Returns("C");
			AssertEquals(false, amountEqualityComparer.Equals(originalAmount, modifiedAmountMock.Object));
		}

		public void TestEquals_MeasurementUnit()
		{
			var modifiedAmountMock = AmountMock;
			modifiedAmountMock.Setup(x => x.MeasurementUnit).Returns("KGM");
			AssertEquals(false, amountEqualityComparer.Equals(originalAmount, modifiedAmountMock.Object));
		}

		public void TestEquals_Quantity()
		{
			var modifiedAmountMock = AmountMock;
			modifiedAmountMock.Setup(x => x.Quantity).Returns(9.997m);
			AssertEquals(false, amountEqualityComparer.Equals(originalAmount, modifiedAmountMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalAmount = AmountMock.Object;
			amountEqualityComparer = new IAmountEqualityComparer();
		}
		IAmount originalAmount;
		IAmountEqualityComparer amountEqualityComparer;

		internal static Mock<IAmount> AmountMock
		{
			get
			{
				var amountMock = new Mock<IAmount>();
				amountMock.Setup(x => x.Qualifier).Returns("B");
				amountMock.Setup(x => x.Quantity).Returns(9.999m);
				amountMock.Setup(x => x.MeasurementUnit).Returns("TNE");
				return amountMock;
			}
		}
	}
}
