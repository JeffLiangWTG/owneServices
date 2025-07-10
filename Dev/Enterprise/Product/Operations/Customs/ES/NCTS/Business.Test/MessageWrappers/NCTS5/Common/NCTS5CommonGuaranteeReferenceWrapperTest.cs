using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonGuaranteeReferenceWrapperTest : WrapperHelperTest<NCTS5CommonGuaranteeReferenceWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if guarantee is null", typeof(ArgumentNullException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "guarantee"), () => new NCTS5CommonGuaranteeReferenceWrapper(null, 1));
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		public void TestGRN()
		{
			guarantee.PW_BondNumber = "reference";
			AssertEquals("Expected filled GRN", "reference", wrapper.GRN);
		}

		public void TestAccessCode()
		{
			CombineAssertions(() =>
			{
				guarantee.PW_BondType = "4";
				guarantee.PW_Password = "pass";
				AssertEquals("Expected filled AccessCode when type is 4", "pass", wrapper.AccessCode);

				guarantee.PW_BondType = "3";
				AssertEquals("Expected empty AccessCode when type is not 4 or 0 and 1 and grn doesn't start with ES (3)", ZString.Empty, wrapper.AccessCode);

				guarantee.PW_BondType = "0";
				AssertEquals("Expected filled AccessCode when type is 0 (or 1) and grn doesn't start with ES", "pass", wrapper.AccessCode);

				guarantee.PW_BondNumber = "ESref";
				AssertEquals("Expected empty AccessCode when type is 0 (or 1) and GRN starts with ES", ZString.Empty, wrapper.AccessCode);

				guarantee.PW_BondNumber = "ref";
				guarantee.PW_BondType = "1";
				AssertEquals("Expected filled AccessCode when type is 1 (or 0) and grn doesn't start with ES", "pass", wrapper.AccessCode);

				guarantee.PW_BondNumber = "ESref";
				AssertEquals("Expected empty AccessCode when type is 1 (or 0) and GRN starts with ES", ZString.Empty, wrapper.AccessCode);
			});
		}

		public void TestAmountToBeCovered()
		{
			guarantee.PW_BondAmount = 20.30m;
			AssertEquals("Expected filled AmountToBeCovered", 20.30m, wrapper.AmountToBeCovered);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			guarantee = (NctsGuarantee)nctsHeader.GetEffectiveGuarantees().AddNew();

			wrapper = new NCTS5CommonGuaranteeReferenceWrapper(guarantee, 1);
		}

		NctsGuarantee guarantee;
		NCTS5CommonGuaranteeReferenceWrapper wrapper;

		protected override NCTS5CommonGuaranteeReferenceWrapper GetProvider() => wrapper;
	}
}
