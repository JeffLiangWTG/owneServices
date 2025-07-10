using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class GuaranteeReferenceProviderTest : DataProviderTestCase<GuaranteeReferenceProvider>
	{
		public void TestConstructor()
		{
#if NETFRAMEWORK
			var message = "Value cannot be null.\r\nParameter name: guarantee";
#else
			var message = @"Value cannot be null. (Parameter 'guarantee')";
#endif

			AssertExceptionThrown<ArgumentNullException>(
				"Expected ArgumentNullException with provided message.",
				message,
				() => new GuaranteeReferenceProvider(null, 1));
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", provider.SequenceNumber);
		}

		public void TestGrn()
		{
			guarantee.PW_BondNumber = "reference";
			AssertEquals("Expected filled GRN", "reference", provider.Grn);
		}

		public void TestAccessCode()
		{
			guarantee.PW_Password = "pass";
			AssertEquals("Expected filled AccessCode", "pass", provider.AccessCode);
		}

		public void TestAmountToBeCovered()
		{
			guarantee.PW_BondAmount = 20.30m;
			AssertEquals("Expected filled AmountToBeCovered", 20.30m, provider.AmountToBeCovered);
		}

		public void TestCurrencyCode()
		{
			guarantee.PW_RX_NKCurrency = "EUR";
			AssertEquals("Expected filled CurrencyCode", "EUR", provider.CurrencyCode);
		}

		public void TestOtherGuaranteeReference()
		{
			guarantee.PW_GuaranteeDescription = "GuaranteeDescription";
			AssertEquals("Expected filled OtherGuaranteeReference", "GuaranteeDescription", provider.OtherGuaranteeReference);
		}

		public void TestCcQualifier()
		{
			guarantee.PW_RN_NKCountryOfIssue = "BE";
			AssertEquals("Expected CcQualifier", "BE", provider.CcQualifier);
		}

		public void TestCustomsOfficeOfGuarantee()
		{
			guarantee.PW_BondFiledPort = "BFP";
			AssertEquals("Expected CustomsOfficeOfGuarantee", "BFP", provider.CustomsOfficeOfGuarantee);
		}

		protected override void SetUp()
		{
			base.SetUp();

			guarantee = Factory.New<GuaranteeForEntryInstruction>();

			provider = new GuaranteeReferenceProvider(guarantee, 1);
		}

		GuaranteeForEntryInstruction guarantee;
		GuaranteeReferenceProvider provider;

		protected override GuaranteeReferenceProvider GetProvider() => provider;
	}
}
