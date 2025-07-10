using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class LocalPartNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCGI_Reference()
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

			LocalPartNumberPivotFinderTest.CreateProudct("INACTIVE_PROD", OrgPartRelation.RelationshipTypes.Both, owner, isActive: false);
			LocalPartNumberPivotFinderTest.CreateProudct("NOMATCHORG_PROD", OrgPartRelation.RelationshipTypes.Both, orgWithOtherRootCNPJ);
			LocalPartNumberPivotFinderTest.CreateProudct("NOMATCHPIVOT_PROD", OrgPartRelation.RelationshipTypes.Both, owner);

			var product = LocalPartNumberPivotFinderTest.CreateProudct("MATCH_PROD", OrgPartRelation.RelationshipTypes.Both, owner);
			LocalPartNumberPivotFinderTest.CreatePivot(product, ClassificationTypeList.Codes.HTB, owner, "05235345");

			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_Tariff = "05235345";
			var localPartNumber = goodsCatalog.LocalPartNumbers.AddNew();
			localPartNumber.CGI_Reference = "XXX";
			AssertHasWarning(localPartNumber.CGI_ReferenceInfo, LocalPartNumberPivotFinder.MustEnterCatalogOwnerAndTypeMessage.ToString());

			goodsCatalog.CGC_OH_Owner = orgWithSameRootCNPJ.PK;
			localPartNumber.CGI_Reference = "XXX";
			AssertHasWarning(localPartNumber.CGI_ReferenceInfo, LocalPartNumberPivotFinder.MustEnterCatalogOwnerAndTypeMessage.ToString());

			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			localPartNumber.CGI_Reference = "XXX";
			AssertHasWarning(localPartNumber.CGI_ReferenceInfo, LocalPartNumberPivotFinder.CatalogOwnerDoesNotHaveRootCnpjMessage.ToString());

			goodsCatalog.CGC_OH_Owner = owner.PK;
			localPartNumber.CGI_Reference = "ERR_PROD";
			AssertHasWarning(localPartNumber.CGI_ReferenceInfo, LocalPartNumberPivotFinder.NoProductsFoundMessage.ToString());

			localPartNumber.CGI_Reference = "INACTIVE_PROD";
			AssertHasWarning(localPartNumber.CGI_ReferenceInfo, LocalPartNumberPivotFinder.InactiveOrNoProductMatchsOwnerMessage.ToString());

			localPartNumber.CGI_Reference = "NOMATCHORG_PROD";
			AssertHasWarning(localPartNumber.CGI_ReferenceInfo, LocalPartNumberPivotFinder.InactiveOrNoProductMatchsOwnerMessage.ToString());

			localPartNumber.CGI_Reference = "NOMATCHPIVOT_PROD";
			AssertHasWarning(localPartNumber.CGI_ReferenceInfo, LocalPartNumberPivotFinder.GetNoMatchingClassificationMessage("NOMATCHPIVOT_PROD", "05235345", "OWN").ToString());

			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			localPartNumber.CGI_Reference = "NOMATCHPIVOT_PROD";
			AssertHasWarning(localPartNumber.CGI_ReferenceInfo, LocalPartNumberPivotFinder.GetNoMatchingClassificationMessage("NOMATCHPIVOT_PROD", "05235345", "OWN").ToString());

			localPartNumber.CGI_Reference = "MATCH_PROD";
			AssertNoNotifications(localPartNumber.CGI_ReferenceInfo);

			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			localPartNumber.CGI_Reference = "MATCH_PROD";
			AssertNoNotifications(localPartNumber.CGI_ReferenceInfo);
		}
	}
}
