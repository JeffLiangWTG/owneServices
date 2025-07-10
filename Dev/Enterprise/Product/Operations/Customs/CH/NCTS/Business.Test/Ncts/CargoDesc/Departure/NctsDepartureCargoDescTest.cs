using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureCargoDesc))]
sealed class NctsDepartureCargoDescTest : NctsDepartureCargoDescAbstractTest<NctsHeader>
{
	public override int CountSupportingInfoTypes => base.CountSupportingInfoTypes + 1;

	public void TestGetCusSupportingInfoTypes_Restriction()
	{
		AssertEquals(typeof(Restriction), ((Integration.Customs.ICusSupportingInfoTypeSupporter)GoodsItem).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.Restriction]);
	}

	public void TestValidation() => AssertType<NctsDepartureCargoDescValidation>(GoodsItem.Validation);

	public void TestGetNewLookups() => AssertType<NctsDepartureCargoDescLookups>(GoodsItem.Lookups);

	public void TestCusSupplyChainActors() => AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(GoodsItem.CusSupplyChainActorReferences);

	public void TestSupportingDocuments() => AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>(GoodsItem.SupportingDocuments);

	public void TestTariffType()
	{
		AssertEquals(Universal.Constants.TariffTypes.Export, GoodsItem.TariffType);
	}

	public void TestGetNewTariffFormatter()
	{
		AssertType<TariffFormatterCH>(((ITariffFormatProvider)GoodsItem).TariffFormatter);
	}

	public void TestDefaultSecondQuantityUOMFromTariff()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Export);
		Factory.Save();

		AssertEquals("Precondition Customs Second Unit Qty empty", ZString.Empty, GoodsItem.BY_CustomsSecondUnitQty);

		var tariffWithCustomsUOM3Type = helper.CreateTariff(CountryCode, tariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithCustomsUOM3Type, UnitOfMeasureTypes.AdditionalUOMType, "KGM");
		helper.CreateTariffUOM(tariffWithCustomsUOM3Type, UnitOfMeasureTypes.CustomsUOM3Type, "NAR");
		Factory.Save();

		GoodsItem.BY_HarmonisedTariff = "1111111111";
		AssertEquals("Customs Second Unit Qty from Tariff CU3", "NAR", GoodsItem.BY_CustomsSecondUnitQty);
	}

	public void TestEffectiveCountryCodeOfDestination() => CombineAssertions(() =>
	{
		AssertEquals("all empty", ZString.Empty, GoodsItem.EffectiveCountryCodeOfDestination);

		GoodsItem.MoveHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Germany;
		AssertEquals("Country at header level", Core.Constants.CountryCodes.Germany, GoodsItem.EffectiveCountryCodeOfDestination);

		GoodsItem.Bill.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.France;
		AssertEquals("Country at bill level", Core.Constants.CountryCodes.France, GoodsItem.EffectiveCountryCodeOfDestination);

		GoodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Italy;
		AssertEquals("Country at item level", Core.Constants.CountryCodes.Italy, GoodsItem.EffectiveCountryCodeOfDestination);
	});

	public void TestEffectiveCountryCodeOfDispatch() => CombineAssertions(() =>
	{
		AssertEquals("all empty", ZString.Empty, GoodsItem.EffectiveCountryCodeOfDispatch);

		GoodsItem.MoveHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Germany;
		AssertEquals("Country at header level", Core.Constants.CountryCodes.Germany, GoodsItem.EffectiveCountryCodeOfDispatch);

		GoodsItem.Bill.B0_RN_NKCountryOfExport = Core.Constants.CountryCodes.France;
		AssertEquals("Country at bill level", Core.Constants.CountryCodes.France, GoodsItem.EffectiveCountryCodeOfDispatch);

		GoodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Italy;
		AssertEquals("Country at item level", Core.Constants.CountryCodes.Italy, GoodsItem.EffectiveCountryCodeOfDispatch);
	});

	public void TestAdditionalInfos()
	{
		AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(GoodsItem.AdditionalInfos);
	}

	public void TestPreviousDocuments()
	{
		AssertType<NctsPreviousDocumentCollection<NctsPreviousDocument>>(GoodsItem.PreviousDocuments);
	}

	public void TestBY_RN_NKCountryOfDispatch_WhenBM_InBondEntryTypeChanged() => AssertPropertyWhenBM_InBondEntryTypeChanged(GoodsItem.BY_RN_NKCountryOfDispatchInfo);

	public void TestBY_RN_NKCountryOfDestination_WhenBM_InBondEntryTypeChanged() => AssertPropertyWhenBM_InBondEntryTypeChanged(GoodsItem.BY_RN_NKCountryOfDestinationInfo);

	public void TestBY_CusC4Number_WhenBM_InBondEntryTypeChanged() => AssertPropertyWhenBM_InBondEntryTypeChanged(GoodsItem.BY_CusC4NumberInfo);

	void AssertPropertyWhenBM_InBondEntryTypeChanged(ZPropertyInfo property) => CombineAssertions(() =>
	{
		var movementHeader = GoodsItem.MoveHeader;

		property.SetValueFromString("DE");
		AssertProperty("DE", false);

		movementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AssertProperty(ZString.Empty, true);

		movementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		AssertProperty(ZString.Empty, false);

		void AssertProperty(ZString expectedValue, bool expectedReadOnly)
		{
			AssertEquals($"{property.HumanReadableName} Value", expectedValue, property.Value);
			AssertEquals($"{property.HumanReadableName} ReadOnly", expectedReadOnly, property.ReadOnly);
		}
	});

	public void TestConditionC002_IsInactiveOnLineLevel()
	{
		GoodsItem.BY_Description = "xxx";
		GoodsItem.BY_RN_NKCountryOfDestination = "AX";
		AssertEquals("No notification containing 'C002' on OrganisationPKInfo.", false, GoodsItem.Consignee.OrganisationPKInfo.Notifications.Any(n => n.Message.Contains("C002")));
	}

	public void TestIsNationalTransportSwitzerland()
	{
		GoodsItem.MoveHeader.BM_InBondEntryType = EU.NCTS.Business.NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		AssertEquals("NationalTransitSwitzerland false", false, GoodsItem.IsNationalTransitSwitzerland);

		GoodsItem.MoveHeader.BM_InBondEntryType = EU.NCTS.Business.NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AssertEquals("NationalTransitSwitzerland true", true, GoodsItem.IsNationalTransitSwitzerland);
	}

	public void TestRestrictions() => CombineAssertions(() =>
	{
		AssertType<Restriction>(GoodsItem.Restrictions.AddNew());
		AssertEquals("SequenceNumberGenerator works", 2, GoodsItem.Restrictions.AddNew().CSI_LineNo);
	});

	public void TestRestrictions_OnFactorySaving() => CombineAssertions(() =>
	{
		GoodsItem.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		var restriction = GoodsItem.Restrictions.AddNew();
		AssertEquals("Restrictions not empty", 1, GoodsItem.Restrictions.Count);

		GoodsItem.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		Factory.Save();
		AssertEquals("Empty restrictions for EntryType T", 0, GoodsItem.Restrictions.Count);

		GoodsItem.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		restriction = GoodsItem.Restrictions.AddNew();
		GoodsItem.MoveHeader.Delete();
		Factory.Save();
		AssertEquals("Empty restrictions when Header is Deleted", 0, GoodsItem.Restrictions.Count);
	});

	public void TestGetNctsPackageCollection() => AssertType<NctsPackageCollection>(GoodsItem.Packages);

	protected override ZString CountryCode => Core.Constants.CountryCodes.Switzerland;

	protected override BusinessObject GetNewBusinessObject() => CreateGoodsItem(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateGoodsItem(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateGoodsItem(Factory);

	NctsDepartureCargoDesc GoodsItem => goodsItem ?? (goodsItem = CreateGoodsItem(Factory));
	NctsDepartureCargoDesc goodsItem;

	NctsDepartureCargoDesc CreateGoodsItem(BusinessObjectFactory factory)
	{
		var header = factory.NewWithValidTestData<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		return header.Bills.AddNew().GoodsItems.AddNew();
	}
}
