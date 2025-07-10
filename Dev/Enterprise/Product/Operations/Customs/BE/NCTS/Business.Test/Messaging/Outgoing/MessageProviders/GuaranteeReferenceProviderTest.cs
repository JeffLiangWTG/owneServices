using System;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeReferenceProvider))]
	sealed class GuaranteeReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeReferenceProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeReferenceProvider(null, 1));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestAccessCode()
		{
			SetupGuarantee();
			guarantee.PW_Password = "abc";
			AssertEquals("abc", Provider.AccessCode);
		}

		public void TestAmountToBeCovered()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", 10000m, Provider.AmountToBeCovered);

				guarantee.PW_BondAmount = 12.3456;
				AssertEquals("Amount to be covered must be rounded on 2 decimals", 12.35m, Provider.AmountToBeCovered);
			});
		}

		public void TestCurrency()
		{
			SetupGuarantee();
			guarantee.PW_BondAmount = 100;
			guarantee.PW_RX_NKCurrency = "EUR";
			AssertEquals("EUR", Provider.Currency);
		}

		public void TestCurrency_RetrieveFromGuarantee()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var cusGuarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuarantee.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			cusGuarantee.CPH_Number = "GUA001";
			cusGuarantee.CPH_UnitOfMeasure = Core.Constants.CurrencyCodes.UnitedStates;
			cusGuarantee.CPH_OH_PermitHolder = org1.PK;

			SetupGuarantee();
			var nctsHeader = guarantee.NctsHeader;
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			guarantee.PW_BondNumber = "GUA001";
			guarantee.PW_BondAmount = 100;

			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, Provider.Currency);
		}

		public void TestCurrency_DefaultEUR()
		{
			SetupGuarantee();
			guarantee.PW_BondAmount = 100;
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, Provider.Currency);
		}

		public void TestGRN()
		{
			SetupGuarantee();
			guarantee.PW_BondNumber = "BondNr";
			AssertEquals("BondNr", Provider.GRN);
		}

		public void TestCCQualifier()
		{
			AssertExceptionThrown<NotImplementedException>(() => new ZString(Provider.CCQualifier));
		}

		public void TestOtherGuaranteeReference()
		{
			AssertExceptionThrown<NotImplementedException>(() => new ZString(Provider.OtherGuaranteeReference));
		}

		public void TestCustomsOfficeOfGuaranteeReferenceNumber()
		{
			AssertExceptionThrown<NotImplementedException>(() => new ZString(Provider.CustomsOfficeOfGuaranteeReferenceNumber));
		}

		protected override GuaranteeReferenceProvider GetProvider() => new GuaranteeReferenceProvider(SetupGuarantee(), 1);

		NctsGuarantee SetupGuarantee()
		{
			if (guarantee == null)
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				guarantee = header.MovementHeader.Guarantees.AddNew();
			}
			return guarantee;
		}
		NctsGuarantee guarantee;
	}
}
