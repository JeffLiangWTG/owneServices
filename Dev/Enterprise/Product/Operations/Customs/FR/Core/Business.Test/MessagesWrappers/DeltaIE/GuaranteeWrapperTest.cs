using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class GuaranteeWrapperTest : DataProviderTestCase<GuaranteeWrapper>
	{
		public void TestGuaranteeReference()
		{
			var guaranteeReference = Provider.GuaranteeReference.First();
			CombineAssertions("GuaranteeReference should use GuaranteeReferenceWrapper.", () =>
			{
				AssertEquals("AccessCode should equal guarantee PW_Password.", "1234", guaranteeReference.AccessCode);
				AssertEquals("AmountToBeCovered should equal guarantee PW_BondAmount.", 123.456d, guaranteeReference.AmountToBeCovered);
				AssertNull("CcQualifier should never be written for France.", guaranteeReference.CcQualifier);
				AssertEquals("CurrencyCode should equal guarantee PW_RX_NKCurrency.", Core.Constants.CurrencyCodes.France, guaranteeReference.CurrencyCode);
				AssertEquals("CustomsOfficeOfGuarantee should use CustomsOfficeOfGuaranteeWrapper.", "FR000001", guaranteeReference.CustomsOfficeOfGuarantee.ReferenceNumber);
				AssertEquals("Grn should equal guarantee PW_BondNumber.", "GRN0001", guaranteeReference.Grn);
				AssertEquals("OtherGuaranteeReference should equal guarantee PW_BondNumber2.", "OTH0001", guaranteeReference.OtherGuaranteeReference);
			});
		}

		public void TestGuaranteeType()
		{
			AssertEquals("GuaranteeType should equal guarantee PW_BondType.", EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, Provider.GuaranteeType);
		}

		protected override GuaranteeWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = entryInstruction.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GRN0001";
			guarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee;
			guarantee.PW_Password = "1234";
			guarantee.PW_RX_NKCurrency = Core.Constants.CurrencyCodes.France;
			guarantee.PW_BondNumber2 = "OTH0001";
			guarantee.PW_BondFiledPort = "FR000001";
			guarantee.PW_BondAmount = 123.456m;
			return GuaranteeWrapper.New(guarantee);
		}
	}
}
