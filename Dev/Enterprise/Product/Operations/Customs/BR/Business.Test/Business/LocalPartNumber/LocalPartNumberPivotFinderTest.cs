using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LocalPartNumberPivotFinder))]
	sealed class LocalPartNumberPivotFinderTest : TestCaseWithFactory
	{
		public void TestMatchingPivots()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWN";
			owner.OH_FullName = "TEST COMPANY1";
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "75.400.331/0001-15", Core.Constants.CountryCodes.Brazil);
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "75400331", Core.Constants.CountryCodes.Brazil);

			var orgWithSameRootCNPJ = Factory.New<OrgHeader>();
			orgWithSameRootCNPJ.OH_Code = "XX1";
			orgWithSameRootCNPJ.OH_FullName = "TEST COMPANY2";
			orgWithSameRootCNPJ.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "75.400.331/0001-16", Core.Constants.CountryCodes.Brazil);

			var orgWithOtherRootCNPJ = Factory.New<OrgHeader>();
			orgWithOtherRootCNPJ.OH_Code = "XX2";
			orgWithOtherRootCNPJ.OH_FullName = "TEST COMPANY3";
			orgWithOtherRootCNPJ.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "76.400.331/0001-16", Core.Constants.CountryCodes.Brazil);

			var inactiveProduct = CreateProudct("INACTIVE_PROD", OrgPartRelation.RelationshipTypes.Both, owner, isActive: false);
			CreatePivot(inactiveProduct, ClassificationTypeList.Codes.HTB, owner, "05235345");

			var productNoMatchOrg = CreateProudct("NOMATCHORG_PROD", OrgPartRelation.RelationshipTypes.Both, orgWithOtherRootCNPJ);
			CreatePivot(productNoMatchOrg, ClassificationTypeList.Codes.HTB, owner, "05235345");

			var productNoMatchPivot = CreateProudct("NOMATCHPIVOT_PROD", OrgPartRelation.RelationshipTypes.Both, owner);
			CreatePivot(productNoMatchPivot, ClassificationTypeList.Codes.HTB, owner, "11111111");
			CreatePivot(productNoMatchPivot, ClassificationTypeList.Codes.HTB, orgWithSameRootCNPJ, "11111111");
			CreatePivot(productNoMatchPivot, ClassificationTypeList.Codes.HTB, orgWithOtherRootCNPJ, "05235345");

			var part_OWN1 = CreateProudct("MATCH_PROD", OrgPartRelation.RelationshipTypes.Owner, owner);
			var pivot_OWN1_HTB1 = CreatePivot(part_OWN1, ClassificationTypeList.Codes.HTB, owner, "05235345");
			var pivot_OWN1_HTI2 = CreatePivot(part_OWN1, ClassificationTypeList.Codes.HTI, orgWithSameRootCNPJ, "05235345");
			var pivot_OWN1_HTE2 = CreatePivot(part_OWN1, ClassificationTypeList.Codes.HTE, orgWithSameRootCNPJ, "05235345");

			var part_SUP1 = CreateProudct("MATCH_PROD", OrgPartRelation.RelationshipTypes.Supplier, owner);
			var pivot_SUP1_HTB1 = CreatePivot(part_SUP1, ClassificationTypeList.Codes.HTB, owner, "05235345");
			var pivot_SUP1_HTI2 = CreatePivot(part_SUP1, ClassificationTypeList.Codes.HTI, orgWithSameRootCNPJ, "05235345");
			var pivot_SUP1_HTE2 = CreatePivot(part_SUP1, ClassificationTypeList.Codes.HTE, orgWithSameRootCNPJ, "05235345");

			var part_BTH2 = CreateProudct("MATCH_PROD", OrgPartRelation.RelationshipTypes.Both, orgWithSameRootCNPJ);
			var pivot_BTH2_HTB1 = CreatePivot(part_BTH2, ClassificationTypeList.Codes.HTB, owner, "05235345");
			var pivot_BTH2_HTB2 = CreatePivot(part_BTH2, ClassificationTypeList.Codes.HTB, orgWithSameRootCNPJ, "11111111");
			var pivot_BTH2_HTB3 = CreatePivot(part_BTH2, ClassificationTypeList.Codes.HTB, orgWithOtherRootCNPJ, "05235345");

			var part_BTH3 = CreateProudct("MATCH_PROD", OrgPartRelation.RelationshipTypes.Both, orgWithOtherRootCNPJ);
			var pivot_BTH3_HTB1 = CreatePivot(part_BTH3, ClassificationTypeList.Codes.HTB, owner, "05235345");

			var pivot_WithoutOrg = CreatePivot(part_BTH2, ClassificationTypeList.Codes.HTB, null, "05235345");
			var pivot_WithCatalog = CreatePivot(part_BTH2, ClassificationTypeList.Codes.HTB, owner, "");

			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_Tariff = "05235345";
			pivot_WithCatalog.CI_CGC_Catalog = goodsCatalog.PK;

			var localPartNumber = goodsCatalog.LocalPartNumbers.AddNew();
			AssertPivotAndWarningMessage(System.Array.Empty<CusClassPartPivot>(), LocalPartNumberPivotFinder.MustEnterCatalogOwnerAndTypeMessage);

			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			AssertPivotAndWarningMessage(System.Array.Empty<CusClassPartPivot>(), LocalPartNumberPivotFinder.MustEnterCatalogOwnerAndTypeMessage);

			goodsCatalog.CGC_OH_Owner = orgWithSameRootCNPJ.PK;
			AssertPivotAndWarningMessage(System.Array.Empty<CusClassPartPivot>(), LocalPartNumberPivotFinder.CatalogOwnerDoesNotHaveRootCnpjMessage);

			goodsCatalog.CGC_OH_Owner = owner.PK;
			localPartNumber.CGI_Reference = "ERR_PROD";
			AssertPivotAndWarningMessage(System.Array.Empty<CusClassPartPivot>(), LocalPartNumberPivotFinder.NoProductsFoundMessage);

			localPartNumber.CGI_Reference = "INACTIVE_PROD";
			AssertPivotAndWarningMessage(System.Array.Empty<CusClassPartPivot>(), LocalPartNumberPivotFinder.InactiveOrNoProductMatchsOwnerMessage);

			localPartNumber.CGI_Reference = "NOMATCHORG_PROD";
			AssertPivotAndWarningMessage(System.Array.Empty<CusClassPartPivot>(), LocalPartNumberPivotFinder.InactiveOrNoProductMatchsOwnerMessage);

			localPartNumber.CGI_Reference = "NOMATCHPIVOT_PROD";
			AssertPivotAndWarningMessage(System.Array.Empty<CusClassPartPivot>(), LocalPartNumberPivotFinder.GetNoMatchingClassificationMessage("NOMATCHPIVOT_PROD", "05235345", "OWN"));

			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			localPartNumber.CGI_Reference = "NOMATCHPIVOT_PROD";
			AssertPivotAndWarningMessage(System.Array.Empty<CusClassPartPivot>(), LocalPartNumberPivotFinder.GetNoMatchingClassificationMessage("NOMATCHPIVOT_PROD", "05235345", "OWN"));

			localPartNumber.CGI_Reference = "MATCH_PROD";
			AssertPivotAndWarningMessage(new CusClassPartPivot[] { pivot_SUP1_HTB1, pivot_SUP1_HTE2, pivot_BTH2_HTB1, pivot_WithoutOrg, pivot_WithCatalog }, ZString.Empty);

			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			AssertPivotAndWarningMessage(new CusClassPartPivot[] { pivot_OWN1_HTB1, pivot_OWN1_HTI2, pivot_BTH2_HTB1, pivot_WithoutOrg, pivot_WithCatalog }, ZString.Empty);

			void AssertPivotAndWarningMessage(CusClassPartPivot[] expectedPivots, string expectedMessage)
			{
				CombineAssertions($"PartNumber: {localPartNumber.CGI_Reference}, Type: {goodsCatalog.CGC_Type}",  () =>
				{
					var matchingPivots = localPartNumber.PivotFinder.MatchingPivots;
					AssertContainsExactElementsInAnyOrder(expectedPivots, matchingPivots);
					AssertEquals(expectedMessage, localPartNumber.PivotFinder.WarningMessage);
				});
			}
		}

		public static OrgSupplierPart CreateProudct(string partNum, string relationship, OrgHeader org, bool isActive = true)
		{
			var part = org.Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partNum;
			part.OP_IsActive = isActive;
			var relatedOrg = part.RelatedOrganisations.AddNew();
			relatedOrg.OU_Relationship = relationship;
			relatedOrg.OU_OH = org.PK;
			return part;
		}

		public static CusClassPartPivot CreatePivot(OrgSupplierPart part, string childType, OrgHeader org, string tariffCode)
		{
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = childType;
			pivot.CI_TariffNum = tariffCode;
			pivot.CI_OH = org?.PK ?? ZGuid.Empty;
			return pivot;
		}
	}
}
