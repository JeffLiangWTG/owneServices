using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<CusGoodsLocationLookups>(cusGoodsLocation.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusGoodsLocationValidation>(cusGoodsLocation.Validation);
		}

		public void TestCGL_AdditionalIdentifier_MaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CGL_Qualifier isn't U", 4, cusGoodsLocation.CGL_AdditionalIdentifierInfo.MaxLength);

				cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				AssertEquals("CGL_Qualifier is U", 5, cusGoodsLocation.CGL_AdditionalIdentifierInfo.MaxLength);
			});
		}

		public void TestCGL_AdditionalIdentifier_ExceedMaxLength()
		{
			cusGoodsLocation.CGL_AdditionalIdentifier = "12345";
			AssertEquals("1234", cusGoodsLocation.CGL_AdditionalIdentifier);
		}

		public void TestCGL_Qualifier_ReadOnly()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
			var incident = nctsHeader.EnRouteIncidents.AddNew();
			var cusGoodsLocation = incident.GoodsLocation;
			AssertEquals(false, cusGoodsLocation.CGL_QualifierInfo.ReadOnly);
		}

		public void TestCGL_Qualifier_ReadOnly_ParentIsArrivalMovementHeader()
		{
			AssertEquals(true, cusGoodsLocation.CGL_QualifierInfo.ReadOnly);
		}

		public void TestCGL_Qualifier_ReadOnly_ParentIsDepartureMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var cusGoodsLocation = nctsHeader.MovementHeader.GoodsLocation;
			AssertEquals(true, cusGoodsLocation.CGL_QualifierInfo.ReadOnly);
		}

		public void TestAddressIdentificationHolderPK_ReadOnly()
		{
			AssertEquals(true, cusGoodsLocation.AddressIdentificationHolderPKInfo.ReadOnly);
		}

		public void TestAddressAuthorisationNumber_ReadOnly()
		{
			AssertEquals(true, cusGoodsLocation.AddressAuthorisationNumberInfo.ReadOnly);
		}

		public void TestSetDefaultsForNew()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Parent is MovementHeader", CusGoodsLocationQualifierList.Codes.AuthorizationNumber, cusGoodsLocation.CGL_Qualifier);

				var incidentGoodsLocation = nctsHeader.EnRouteIncidents.AddNew().GoodsLocation;
				AssertEquals("Parent isn't MovementHeader", ZString.Empty, incidentGoodsLocation.CGL_Qualifier);
			});
		}

		public void TestAdditionalIdentifierDescription()
		{
			const string description = "Identifier Description";
			const string code = "T001";
			const string officeCode = "DE010101";

			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			cusAuthorisationHeader.CPH_OH_PermitHolder = orgHeader.PK;
			cusAuthorisationHeader.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;

			var rule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			rule.CPR_ValueFrom = code;
			rule.CPR_Description = description;
			rule.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, officeCode);

			Factory.Save();

			var arrivalMovement = nctsHeader.ArrivalMovementHeader;
			arrivalMovement.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			arrivalMovement.DestinationCustomsOfficeCodeForArrival = officeCode;
			cusGoodsLocation.AddressIdentificationHolderPK = cusAuthorisationHeader.CPH_OH_PermitHolder;
			cusGoodsLocation.AddressAuthorisationNumber = cusAuthorisationHeader.CPH_Number;

			cusGoodsLocation.CGL_AdditionalIdentifier = code;

			CombineAssertions(() =>
			{
				AssertEquals("Description matches on valid additional identifier", description, cusGoodsLocation.AdditionalIdentifierDescription);
				cusGoodsLocation.CGL_AdditionalIdentifier = "InvalidCode";
				AssertEquals("Description is empty on invalid additional identifier", string.Empty, cusGoodsLocation.AdditionalIdentifierDescription);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			return cusGoodsLocation;
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			cusGoodsLocation = nctsHeader.ArrivalMovementHeader.GoodsLocation;
		}

		CusGoodsLocation cusGoodsLocation;
		NctsHeader nctsHeader;
	}
}
