using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsArrivalMovementHeader))]
	sealed class NctsArrivalMovementHeaderTest : EU.NCTS.Business.Testing.NctsArrivalMovementHeaderAbstractTest
	{
		public void TestNctsArrivalMovementHeaderValidation()
		{
			AssertType<NctsArrivalMovementHeaderValidation>(GetNewBusinessObject(Factory).Validation);
		}

		[TestDate(2024, 09, 18)]
		public void TestSetDefaultValuesAfterNctsHeaderIsSet()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			var header = arrivalMovementHeader.Header;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			CombineAssertions(() =>
			{
				AssertEquals("Default BM_ArrivalDate", ZDateTime.Now, arrivalMovementHeader.BM_ArrivalDate);
				AssertEquals("Default CPH_Type", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, arrivalMovementHeader.AuthorizationCode);
				AssertEquals("Default Header.BH_ExportFlag", YesNoList.Codes.No, header.BH_ExportFlag);
			});
		}

		[TestDate(2025, 02, 05)]
		public void TestDefaultGoodsLocationValues()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var arrivalMovement = nctsHeader.ArrivalMovementHeader;
			arrivalMovement.IsSimplifiedNctsProcedure = true;

			var header = arrivalMovement.Header;
			header.Principal.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			cusAuthorisationHeader.CPH_Number = "AR10001";
			cusAuthorisationHeader.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;

			var cusAuthorisationRule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			cusAuthorisationRule.CPR_ValueFrom = "IEDUB";
			var linkedCusAuthorisationRule = cusAuthorisationRule.LinkedCusAuthorisationRules.AddNew();
			linkedCusAuthorisationRule.CPR_RuleCode = "CUS";
			linkedCusAuthorisationRule.CPR_ValueFrom = "IEDUB200";

			arrivalMovement.AuthorizationOwner = GlbCompany.CurrentCompany.OrgProxy.PK;
			arrivalMovement.AuthorizationCode = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			arrivalMovement.AuthorizationNumber = "AR10001";

			var goodsLocation = arrivalMovement.GoodsLocation;

			CombineAssertions(() =>
			{
				AssertEquals("Default Location of Goods CGL_Qualifier", CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, goodsLocation.CGL_Qualifier);
				AssertEquals("Default Location of Goods CGL_Type", CusGoodsLocationTypeList.Codes.AuthorizedPlace, goodsLocation.CGL_Type);
				AssertEquals("Default Location of Goods CGL_CustomsOffice", "IEDUB200", goodsLocation.CGL_CustomsOffice);
				AssertEquals("Default Location of Goods Description", "V;B;IEDUB200", arrivalMovement.GoodsLocationDescription);
			});
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);
		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new LightValidationTesterExcludingJobDocAddress(bizObjToTest);
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name != "DestinationCustomsOfficeCodeForDeparture"
				&& info.Name != "DestinationCustomsOfficeCodeForArrival")
			{
				base.TestBizObjectField(info);
			}
		}

		NctsArrivalMovementHeader GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader.CusAuthorizationUsages.RemoveAndDeleteAll();
			return nctsHeader.ArrivalMovementHeader;
		}

		class LightValidationTesterExcludingJobDocAddress : LightValidationTester
		{
			public LightValidationTesterExcludingJobDocAddress(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var objectType = info.BizObj.GetType();
				if (objectType == typeof(CusGoodsLocation) || objectType == typeof(CusGoodsLocationAddress))
				{
					return false;
				}
				else
				{
					return base.ShouldTestProperty(info);
				}
			}
		}
	}
}
