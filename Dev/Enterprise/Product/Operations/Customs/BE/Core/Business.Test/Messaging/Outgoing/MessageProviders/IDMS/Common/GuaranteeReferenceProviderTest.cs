using System;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class GuaranteeReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeReferenceProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentException>(() => new GuaranteeReferenceProvider(null, 1));
	}

	public void TestSequenceNumber()
	{
		AssertEquals(3, provider.SequenceNumber);
	}

	public void TestGRN()
	{
		guarantee.PW_BondNumber = "grn";
		AssertEquals("grn", provider.GRN);
	}

	public void TestAccessCode()
	{
		guarantee.PW_Password = "acc";
		AssertEquals("acc", provider.AccessCode);
	}

	public void TestAmountToBeCovered()
	{
		guarantee.PW_BondAmount = 111;
		AssertEquals(111m, provider.AmountToBeCovered);
	}

	public void TestCurrency()
	{
		guarantee.PW_RX_NKCurrency = "EUR";
		AssertEquals("EUR", provider.Currency);
	}

	public void TestCCQualifier()
	{
		AssertEquals("NVT", provider.CCQualifier);
	}

	public void TestOtherGuaranteeReference()
	{
		guarantee.PW_GuaranteeDescription = "other";
		AssertEquals("other", provider.OtherGuaranteeReference);
	}

	public void TestCustomsOfficeOfGuaranteeReferenceNumber()
	{
		guarantee.PW_BondFiledPort = "offref";
		AssertEquals("offref", provider.CustomsOfficeOfGuaranteeReferenceNumber);
	}

	protected override GuaranteeReferenceProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		guarantee = Factory.New<CommonGuarantee>();
		provider = new GuaranteeReferenceProvider(guarantee, 3);
	}
	CommonGuarantee guarantee;
	GuaranteeReferenceProvider provider;
}
