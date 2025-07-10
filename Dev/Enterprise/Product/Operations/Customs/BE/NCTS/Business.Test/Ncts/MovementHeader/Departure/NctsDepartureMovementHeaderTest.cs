using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureMovementHeader))]
sealed class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestMessages()
	{
		var messages = DepartureMovement.Messages;
		AssertEquals(DepartureMovement, messages.Master);
	}

	public void TestHeader()
	{
		AssertType<NctsHeader>(DepartureMovement.Header);
	}

	public void TestInlandTransports()
	{
		AssertType<InlandTransportCollection>(DepartureMovement.InlandTransports);
	}

	public void TestGetCusCodeDataTypes()
	{
		AssertEquals(typeof(InlandTransport), DepartureMovement.GetCusCodeDataTypes()[Constants.CusCodeDataTypes.TransportInland]);
	}

	public void TestGetFetchStrategies()
	{
		AssertType<Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy>(DepartureMovement.GetFetchStrategies().Single());
	}

	public void TestInlandTransportLineNumberGenerator()
	{
		CombineAssertions(() =>
		{
			var inlandTransport1 = DepartureMovement.InlandTransports.AddNew();
			AssertEquals("Line 1", (ZShort)1, inlandTransport1.CY_Order);
			var inlandTransport2 = DepartureMovement.InlandTransports.AddNew();
			AssertEquals("Line 2", (ZShort)2, inlandTransport2.CY_Order);
			var inlandTransport3 = DepartureMovement.InlandTransports.AddNew();
			AssertEquals("Line 3", (ZShort)3, inlandTransport3.CY_Order);
			DepartureMovement.InlandTransports.RemoveAndDelete(inlandTransport2);
			AssertEquals("Renumbered 3 to 2", (ZShort)2, inlandTransport3.CY_Order);
		});
	}

	public void TestGoodsLocation()
	{
		AssertType<CusGoodsLocation>(DepartureMovement.GoodsLocation);
	}

	public void TestIsAmendingDeclarationAllowed()
	{
		var availableStatus = typeof(NctsTransitStatusList.Codes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string)).Select(x => (string)x.GetRawConstantValue()).ToList();
		var allowedStatus = new List<string>() { NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid };

		CombineAssertions(() =>
		{
			foreach (var status in availableStatus)
			{
				DepartureMovement.BM_CustomsStatus = status;
				AssertEquals($"Checking Customs Status '{status}'", allowedStatus.Contains(status), DepartureMovement.IsAmendingDeclarationAllowed);
			}
		});
	}

	public void TestBM_AdditionalText_MaxLength()
	{
		AssertEquals(500, DepartureMovement.BM_AdditionalTextInfo.MaxLength);
	}

	public void TestEnquiryCustomsOffice()
	{
		AssertNotNull(DepartureMovement.EnquiryCustomsOffice);
	}

	public void TestLookups()
	{
		AssertType<NctsDepartureMovementHeaderLookups>(DepartureMovement.Lookups);
	}

	public void TestValidation()
	{
		AssertType<NctsDepartureMovementHeaderValidation>(DepartureMovement.Validation);
	}

	public void TestGuarantees()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<NctsGuaranteeCollection<NctsGuarantee>>(nctsHeader.MovementHeader.Guarantees);
	}

	public void TestDefaultDepartureLocationCodeFromCusAuthorisationIfBlank()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		departureMovement.IsSimplifiedNctsProcedure = true;

		var header = departureMovement.Header;
		header.Principal.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;

		var configuration = header.Configuration.LocationOfGoodsFromAuthorisationDefaulterConfiguration;
		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Configuration IsDefaultingEnabled", expected: true, configuration.IsDefaultingEnabled);
			AssertEquals("Pre-requisite: Configuration QualifierCode", CusGoodsLocationQualifierList.Codes.UnLocode, configuration.QualifierCode);
			AssertEquals("Pre-requisite: Configuration TypeCode", CusGoodsLocationTypeList.Codes.ApprovedPlace, configuration.TypeCode);
		});

		var cusAuthorisationUsage = departureMovement.CusAuthorizationUsages.AddNew();
		cusAuthorisationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		cusAuthorisationUsage.AGC_Number = "AR10001";
		cusAuthorisationUsage.AGC_OH_Owner = GlbCompany.CurrentCompany.OrgProxy.PK;

		var cusAuthorisationHeader = Factory.New<CusAuthorisationHeader>();
		cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		cusAuthorisationHeader.CPH_Number = "AR10001";
		cusAuthorisationHeader.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;

		var goodsLocation = departureMovement.GoodsLocation;

		var rule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		rule.CPR_ValueFrom = "A000";
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, configuration.QualifierCode, configuration.TypeCode, rule.CPR_ValueFrom);

		ClearDepartureGoodsLocation(goodsLocation);
		cusAuthorisationHeader.CusAuthorisationRules.DeleteAll();

		var rule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule1.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		rule1.CPR_ValueFrom = "BE000001";
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, CusGoodsLocationTypeList.Codes.DesignatedLocation, rule1.CPR_ValueFrom, isCustomsOffice: true);

		ClearDepartureGoodsLocation(goodsLocation);

		var rule2 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		rule2.CPR_ValueFrom = "BE000002";
		_ = NCTSTestHelper.CreateLinkedAuthorisationRuleForTest(rule2, LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "BE000002");
		departureMovement.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture).CY_Data = "BE000002";
		AssertEquals("Departure Customs Office", "BE000002", departureMovement.DepartureCustomsOffice.OfficeCode);
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, CusGoodsLocationTypeList.Codes.DesignatedLocation, rule2.CPR_ValueFrom, isCustomsOffice: true);

		ClearDepartureGoodsLocation(goodsLocation);

		var rule3 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule3.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		rule3.CPR_ValueFrom = "A001";
		_ = NCTSTestHelper.CreateLinkedAuthorisationRuleForTest(rule3, LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "BE000002");
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, ZString.Empty, ZString.Empty, ZString.Empty);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)departureMovement).GetCusSupportingInfoTypes();
		CombineAssertions(() =>
		{
			AssertEquals("#CusSupportingInfoTypes", 1, cusSupportingInfoTypes.Count);
			AssertEquals("SUP", typeof(NctsSupportingDocument), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		});
	}

	public void TestRepresentativeReadOnly()
	{
		var departureMovement = GetNewBusinessObject(Factory);
		var header = departureMovement.Header;

		CombineAssertions(() =>
		{
			AssertEquals("Representative should not be not readonly when there are no additional documents", false, header.MovementHeader.Representative.ReadOnly);

			var additionalDocument1 = header.AdditionalDocuments.AddNew();
			additionalDocument1.CSI_Code = "4009";
			additionalDocument1.CSI_SubType = "XYZ";

			AssertEquals("Representative should not be readonly when the NctsHeader does not have an additional document with code 4009, Type OTH and SubType REF (1)", false, header.MovementHeader.Representative.ReadOnly);

			var additionalDocument2 = header.AdditionalDocuments.AddNew();
			additionalDocument2.CSI_Code = "1000";
			additionalDocument2.CSI_SubType = "REF";

			AssertEquals("Representative should not be readonly when the NctsHeader does not have an additional document with code 4009, Type OTH and SubType REF (2)", false, header.MovementHeader.Representative.ReadOnly);

			var additionalDocument3 = header.AdditionalDocuments.AddNew();
			additionalDocument3.CSI_Code = "4009";
			additionalDocument3.CSI_SubType = "REF";

			AssertEquals("Representative should be readonly when the NctsHeader has an additional document with code 4009, Type OTH and SubType REF", true, header.MovementHeader.Representative.ReadOnly);
		});
	}

	void ClearDepartureGoodsLocation(CusGoodsLocation goodsLocation)
	{
		goodsLocation.CGL_Qualifier = ZString.Empty;
		goodsLocation.CGL_Type = ZString.Empty;
		goodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
		goodsLocation.CGL_CustomsOffice = ZString.Empty;
	}

	void AssertDepartureGoodsLocation(CusGoodsLocation goodsLocation, ZString qualifier, ZString type, ZString code, bool isCustomsOffice = false)
	{
		CombineAssertions(() =>
		{
			AssertEquals("Goods Location Qualifier", qualifier, goodsLocation.CGL_Qualifier);
			AssertEquals("Goods Location Type", type, goodsLocation.CGL_Type);
			if (isCustomsOffice)
			{
				AssertEquals("Goods Location Customs Office", code, goodsLocation.CGL_CustomsOffice);
			}
			else
			{
				AssertEquals("Goods Location UNLOCODE", code, goodsLocation.CGL_AdditionalIdentifier);
			}

			var displayText = new ZStringBuilder().AppendIfNotEmpty(qualifier).AppendIfNotEmpty(type).AppendIfNotEmpty(code).ToStringWithDelimiterBetweenAppends(";").TrimEnd(';');
			AssertEquals("Goods Location Display Text", displayText, goodsLocation.DisplayText);
		});
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
	{
		var result = GetNewBusinessObject(Factory);

		return result;
	}

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
	{
		return new LightValidationTesterExcludingJobDocAddress(bizObjToTest);
	}

	NctsDepartureMovementHeader DepartureMovement => departureMovement ?? (departureMovement = GetNewBusinessObject(Factory));
	NctsDepartureMovementHeader departureMovement;

	NctsDepartureMovementHeader GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader;
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
