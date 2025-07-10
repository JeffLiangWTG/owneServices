using CargoWise.EntityFramework.Testing;
namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusSupplyChainActorReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_Reference_Header()
		{
			var supplyChainActorReferenceHeader = header.CusSupplyChainActors.AddNew();
			AssertHasMessageErrorWithDifferentCfrReferenceActiveAndInactive(supplyChainActorReferenceHeader);
		}

		public void TestCheckCFR_Reference_Bill()
		{
			var supplyChainActorReferenceBill = bill.CusSupplyChainActorReferences.AddNew();
			AssertHasMessageErrorWithDifferentCfrReferenceActiveAndInactive(supplyChainActorReferenceBill);
		}

		public void TestCheckCFR_Reference_CargoDes()
		{
			var supplyChainActorReferenceGoodsItem = goodsItem.CusSupplyChainActorReferences.AddNew();
			AssertHasMessageErrorWithDifferentCfrReferenceActiveAndInactive(supplyChainActorReferenceGoodsItem);
		}

		public void TestCheckCFR_Reference_DepartureMovementHeader()
		{
			var supplyChainActorReferenceDepartureMovementHeader = departureMovementHeader.CusSupplyChainActors.AddNew();
			AssertHasMessageErrorWithDifferentCfrReferenceActiveAndInactive(supplyChainActorReferenceDepartureMovementHeader);
		}

		void AssertHasMessageErrorWithDifferentCfrReferenceActiveAndInactive(CusSupplyChainActorReference cusSupplyChainActorReference)
		{
			const string cfrReferenceWithLetter = "PLT*est";
			const string cfrReferenceNoNumber = "DE";
			const string cfrReferenceInvalidCountry = "AA1234";

			const string cfrReferenceValidForCountry = "PL123456789";
			const string cfrReferenceInvalidForCountry = "PL1234567891234567";

			var propertyInfo = cusSupplyChainActorReference.CFR_ReferenceInfo;
			var message = "[R0840] Format of entered Identification (EORI/TCUIN) is incorrect; please enter in correct format (Nationality Code + National Identification Number i.e. a..2 + an..15)";

			CombineAssertions(() =>
			{
				using var deciderTestContext = new CusSupplyChainActorReferenceValidationDeciderTestContext<ICusSupplyChainActorReferenceValidationDecider>(Factory);
				deciderTestContext.DisableRule(decider => decider.IsRuleR0840Active);

				cusSupplyChainActorReference.CFR_Reference = cfrReferenceWithLetter;
				AssertNoMessageError(propertyInfo, message);

				cusSupplyChainActorReference.CFR_Reference = cfrReferenceNoNumber;
				AssertNoMessageError(propertyInfo, message);

				cusSupplyChainActorReference.CFR_Reference = cfrReferenceInvalidCountry;
				AssertNoMessageError(propertyInfo, message);

				cusSupplyChainActorReference.CFR_Reference = cfrReferenceValidForCountry;
				AssertNoMessageError(propertyInfo, message);

				cusSupplyChainActorReference.CFR_Reference = cfrReferenceInvalidForCountry;
				AssertNoMessageError(propertyInfo, message);

				deciderTestContext.EnableRule(decider => decider.IsRuleR0840Active);

				cusSupplyChainActorReference.CFR_Reference = cfrReferenceWithLetter;
				AssertHasMessageError("The CFR_Reference should start with a country code and contain only alphanumeric characters", cusSupplyChainActorReference.CFR_ReferenceInfo, message);

				cusSupplyChainActorReference.CFR_Reference = cfrReferenceNoNumber;
				AssertHasMessageError("The CFR_Reference should be countrycode + alph a-numeric", cusSupplyChainActorReference.CFR_ReferenceInfo, message);

				cusSupplyChainActorReference.CFR_Reference = cfrReferenceInvalidCountry;
				AssertHasMessageError("The CFR_Reference's prefix countrycode is invalid", cusSupplyChainActorReference.CFR_ReferenceInfo, message);

				cusSupplyChainActorReference.CFR_Reference = cfrReferenceValidForCountry;
				AssertNoMessageError("The CFR_Reference's suffix length for country is right", cusSupplyChainActorReference.CFR_ReferenceInfo, message);

				cusSupplyChainActorReference.CFR_Reference = cfrReferenceInvalidForCountry;
				AssertHasMessageError("The CFR_Reference's suffix length for country is wrong", cusSupplyChainActorReference.CFR_ReferenceInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			bill = header.Bills.AddNew();
			goodsItem = bill.GoodsItems.AddNew();
			departureMovementHeader = header.MovementHeader;
			Factory.Save();
		}

		NctsHeader header;
		NctsBill bill;
		NctsDepartureCargoDesc goodsItem;
		NctsDepartureMovementHeader departureMovementHeader;
	}
}
