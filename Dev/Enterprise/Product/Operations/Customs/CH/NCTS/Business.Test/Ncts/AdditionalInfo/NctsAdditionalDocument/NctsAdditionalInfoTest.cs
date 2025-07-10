using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using CoreConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsAdditionalInfo))]
class NctsAdditionalInfoTest : EU.NCTS.Business.Testing.NctsAdditionalInfoTest<NctsAdditionalInfo>
{
	public void TestGetNewLookups()
	{
		var nctsHeader = CreateNctsHeader(Factory);
		var additionalInfo = nctsHeader.AdditionalDocuments.AddNew();
		AssertType<NctsAdditionalInfoLookups>(additionalInfo.Lookups);
	}

	public void TestCSI_SubType_Default_Header() => CombineAssertions(() =>
	{
		var nctsHeader = CreateNctsHeader(Factory);

		nctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AssertEquals("BM_InBondEntryType={nctsHeader.MovementHeader.BM_InBondEntryType}", AdditionalInfoSubTypeList.Codes.TransportDocument, nctsHeader.AdditionalDocuments.AddNew().CSI_SubType);

		nctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		AssertEquals("BM_InBondEntryType={nctsHeader.MovementHeader.BM_InBondEntryType}", AdditionalInfoSubTypeList.Codes.AdditionalReference, nctsHeader.AdditionalDocuments.AddNew().CSI_SubType);
	});

	public void TestCSI_SubType_Default_GoodsItem() => CombineAssertions(() =>
	{
		var nctsHeader = CreateNctsHeader(Factory);
		var nctsBill = nctsHeader.Bills.AddNew();
		var goodsItem = nctsBill.GoodsItems.AddNew();

		nctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AssertEquals("BM_InBondEntryType={nctsHeader.MovementHeader.BM_InBondEntryType}", AdditionalInfoSubTypeList.Codes.AdditionalInformation, goodsItem.AdditionalInfos.AddNew().CSI_SubType);

		nctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		AssertEquals("BM_InBondEntryType={nctsHeader.MovementHeader.BM_InBondEntryType}", AdditionalInfoSubTypeList.Codes.AdditionalReference, goodsItem.AdditionalInfos.AddNew().CSI_SubType);
	});

	public void TestCodeListType_Header() => CombineAssertions(() =>
	{
		var nctsHeader = CreateNctsHeader(Factory);
		var additionalDocument = nctsHeader.AdditionalDocuments.AddNew();

		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.TransportDocument, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, RefCusCodeListTypes.Codes.Code_TD44N);
		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.TransportDocument, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, RefCusCodeListTypes.Codes.Code_TD44N);

		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.AdditionalReference, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, RefCusCodeListTypes.Codes.Code_AR44N);
		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.AdditionalReference, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, ZString.Empty);

		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.AdditionalInformation, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, RefCusCodeListTypes.Codes.Code_AI44N);
		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.AdditionalInformation, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, CoreConstants.RefCusCodeListTypes.Codes.ExportAddDocAdditionalInformation);

		AssertCodeListType(additionalDocument, "XXX", NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, RefCusCodeListTypes.Codes.Code_AI44N);
		AssertCodeListType(additionalDocument, "XXX", NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, RefCusCodeListTypes.Codes.Code_AI44N);
	});

	public void TestCodeListType_GoodsItems() => CombineAssertions(() =>
	{
		var nctsHeader = CreateNctsHeader(Factory);
		var additionalDocument = nctsHeader.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew();

		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.TransportDocument, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, ZString.Empty);
		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.TransportDocument, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, ZString.Empty);

		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.AdditionalReference, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, RefCusCodeListTypes.Codes.Code_AR44N);
		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.AdditionalReference, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, ZString.Empty);

		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.AdditionalInformation, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, RefCusCodeListTypes.Codes.Code_AI44N);
		AssertCodeListType(additionalDocument, AdditionalInfoSubTypeList.Codes.AdditionalInformation, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, CoreConstants.RefCusCodeListTypes.Codes.ExportAddDocAdditionalInformation);

		AssertCodeListType(additionalDocument, "XXX", NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, RefCusCodeListTypes.Codes.Code_AI44N);
		AssertCodeListType(additionalDocument, "XXX", NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, RefCusCodeListTypes.Codes.Code_AI44N);
	});

		public void TestCodeListType_Arrival() => CombineAssertions(() =>
		{
			var arrivalNctsHeader = CreateNctsHeader(Factory, Common.EU.NctsMoveHeaderType.Codes.Arrival);
			var additionalDocument = arrivalNctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			AssertEquals("For NctsArrivalMovementHeader Type should be: ", RefCusCodeListTypes.Codes.Code_AI44N, additionalDocument.CodeListType);

		additionalDocument = arrivalNctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();
		AssertEquals("For NctsArrivalCargoDesc Type should be: ", RefCusCodeListTypes.Codes.Code_AI44N, additionalDocument.CodeListType);
	});

	void AssertCodeListType(NctsAdditionalInfo additionalDocument, string subType, string inBondEntryType, string expectedResult)
	{
		additionalDocument.CSI_SubType = subType;
		additionalDocument.Header.MovementHeader.BM_InBondEntryType = inBondEntryType;
		AssertEquals($"When SubType is {subType}, InBondEntryType is {inBondEntryType}, Type should be: ", expectedResult, additionalDocument.CodeListType);
	}

		protected override IEnumerable<NctsAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var departureNctsHeader = CreateNctsHeader(factory, Common.EU.NctsMoveHeaderType.Codes.Departure);
			yield return departureNctsHeader.AdditionalDocuments.AddNew();
			yield return departureNctsHeader.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew();
			var arrivalNctsHeader = CreateNctsHeader(factory, Common.EU.NctsMoveHeaderType.Codes.Arrival);
			yield return (NctsAdditionalInfo)arrivalNctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			yield return arrivalNctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(Factory, Common.EU.NctsMoveHeaderType.Codes.Departure);
		}

	NctsAdditionalInfo GetNewBusinessObject(BusinessObjectFactory factory, string movementType)
	{
		var nctsHeader = CreateNctsHeader(factory, movementType);
		return nctsHeader.AdditionalDocuments.AddNew();
	}

	NctsHeader CreateNctsHeader(BusinessObjectFactory factory, string movementType = Common.EU.NctsMoveHeaderType.Codes.Departure)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(movementType);
		return nctsHeader;
	}
}
