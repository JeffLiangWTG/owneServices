using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSGuaranteeProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSGuaranteeProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(NCTSGuaranteeProvider.NewOrNull(null));
		}

		public void TestArgumentNull() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>(() => new NCTSGuaranteeProvider("8", null));
			AssertExceptionThrown<ArgumentException>(() => new NCTSGuaranteeProvider(null, Array.Empty<NctsGuarantee>()));
		});

		public void TestType()
		{
			AssertEquals(NctsGuaranteeTypeList.Codes._0, Provider.Type);
		}

		public void TestOtherReference()
		{
			AssertNull(Provider.OtherReference);
		}

		public void TestOtherReference_8()
		{
			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._8;
			AssertEquals("DE1234567", Provider.OtherReference);
		}

		public void TestOtherReference_R()
		{
			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes.R;
			AssertEquals("DE1234567", Provider.OtherReference);
		}

		public void TestReferences_0() => AssertMultipleReferences(NctsGuaranteeTypeList.Codes._0);
		public void TestReferences_1() => AssertMultipleReferences(NctsGuaranteeTypeList.Codes._1);
		public void TestReferences_2() => AssertMultipleReferences(NctsGuaranteeTypeList.Codes._2);
		public void TestReferences_3() => AssertReferenceCount(NctsGuaranteeTypeList.Codes._3, 1);
		public void TestReferences_4() => AssertMultipleReferences(NctsGuaranteeTypeList.Codes._4);
		public void TestReferences_8() => AssertReferenceCount(NctsGuaranteeTypeList.Codes._8, 0);
		public void TestReferences_B() => AssertReferenceCount(NctsGuaranteeTypeList.Codes.B, 0);
		public void TestReferences_R() => AssertReferenceCount(NctsGuaranteeTypeList.Codes.R, 0);

		void AssertReferenceCount(string bondType, int count)
		{
			guarantee.PW_BondType = bondType;
			AssertEquals(count, Provider.References.Count);
		}

		void AssertMultipleReferences(string bondType)
		{
			guarantee.PW_BondType = bondType;
			var guarantee2 = Factory.New<NctsGuarantee>();
			guarantee2.PW_BondType = bondType;
			var provider = new NCTSGuaranteeProvider(bondType, new[] { guarantee, guarantee2 });
			AssertEquals(2, provider.References.Count);
		}

		protected override NCTSGuaranteeProvider GetProvider() => NCTSGuaranteeProvider.NewOrNull(guarantee);

		protected override void SetUp()
		{
			base.SetUp();
			guarantee = Factory.New<NctsGuarantee>();
			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._0;
			guarantee.PW_BondNumber2 = "DE1234567";
			guarantee.PW_Password = "1234";
		}
		NctsGuarantee guarantee;
	}
}
