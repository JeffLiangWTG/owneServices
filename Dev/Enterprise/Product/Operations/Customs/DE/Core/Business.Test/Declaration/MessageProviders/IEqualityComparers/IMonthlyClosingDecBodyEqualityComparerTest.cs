using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IMonthlyClosingDecBodyEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, bodyEqualityComparer.GetHashCode(originalBody));
		}

		public void TestEquals()
		{
			AssertEquals(true, bodyEqualityComparer.Equals(originalBody, BodyMock.Object));
		}

		public void TestEquals_ConsigneePK()
		{
			var modifiedBodyMock = BodyMock;
			modifiedBodyMock.Setup(x => x.ConsigneePK).Returns(new Guid("44444444-0000-0000-0000-000000000000"));
			AssertEquals(false, bodyEqualityComparer.Equals(originalBody, modifiedBodyMock.Object));
		}

		public void TestEquals_DeliveryTermsCode()
		{
			var modifiedBodyMock = BodyMock;
			modifiedBodyMock.Setup(x => x.DeliveryTermsCode).Returns("BBB");
			AssertEquals(false, bodyEqualityComparer.Equals(originalBody, modifiedBodyMock.Object));
		}

		public void TestEquals_DeliveryTermsDescription()
		{
			var modifiedBodyMock = BodyMock;
			modifiedBodyMock.Setup(x => x.DeliveryTermsDescription).Returns("BBB");
			AssertEquals(false, bodyEqualityComparer.Equals(originalBody, modifiedBodyMock.Object));
		}

		public void TestEquals_DeliveryTermsKey()
		{
			var modifiedBodyMock = BodyMock;
			modifiedBodyMock.Setup(x => x.DeliveryTermsKey).Returns("BBB");
			AssertEquals(false, bodyEqualityComparer.Equals(originalBody, modifiedBodyMock.Object));
		}

		public void TestEquals_DeliveryTermsPlace()
		{
			var modifiedBodyMock = BodyMock;
			modifiedBodyMock.Setup(x => x.DeliveryTermsPlace).Returns("BBB");
			AssertEquals(false, bodyEqualityComparer.Equals(originalBody, modifiedBodyMock.Object));
		}

		public void TestEquals_PaymentTransaction()
		{
			var modifiedBodyMock = BodyMock;
			modifiedBodyMock.Setup(x => x.PaymentTransaction).Returns(GetPaymentTransactionMock("AUR").Object);
			AssertEquals(false, bodyEqualityComparer.Equals(originalBody, modifiedBodyMock.Object));
		}

		public void TestEquals_ForeignTradeStatisticsEntryCustomsOffice()
		{
			var modifiedBodyMock = BodyMock;
			modifiedBodyMock.Setup(x => x.ForeignTradeStatisticsEntryCustomsOffice).Returns("DE0002");
			AssertEquals(false, bodyEqualityComparer.Equals(originalBody, modifiedBodyMock.Object));
		}

		public void TestEquals_CustomsValue()
		{
			var modifiedBodyMock = BodyMock;
			modifiedBodyMock.Setup(x => x.CustomsValue).Returns(GetCustomsValue("DDD").Object);
			AssertEquals(false, bodyEqualityComparer.Equals(originalBody, modifiedBodyMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalBody = BodyMock.Object;
			bodyEqualityComparer = new IMonthlyClosingDecBodyEqualityComparer();
		}
		IMonthlyClosingDecBody originalBody;
		IMonthlyClosingDecBodyEqualityComparer bodyEqualityComparer;

		Mock<IMonthlyClosingDecBody> BodyMock
		{
			get
			{
				var bodyMock = new Mock<IMonthlyClosingDecBody>();
				bodyMock.Setup(x => x.ConsigneePK).Returns(new Guid("11111111-0000-0000-0000-000000000000"));
				bodyMock.Setup(x => x.DeliveryTermsCode).Returns("AAA");
				bodyMock.Setup(x => x.DeliveryTermsDescription).Returns("DDD");
				bodyMock.Setup(x => x.DeliveryTermsKey).Returns("KKK");
				bodyMock.Setup(x => x.DeliveryTermsPlace).Returns("PPP");
				bodyMock.Setup(x => x.PaymentTransaction).Returns(GetPaymentTransactionMock("EUR").Object);
				bodyMock.Setup(x => x.ForeignTradeStatisticsEntryCustomsOffice).Returns("DE0001");
				bodyMock.Setup(x => x.CustomsValue).Returns(GetCustomsValue("FFF").Object);
				return bodyMock;
			}
		}

		Mock<ICustomsValue> GetCustomsValue(string formerDecisions)
		{
			var rateMock = new Mock<ICustomsValue>();
			rateMock.Setup(a => a.FormerDecisions).Returns(formerDecisions);
			return rateMock;
		}

		Mock<IMoney> GetPaymentTransactionMock(ZString currencyCode)
		{
			var rateMock = new Mock<IMoney>();
			rateMock.Setup(a => a.CurrencyCode).Returns(currencyCode);
			return rateMock;
		}
	}
}
