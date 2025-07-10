using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using CoreConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsBillAdditionalDocument))]
sealed class NctsBillAdditionalDocumentTest : CusSupportingInfoTest<NctsBillAdditionalDocument>
{
	public void TestLookups()
	{
		AssertType<NctsBillAdditionalDocumentLookups>(AdditionalDocument.Lookups);
	}

	protected override void LoadParentIfNeeded(BusinessObjectFactory factory, NctsBillAdditionalDocument bizObj)
	{
		base.LoadParentIfNeeded(factory, bizObj);
		factory.Load<NctsBill>(bizObj.CSI_ParentID);
	}

	public void TestGetCodeTypeBySubType()
	{
		CombineAssertions(() =>
		{
			AssertSubType(AdditionalInfoSubTypeList.Codes.TransportDocument, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, RefCusCodeListTypes.Codes.Code_TD44N);
			AssertSubType(AdditionalInfoSubTypeList.Codes.TransportDocument, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, RefCusCodeListTypes.Codes.Code_TD44N);

			AssertSubType(AdditionalInfoSubTypeList.Codes.AdditionalReference, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, RefCusCodeListTypes.Codes.Code_AR44N);
			AssertSubType(AdditionalInfoSubTypeList.Codes.AdditionalReference, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, ZString.Empty);

			AssertSubType(AdditionalInfoSubTypeList.Codes.AdditionalInformation, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, RefCusCodeListTypes.Codes.Code_AI44N);
			AssertSubType(AdditionalInfoSubTypeList.Codes.AdditionalInformation, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, CoreConstants.RefCusCodeListTypes.Codes.ExportAddDocAdditionalInformation);

			AssertSubType("XXX", NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, ZString.Empty);
			AssertSubType("XXX", NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, ZString.Empty);
		});

		void AssertSubType(string subType, string inBondEntryType, string expectedResult)
		{
			AdditionalDocument.CSI_SubType = subType;
			AdditionalDocument.Header.MovementHeader.BM_InBondEntryType = inBondEntryType;
			AssertEquals($"When subType is {subType}, InBondEntryType is {inBondEntryType}, Code type should be: ", expectedResult, additionalDocument.GetCodeTypeBySubType());
		}
	}

	NctsBillAdditionalDocument AdditionalDocument => additionalDocument ?? (additionalDocument = GetNewAdditionalDocument(Factory));
	NctsBillAdditionalDocument additionalDocument;

	NctsBillAdditionalDocument GetNewAdditionalDocument(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		return nctsHeader.Bills.AddNew().AdditionalDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewAdditionalDocument(Factory);

	protected override IEnumerable<NctsBillAdditionalDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewAdditionalDocument(factory);
	}
}
