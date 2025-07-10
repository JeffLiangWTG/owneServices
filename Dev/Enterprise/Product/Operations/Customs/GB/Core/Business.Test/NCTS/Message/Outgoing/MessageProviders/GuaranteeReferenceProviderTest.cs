using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class GuaranteeReferenceProviderTest : DataProviderTestCase<GuaranteeReferenceProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeReferenceProvider(null, 1));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestAccessCode()
		{
			AssertEquals("abc", provider.AccessCode);
		}

		public void TestAmountToBeCovered()
		{
			AssertEquals(ZDecimal.Zero, provider.AmountToBeCovered);
		}

		public void TestCurrency()
		{
			AssertEquals("pre-req: default to blank if not specified", "", guarantee.CusGuarantee.CPH_UnitOfMeasure);
			AssertEquals("default to GBP if not present", "GBP", provider.Currency);
			guarantee.CusGuarantee.CPH_UnitOfMeasure = "EUR";
			AssertEquals("return CusPermitHeader currency", "EUR", provider.Currency);
		}

		public void TestGRN()
		{
			AssertEquals("BondNr", provider.GRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			NCTSTestHelper.SetupC0009ForCountries(Factory, (string[])Factory.GetEuropeanUnionAndCtCountries());
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Number = "BondNr";
			guaranteeHeader1.CPH_UnitOfMeasure = "";
			guaranteeHeader1.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader1.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader1.CPH_Type = EUGuaranteeTypeList.Codes.TRA;

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.Principal.E2_OA_Address = org1.MainAddress.PK;
			guarantee = (Guarantee)header.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = "B";
			guarantee.PW_BondNumber = "BondNr";
			guarantee.PW_BondNumber2 = "Other";
			guarantee.PW_BondAmount = ZDecimal.Zero;
			guarantee.PW_Password = "abc";
			guarantee.PW_RX_NKCurrency = "USD";
			provider = new GuaranteeReferenceProvider(guarantee, 1);
		}
		GuaranteeReferenceProvider provider;
		Guarantee guarantee;

		protected override GuaranteeReferenceProvider GetProvider() => provider;
	}
}
