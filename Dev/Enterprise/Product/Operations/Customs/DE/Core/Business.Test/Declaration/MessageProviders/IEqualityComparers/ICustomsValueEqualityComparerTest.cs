using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ICustomsValueEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, customsValueEqualityComparer.GetHashCode(customsValue));
		}

		public void TestEquals()
		{
			AssertEquals(true, customsValueEqualityComparer.Equals(customsValue, CustomsValueMock.Object));
		}

		public void TestEquals_FormerDecisions()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.FormerDecisions).Returns("AAA");
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_VendorPK()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.VendorPK).Returns(new Guid("33333333-0000-0000-0000-000000000000"));
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_VendeePK()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.VendeePK).Returns(new Guid("44444444-0000-0000-0000-000000000000"));
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_AffiliationType()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.AffiliationType).Returns("1");
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_AffiliationDescription()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.AffiliationDescription).Returns("Affiliation Description 2");
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_RestrictionFlag()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.RestrictionFlag).Returns(ZBool.False);
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_ConditionFlag()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.ConditionFlag).Returns(ZBool.True);
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_RestrictionOrConditionDescription()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.RestrictionOrConditionDescription).Returns("Restriction Or Condition Description 2");
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_LicenseFeeFlag()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.LicenseFeeFlag).Returns(ZBool.False);
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_LicenseFeeDescription()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.LicenseFeeDescription).Returns("License Fee Description 2");
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_ResaleFlag()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.ResaleFlag).Returns(ZBool.True);
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_ResaleDescription()
		{
			var modifiedCustomsValueMock = CustomsValueMock;
			modifiedCustomsValueMock.Setup(x => x.ResaleDescription).Returns("Resale Description 2");
			AssertEquals(false, customsValueEqualityComparer.Equals(customsValue, modifiedCustomsValueMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			customsValue = CustomsValueMock.Object;
			customsValueEqualityComparer = new ICustomsValueEqualityComparer();
		}
		ICustomsValue customsValue;
		ICustomsValueEqualityComparer customsValueEqualityComparer;

		Mock<ICustomsValue> CustomsValueMock
		{
			get
			{
				var result = new Mock<ICustomsValue>();
				result.Setup(x => x.FormerDecisions).Returns("CDN");
				result.Setup(x => x.VendorPK).Returns(new Guid("11111111-0000-0000-0000-000000000000"));
				result.Setup(x => x.VendeePK).Returns(new Guid("22222222-0000-0000-0000-000000000000"));
				result.Setup(x => x.FormerDecisions).Returns("CDN");
				result.Setup(x => x.AffiliationType).Returns("0");
				result.Setup(x => x.AffiliationDescription).Returns("Affiliation Description");
				result.Setup(x => x.RestrictionFlag).Returns(ZBool.True);
				result.Setup(x => x.ConditionFlag).Returns(ZBool.False);
				result.Setup(x => x.RestrictionOrConditionDescription).Returns("Restriction Or Condition Description");
				result.Setup(x => x.LicenseFeeFlag).Returns(ZBool.True);
				result.Setup(x => x.LicenseFeeDescription).Returns("License Fee Description");
				result.Setup(x => x.ResaleFlag).Returns(ZBool.False);
				result.Setup(x => x.ResaleDescription).Returns("Resale Description");
				return result;
			}
		}
	}
}
