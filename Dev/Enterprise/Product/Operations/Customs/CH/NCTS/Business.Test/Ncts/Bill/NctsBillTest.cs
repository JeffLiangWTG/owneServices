using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsBill))]
sealed class NctsBillTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => NctsBill;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateBill(factory, EU.NCTS.Business.NctsMovementType.Codes.Departure);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => NctsBill;

	public void TestGoodsItems() => AssertType<NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(NctsBill.GoodsItems);

	public void TestArrivalGoodsItems()
	{
		var nctsBill = CreateBill(Factory, EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		AssertType<NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>>("ArrivalGoodsItems Type", nctsBill.ArrivalGoodsItems);
	}

	public void TestCusSupplyChainActors() => AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(NctsBill.CusSupplyChainActorReferences);

	public void TestReadOnlyMembersDeparture() => CombineAssertions(() =>
	{
		AssertEquals("SupportingDocuments ReadOnly", false, NctsBill.SupportingDocuments.ReadOnly);
		AssertEquals("AdditionalDocuments ReadOnly", false, NctsBill.AdditionalDocuments.ReadOnly);
		AssertEquals("PreviousDocuments ReadOnly", false, NctsBill.PreviousDocuments.ReadOnly);
	});

	public void TestReadOnlyMembersArrival() => CombineAssertions(() =>
	{
		var nctsBill = CreateBill(Factory, EU.NCTS.Business.NctsMovementType.Codes.Arrival);

		AssertEquals("SupportingDocuments ReadOnly", true, nctsBill.SupportingDocuments.ReadOnly);
		AssertEquals("AdditionalDocuments ReadOnly", true, nctsBill.AdditionalDocuments.ReadOnly);
		AssertEquals("PreviousDocuments ReadOnly", true, nctsBill.PreviousDocuments.ReadOnly);
	});

	public void TestUnloadingRemarkCode_Caption() => AssertEquals("UnloadingRemarkCode", "Unloading Code", NctsBill.UnloadingRemarkCodeInfo.Description);

	public void TestUnloadingRemarkText_Caption() => AssertEquals("UnloadingRemarkText", "Unloading Remarks", NctsBill.UnloadingRemarkTextInfo.Description);

	public void TestHouseConsignmentDifferencesCollection() => CombineAssertions(() =>
	{
		AssertType<HouseConsignmentDifferencesCollection>(NctsBill.HouseConsignmentDifferences);
		AssertType<HouseConsignmentDifferences>(NctsBill.HouseConsignmentDifferences.AddNew());
		AssertSame("Cached", NctsBill.HouseConsignmentDifferences, NctsBill.HouseConsignmentDifferences);
	});

	public void TestHouseConsignmentDifferences() => CombineAssertions(() =>
	{
		AssertEquals("Collection initially empty", 0, NctsBill.HouseConsignmentDifferences.Count);
		AssertEquals("UnloadingRemarkCode initial state", ZString.Empty, NctsBill.UnloadingRemarkCode);
		AssertEquals("UnloadingRemarksText initial state", ZString.Empty, NctsBill.UnloadingRemarkText);

		NctsBill.UnloadingRemarkCode = UnloadingRemarkCodeList.Codes.NotShipped;
		NctsBill.UnloadingRemarkText = "Description 1";

		AssertEquals("Collection value set", 1, NctsBill.HouseConsignmentDifferences.Count);
		AssertEquals("UnloadingRemarkCode set", UnloadingRemarkCodeList.Codes.NotShipped, NctsBill.HouseConsignmentDifference.CY_Code);
		AssertEquals("UnloadingRemarksText set", "Description 1", NctsBill.HouseConsignmentDifference.CY_Data);

		NctsBill.UnloadingRemarkCode = UnloadingRemarkCodeList.Codes.Stolen;
		NctsBill.UnloadingRemarkText = "Description 2";
		AssertEquals("Collection value changed", 1, NctsBill.HouseConsignmentDifferences.Count);
	});

	public void TestClearUnloadingRemarks() => CombineAssertions(() =>
	{
		nctsBill = CreateBill(Factory, NctsMovementType.Codes.Arrival);

		nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		nctsBill.UnloadingRemarkCode = UnloadingRemarkCodeList.Codes.NotShipped;
		nctsBill.UnloadingRemarkText = "Text";

		nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;

		AssertEquals("Clear UnloadingRemarkCode", ZString.Empty, NctsBill.UnloadingRemarkCode);
		AssertEquals("Clear UnloadingRemarkText", ZString.Empty, NctsBill.UnloadingRemarkText);
	});

	public void TestB0_TransportPaymentMethod_ReadOnly() => CombineAssertions(() =>
	{
		nctsBill = CreateBill(Factory, NctsMovementType.Codes.Departure);
		nctsBill.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		var movementHeader = nctsBill.Header.MovementHeader;

		movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		AssertEquals("Method of Payment not empty, Type Of Security is ENT.", false, nctsBill.B0_TransportPaymentMethodInfo.ReadOnly);

		movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		AssertEquals("Method of Payment not empty, Type Of Security is NON.", false, nctsBill.B0_TransportPaymentMethodInfo.ReadOnly);

		movementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
		AssertEquals("Method of Payment is empty", true, nctsBill.B0_TransportPaymentMethodInfo.ReadOnly);

		nctsBill = CreateBill(Factory, NctsMovementType.Codes.Arrival);
		AssertEquals("Arrival bill", false, nctsBill.B0_TransportPaymentMethodInfo.ReadOnly);
	});

	public void TestConsigneeJobDocAddress_NS30132() => CombineAssertions(() =>
	{
		const string message1 = "[NS30132] When already present in Declaration, Consignee must not be specified in House Consignment.";
		const string message2 = "[NS30132] You have not entered Consignee. It is required either on Declaration or House Consignment.";

		var address = Factory.New<JobDocAddress>();

		AssertMessage(true, true, message1);
		AssertMessage(true, false, null);
		AssertMessage(false, true, null);
		AssertMessage(false, false, message2);

		AssertMessage(true, true, null, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure);
		AssertMessage(false, false, null, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure);

		void AssertMessage(bool consigneeAtHeader, bool consigneeAtBill, string expectedMessage, string inBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland)
		{
			NctsBill.Header.MovementHeader.BM_InBondEntryType = inBondEntryType;
			NctsBill.Header.Consignee.E2_OA_Address = consigneeAtHeader ? address.PK : ZGuid.Empty;
			NctsBill.Consignee.E2_OA_Address = consigneeAtBill ? address.PK : ZGuid.Empty;
			NctsBill.Consignee.Validation.ValidateOrganisationPK();
			if (expectedMessage != null)
			{
				AssertHasMessageError(AssertionMessage(), nctsBill.Consignee.OrganisationPKInfo, expectedMessage);
			}
			if (expectedMessage != message1)
			{
				AssertNoMessageError(AssertionMessage(), nctsBill.Consignee.OrganisationPKInfo, message1);
			}
			if (expectedMessage != message2)
			{
				AssertNoMessageError(AssertionMessage(), nctsBill.Consignee.OrganisationPKInfo, message2);
			}
			string AssertionMessage() => $"consigneeAtHeader={consigneeAtHeader} consigneeAtBill={consigneeAtBill}, inBondEntryType={inBondEntryType}";
		}
	});

	public void TestConsigneeJobDocAddress_NS30018() => CombineAssertions(() =>
	{
		PrepareCTCCountriesRefCusCodeList();

		var address = Factory.New<JobDocAddress>();
		NctsBill.GoodsItems.AddNew();
		NctsBill.GoodsItems.AddNew();
		bool inCHNT015V4;

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, value: false))
		{
			inCHNT015V4 = false;
			AssertMessage(true, true, ZString.Empty, ZString.Empty, ZString.Empty, PassarValidationMessages.MessageNS30018_1);
			AssertMessage(false, false, "GB", ZString.Empty, ZString.Empty, PassarValidationMessages.MessageNS30018_2);
			AssertMessage(false, false, "FR", ZString.Empty, ZString.Empty, null);
			AssertMessage(false, false, ZString.Empty, "GB", ZString.Empty, PassarValidationMessages.MessageNS30018_3);
			AssertMessage(false, false, ZString.Empty, "FR", ZString.Empty, null);
			AssertMessage(false, false, ZString.Empty, ZString.Empty, "GB", PassarValidationMessages.MessageNS30018_3);
			AssertMessage(false, false, ZString.Empty, ZString.Empty, "FR", null);

			AssertMessage(true, true, ZString.Empty, ZString.Empty, ZString.Empty, null, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland);
			AssertMessage(false, false, "GB", ZString.Empty, ZString.Empty, null, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland);
			AssertMessage(false, false, ZString.Empty, "GB", ZString.Empty, null, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland);
			AssertMessage(false, false, ZString.Empty, "GB", ZString.Empty, null, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland);
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, value: true))
		{
			inCHNT015V4 = true;
			AssertMessage(true, true, ZString.Empty, ZString.Empty, ZString.Empty, NctsBill.Header.Configuration.ValidationRuleConfiguration.Messages.C0001_2aMessage);
			AssertMessage(false, false, "GB", ZString.Empty, ZString.Empty, null);
			AssertMessage(false, false, ZString.Empty, "GB", ZString.Empty, null);
			AssertMessage(false, false, ZString.Empty, ZString.Empty, "GB", null);
		}

		void AssertMessage(bool consigneeAtHeader, bool consigneeAtBill, ZString headerCountryOfDestination, ZString billCountryOfDestination, ZString goodsItemCountryOfDestination, string expectedMessage, string inBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure)
		{
			NctsBill.Header.MovementHeader.BM_InBondEntryType = inBondEntryType;
			NctsBill.Header.MovementHeader.BM_RL_NKDestinationPort = headerCountryOfDestination;
			NctsBill.Header.Consignee.E2_OA_Address = consigneeAtHeader ? address.PK : ZGuid.Empty;
			NctsBill.Consignee.E2_OA_Address = consigneeAtBill ? address.PK : ZGuid.Empty;
			NctsBill.B0_RN_NKCountryOfDestination = billCountryOfDestination;
			NctsBill.GoodsItems[1].BY_RN_NKCountryOfDestination = goodsItemCountryOfDestination;

			NctsBill.Consignee.Validation.ValidateOrganisationPK();
			if (expectedMessage != null)
			{
				AssertHasMessageError(AssertionMessage(), NctsBill.Consignee.OrganisationPKInfo, expectedMessage);
			}
			if (expectedMessage != PassarValidationMessages.MessageNS30018_1)
			{
				AssertNoMessageError(AssertionMessage(), NctsBill.Consignee.OrganisationPKInfo, PassarValidationMessages.MessageNS30018_1);
			}
			if (expectedMessage != PassarValidationMessages.MessageNS30018_2)
			{
				AssertNoMessageError(AssertionMessage(), NctsBill.Consignee.OrganisationPKInfo, PassarValidationMessages.MessageNS30018_2);
			}
			if (expectedMessage != PassarValidationMessages.MessageNS30018_3)
			{
				AssertNoMessageError(AssertionMessage(), NctsBill.Consignee.OrganisationPKInfo, PassarValidationMessages.MessageNS30018_3);
			}
			string AssertionMessage() => $"inCHNT015V4= {inCHNT015V4} consigneeAtHeader={consigneeAtHeader} consigneeAtBill={consigneeAtBill} headerCountryOfDestination={headerCountryOfDestination} billCountryOfDestination={billCountryOfDestination} goodsItemCountryOfDestination={goodsItemCountryOfDestination} inBondEntryType={inBondEntryType}";
		}

		void PrepareCTCCountriesRefCusCodeList()
		{
			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(Factory, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, (CountryCodes.Switzerland, "Switzerland"), (CountryCodes.Norway, "Norway"), (CountryCodes.UnitedKingdom, "United Kingdom"));
			Factory.Save();
		}
	});

	public void TestAdditionalDocuments()
	{
		AssertType<NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>>(NctsBill.AdditionalDocuments);
	}

	public void TestSupportingDocuments()
	{
		AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>(NctsBill.SupportingDocuments);
	}

	public void TestPreviousDocuments()
	{
		AssertType<CommonPreviousDocument>(NctsBill.PreviousDocuments.AddNew());
	}

	public void TestCusSupportingInfoTypes()
	{
		var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)NctsBill).GetCusSupportingInfoTypes();
		CombineAssertions(() =>
		{
			AssertEquals("AdditionalInfo", typeof(NctsBillAdditionalDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			AssertEquals("AdditionalInfo", typeof(NctsSupportingDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument]);
		});
	}

	public void TestCusCodeDataTypes()
	{
		var cusCodeDataTypes = NctsBill.GetCusCodeDataTypes();
		CombineAssertions(() =>
		{
			AssertEquals("AdditionalInfo", typeof(HouseConsignmentDifferences), cusCodeDataTypes[CH.Business.CusCodeDataTypeList.Codes.UnloadingRemarks]);
		});
	}

	public void TestSetArrivalTransportInfosReadOnly() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var bill = nctsHeader.Bills.AddNew();
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
		{
			bill.SetArrivalTransportInfosReadOnly();
			AssertEquals("NC5TP active", bill.ArrivalTransportInfos.ReadOnly, true);
		}
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
		{
			bill.SetArrivalTransportInfosReadOnly();
			AssertEquals("NC5TP inactive", bill.ArrivalTransportInfos.ReadOnly, true);
		}
	});

	NctsBill NctsBill => nctsBill ?? (nctsBill = CreateBill(Factory, NctsMovementType.Codes.Departure));
	NctsBill nctsBill;

	NctsBill CreateBill(BusinessObjectFactory factory, string movementType)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(movementType);
		return nctsHeader.Bills.AddNew();
	}
}
