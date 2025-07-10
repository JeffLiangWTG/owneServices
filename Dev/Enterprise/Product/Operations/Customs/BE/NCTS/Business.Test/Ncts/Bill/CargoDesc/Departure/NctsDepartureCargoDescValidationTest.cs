using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescValidationTest : TestCaseWithFactory
{
	public void TestCheckBY_CommercialReferenceNumber()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var nctsBill = nctsHeader.Bills.AddNew();
		var goodsItem = nctsBill.GoodsItems.AddNew();
		var validation = goodsItem.Validation;
		const string RuleC0502ExpectedMessageError = "[C0502] You have not entered a Reference Number / UCR. A Reference Number / UCR at Goods Items level is required if there is no Reference Number / UCR at Header or House Consignments level and there is no Transport Document at Header or House Consignments level.";

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			CombineAssertions("Preconditions: the field should be mandatory by default.", () =>
			{
				AssertEquals("BY_CommercialReferenceNumber", ZString.Empty, goodsItem.BY_CommercialReferenceNumber);
				AssertEquals("BM_UniqueConsignmentReference", ZString.Empty, nctsHeader.MovementHeader.BM_UniqueConsignmentReference);
				AssertEquals("B0_ReferenceID", ZString.Empty, goodsItem.Bill.B0_ReferenceID);
				AssertContainsExactElementsInAnyOrder("Additional Documents for Bill", Array.Empty<string>(), goodsItem.Bill.AdditionalDocuments.Select(x => x.CSI_SubType));
				AssertContainsExactElementsInAnyOrder("Additional Documents for Declaration", Array.Empty<string>(), nctsHeader.AdditionalDocuments.Select(x => x.CSI_SubType));

				goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
				AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
			});

			goodsItem.BY_CommercialReferenceNumber = "A";
			AssertNoMessageErrors("Entering a commercial reference number should clear the message error, obviously.", goodsItem.BY_CommercialReferenceNumberInfo);

			goodsItem.BY_CommercialReferenceNumber = ZString.Empty;
			AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
			AssertNoMessageErrors("during the transition period, no need to check", goodsItem.BY_CommercialReferenceNumberInfo);
		}
	}
}
