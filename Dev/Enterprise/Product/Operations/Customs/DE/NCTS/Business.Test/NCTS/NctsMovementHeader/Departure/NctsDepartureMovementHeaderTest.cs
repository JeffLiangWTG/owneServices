using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureMovementHeader))]
	sealed class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMessages()
		{
			var messages = departureMovement.Messages;
			AssertEquals(departureMovement, messages.Master);
		}

		public void TestLookups()
		{
			AssertType<NctsDepartureMovementHeaderLookups>(departureMovement.Lookups);
		}

		public void TestValidation()
		{
			AssertType<NctsDepartureMovementHeaderValidation>(departureMovement.Validation);
		}

		public void TestBM_BTAIndicator()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Read Only", false, departureMovement.BM_PlaceOfUnloadingInfo.ReadOnly);
				departureMovement.BM_PlaceOfUnloading = Core.Constants.CountryCodes.France;
				departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies;
				AssertEquals("Place of Loading not cleared", Core.Constants.CountryCodes.France, departureMovement.BM_PlaceOfUnloading);

				departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
				AssertEquals("Read Only", true, departureMovement.BM_PlaceOfUnloadingInfo.ReadOnly);
				AssertEquals("Place of Loading cleared", ZString.Empty, departureMovement.BM_PlaceOfUnloading);
			});
		}

		public void TestBM_PlaceOfUnloading()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not Readonly", false, departureMovement.BM_PlaceOfUnloadingInfo.ReadOnly);
				departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
				AssertEquals("Readonly", true, departureMovement.BM_PlaceOfUnloadingInfo.ReadOnly);
			});
		}

		public void TestCusGoodsLocation()
		{
			AssertType<CusGoodsLocation>(departureMovement.GoodsLocation);
		}

		public void TestClearGoodsLocationWhenIsSimplifiedNctsProcedure()
		{
			departureMovement.IsSimplifiedNctsProcedure = true;
			var goodsLocation = departureMovement.GoodsLocation;
			goodsLocation.CGL_AdditionalIdentifier = "ABC";
			goodsLocation.Address.E2_Contact = "Yuan";
			goodsLocation.Address.E2_Phone = "57234500";
			AssertEquals("Y;ABC;Contact Yuan;Ph 57234500", departureMovement.GoodsLocationDescription);
			departureMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals(ZString.Empty, departureMovement.GoodsLocationDescription);
		}

		public void TestTransportAtDeparture_CharacterCasing()
		{
			departureMovement.TransportTypeAtDeparture = "21";
			departureMovement.TransportAtDeparture = "Abc";
			AssertEquals("Transport At Departure converted to Uppercase", "ABC", departureMovement.TransportAtDeparture);
		}

		public void TestVesselNameAtDeparture_CharacterCasing()
		{
			departureMovement.TransportTypeAtDeparture = "10";
			departureMovement.VesselNameAtDeparture = "Abc";
			AssertEquals("Vessel Name At Departure converted to Uppercase", "ABC", departureMovement.VesselNameAtDeparture);
		}

		public void TestCustomsOfficesForDeparture_ListChanged_DefaultCGL_AdditionalIdentifier()
		{
			departureMovement.IsSimplifiedNctsProcedure = true;

			var cusAuthorisationUsage = departureMovement.CusAuthorizationUsages.AddNew();
			cusAuthorisationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			cusAuthorisationUsage.AGC_Number = "ACR0001";
			cusAuthorisationUsage.AGC_OH_Owner = GlbCompany.CurrentCompany.OrgProxy.PK;

			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			cusAuthorisationHeader.CPH_Number = "ACR0001";
			cusAuthorisationHeader.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;

			var rule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule1.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			rule1.CPR_ValueFrom = "B001";
			rule1.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000001");

			CombineAssertions(() =>
			{
				AssertEquals("Goods Location is empty", ZString.Empty, departureMovement.GoodsLocationDescription);

				var customsOffice = departureMovement.CustomsOfficesForDeparture.Where(x => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture).FirstOrDefault();
				customsOffice.CY_Data = "DE000001";
				AssertEquals("AdditionalIdentifierList count is 1", "Y;B001", departureMovement.GoodsLocationDescription);

				departureMovement.IsSimplifiedNctsProcedure = false;
				AssertEquals("CGL_AdditionalIdentifier is cleared", ZString.Empty, departureMovement.GoodsLocationDescription);
				customsOffice.CY_Date = ZDateTime.Today;
				AssertEquals("IsSimplifiedNctsProcedure is false", ZString.Empty, departureMovement.GoodsLocationDescription);
			});
		}

		public void TestTransportTypeAtDepartureCapation()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(departureMovement.TransportTypeAtDepartureInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Type of Identification", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Type of ID", captionResourceString.ShortCaption);
			});
		}

		public void TestAddCusGoodsLocationAdditionalIdentifierDescriptionFetchHintsForViewCore()
		{
			var cgl1 = Factory.NewWithValidTestData<CusGoodsLocation>();
			cgl1.CGL_ParentTableCode = CusInBondMoveHeaderSchema.Constants.Prefix;
			cgl1.CGL_ParentID = departureMovement.PK;
			(departureMovement.DepartureCustomsOffice as NctsEuOfficeCode).CY_Data = "OFFIC1";

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.SetMovementType(NctsMovementType.Codes.Departure);
			(nctsHeader2.MovementHeader.DepartureCustomsOffice as NctsEuOfficeCode).CY_Data = "OFFIC2";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var headerCollection = new NctsHeaderCollection(factory);
			headerCollection.Load();
			headerCollection.ForEach(x => x.ReadOnly = true);
			var fetchStrategy = headerCollection.FetchStrategy;
			fetchStrategy.FetchForView(headerCollection.ToArray(), [new TableColumn(string.Empty, nameof(CusGoodsLocation) + "+" + nameof(CusGoodsLocation.AdditionalIdentifierDescription))]);

			_ = headerCollection.Cast<NctsHeader>().Select(x => x.CusGoodsLocation.AdditionalIdentifierDescription).ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Table CusCodeData should have 1 hit", 1, factory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));
				AssertEquals("Table CusAuthorizationUsage should have 1 hit", 1, factory.GetTableHitCount(CusAuthorizationUsageSchema.Constants.TableName));
			});
		}

		public void TestGuarantees() => AssertType<NctsGuaranteeCollection<Guarantee>>(nctsHeader.MovementHeader.Guarantees);

		public void TestGuaranteeTransactionCoordinator() => AssertType<GuaranteeTransactionCoordinator>(departureMovement.GuaranteeTransactionCoordinator);

		//public void TestDefaultDepartureLocationCodeFromCusAuthorisationIfBlank()
		//{
		//	using (NctsCustomsDataRegistry.Instance.EnableDefaultingPhase5AuthorisedLocationFromACRAuthorisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		//	{
		//		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		//		departureMovement.Header.Principal.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
		//		departureMovement.IsSimplifiedNctsProcedure = true;
		//		var goodsLocation = departureMovement.GoodsLocation;

		//		var cusAuthorisationUsage = departureMovement.CusAuthorizationUsages.AddNew();
		//		cusAuthorisationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		//		cusAuthorisationUsage.AGC_Number = "AR10001";
		//		cusAuthorisationUsage.AGC_OH_Owner = GlbCompany.CurrentCompany.OrgProxy.PK;

		//		var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		//		cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		//		cusAuthorisationHeader.CPH_Number = "AR10001";
		//		cusAuthorisationHeader.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;

		//		var rule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		//		rule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		//		rule.CPR_ValueFrom = "A001";
		//		rule.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000001");

		//		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		//		AssertEquals("Goods Location Qualifier", CusGoodsLocationQualifierList.Codes.AuthorizationNumber, goodsLocation.CGL_Qualifier);
		//		AssertEquals("Goods Location Additional Identifier", rule.CPR_ValueFrom, goodsLocation.CGL_AdditionalIdentifier);

		//		var rule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		//		rule1.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		//		rule1.CPR_ValueFrom = "A002";
		//		rule1.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000002");

		//		goodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
		//		departureMovement.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture).CY_Data = "DE000002";
		//		AssertEquals("Departure Customs Office", "DE000002", departureMovement.DepartureCustomsOffice.OfficeCode);

		//		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		//		AssertEquals("Goods Location Qualifier", CusGoodsLocationQualifierList.Codes.AuthorizationNumber, goodsLocation.CGL_Qualifier);
		//		AssertEquals("Goods Location Additional Identifier", rule1.CPR_ValueFrom, goodsLocation.CGL_AdditionalIdentifier);
		//	}
		//}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => departureMovement;

		protected override BusinessObject GetNewBusinessObject() => departureMovement;

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new LightValidationTesterExcludingJobDocAddress(bizObjToTest);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
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
