using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsArrivalMovementHeader))]
	class NctsArrivalMovementHeaderTest : NctsArrivalMovementHeaderAbstractTest
	{
		public void TestAuthorizationNumberAfterAuthorizationCodeOrAuthorizationOwnerChange()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var cusAuthorizationHeader1 = Factory.New<CusAuthorisationHeader>();
			cusAuthorizationHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorizationHeader1.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
			cusAuthorizationHeader1.CPH_OH_PermitHolder = orgHeader.PK;
			cusAuthorizationHeader1.CPH_Number = "123";
			cusAuthorizationHeader1.CPH_IsActive = true;

			var cusAuthorizationHeader2 = Factory.New<CusAuthorisationHeader>();
			cusAuthorizationHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorizationHeader2.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			cusAuthorizationHeader2.CPH_OH_PermitHolder = orgHeader.PK;
			cusAuthorizationHeader2.CPH_Number = "456";
			cusAuthorizationHeader2.CPH_IsActive = true;

			var cusAuthorizationHeader3 = Factory.New<CusAuthorisationHeader>();
			cusAuthorizationHeader3.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorizationHeader3.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			cusAuthorizationHeader3.CPH_OH_PermitHolder = orgHeader.PK;
			cusAuthorizationHeader3.CPH_Number = "789";
			cusAuthorizationHeader3.CPH_IsActive = true;
			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;

			arrivalMovementHeader.Header.Company.SetCountry(Core.Constants.CountryCodes.France);
			arrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
			arrivalMovementHeader.AuthorizationOwner = orgHeader.PK;
			arrivalMovementHeader.AuthorizationNumber = ZString.Empty;
			arrivalMovementHeader.AuthorizationOwner = orgHeader.PK;
			AssertEquals("Not apply to NCTS Phase4.", ZString.Empty, arrivalMovementHeader.AuthorizationNumber);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalMovementHeader.AuthorizationNumber = ZString.Empty;
			arrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
			arrivalMovementHeader.AuthorizationOwner = ZGuid.Empty;
			arrivalMovementHeader.AuthorizationOwner = orgHeader.PK;
			AssertEquals("AuthorizationOwner changed, only one matching AuthorizationHeader found, set AuthorizationNumber to CPH_Number", "123", arrivalMovementHeader.AuthorizationNumber);

			arrivalMovementHeader.AuthorizationNumber = ZString.Empty;
			arrivalMovementHeader.AuthorizationCode = ZString.Empty;
			arrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
			AssertEquals("AuthorizationCode changed, only one matching AuthorizationHeader found, set AuthorizationNumber to CPH_Number", "123", arrivalMovementHeader.AuthorizationNumber);

			arrivalMovementHeader.AuthorizationNumber = ZString.Empty;
			arrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			AssertEquals("Found multiple authorizations, set AuthorizationNumber to empty.", ZString.Empty, arrivalMovementHeader.AuthorizationNumber);
		}

		public void TestGoodsLocationAfterAuthorizationNumberChange()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;

			arrivalMovementHeader.Header.Company.SetCountry(Core.Constants.CountryCodes.France);
			arrivalMovementHeader.AuthorizationNumber = "123";
			AssertEquals("Not apply to NCTS Phase4.", ZString.Empty, arrivalMovementHeader.GoodsLocation.CGL_Qualifier);
			AssertEquals("Not apply to NCTS Phase4.", ZString.Empty, arrivalMovementHeader.GoodsLocation.CGL_Type);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalMovementHeader.AuthorizationNumber = ZString.Empty;
			AssertEquals("AuthorizationNumber is empty.", ZString.Empty, arrivalMovementHeader.GoodsLocation.CGL_Qualifier);
			AssertEquals("AuthorizationNumber is empty.", ZString.Empty, arrivalMovementHeader.GoodsLocation.CGL_Type);

			arrivalMovementHeader.AuthorizationNumber = "123";
			AssertEquals("When AuthorizationNumber has a value, set CGL_Qualifier in GoodsLocation to 'Y' by default.", CusGoodsLocationQualifierList.Codes.AuthorizationNumber, arrivalMovementHeader.GoodsLocation.CGL_Qualifier);
			AssertEquals("When AuthorizationNumber has a value, set CGL_Type in GoodsLocation to 'B' by default.", CusGoodsLocationTypeList.Codes.AuthorizedPlace, arrivalMovementHeader.GoodsLocation.CGL_Type);
		}

		public void TestIsArrivalDetailsReadOnly()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var arrivalMovement = nctsHeader.ArrivalMovementHeader;
			nctsHeader.EffectiveMessageStatus = FrNctsMessageStatusList.Codes.Unknown;
			AssertEquals("Arrival movement should not be readonly by default.", false, arrivalMovement.IsArrivalDetailsReadOnly);

			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			AssertEquals("Arrival movement should be readonly when message status is Sent to Customs.", true, arrivalMovement.IsArrivalDetailsReadOnly);

			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
			AssertEquals("Arrival movement should be readonly when message status is Acknowledge.", true, arrivalMovement.IsArrivalDetailsReadOnly);

			arrivalMovement.BM_CustomsStatus = ZString.Empty;

			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange.EI_Status = EDIMessageStatusList.Codes.Sent;

			var ediMessage = nctsHeader.Messages.AddNew();
			ediMessage.EM_EI = ediInterchange.PK;
			ediMessage.EM_MessageType = "007";
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.DT;

			AssertEquals("Arrival movement should be readonly when a meessage of type 007 has been sent to Customs.", true, arrivalMovement.IsArrivalDetailsReadOnly);
		}

		public void TestBM_ExpectedNextCustomsProcedure()
		{
			CombineAssertions("Captions should be as expected.", () =>
			{
				var movementHeader = Factory.New<NctsArrivalMovementHeader>();
				AssertEquals("Expected Next Customs procedure", DataBoundResourceStrings.GetDataForProperty(movementHeader.BM_ExpectedNextCustomsProcedureInfo).Caption);
				AssertEquals("Expected Next Customs. Proc.", DataBoundResourceStrings.GetDataForProperty(movementHeader.BM_ExpectedNextCustomsProcedureInfo).MediumCaption);
				AssertEquals("Expected Next Proc.", DataBoundResourceStrings.GetDataForProperty(movementHeader.BM_ExpectedNextCustomsProcedureInfo).ShortCaption);
			});
		}

		public void TestNctsTransitStatusList()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			AssertType<NctsTransitStatusList>(nctsHeader.ArrivalMovementHeader.Lookups.NctsTransitStatusList);
		}

		public void TestLookupsType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			AssertType<NctsArrivalMovementHeaderLookups>(nctsHeader.ArrivalMovementHeader.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			return nctsHeader.ArrivalMovementHeader;
		}

		public void TestGetValueSetStrategy()
		{
			var nctsArrivalMovementHeader = Factory.New<NctsArrivalMovementHeaderForTest>();
			AssertType<NctsArrivalHeaderMovementHeaderValueSetStrategy>("GetValueSetStrategy()", nctsArrivalMovementHeader.GetValueSetStrategyExposed());
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name != "DestinationCustomsOfficeCodeForDeparture"
				&& info.Name != "DestinationCustomsOfficeCodeForArrival")
			{
				base.TestBizObjectField(info);
			}
		}

		public void TestTotalUnloadedNumberOfPackages()
		{
			var bulkType = Factory.SetupBulkCusCode();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill1 = header.Bills.AddNew();

			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			package1.B5_UnitCount = 10;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			package2.B5_UnitType = bulkType;

			var goodsItem2 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 20;

			var goodsItem3 = bill1.ArrivalGoodsItems.AddNew();
			var package4 = goodsItem3.Packages.AddNew();
			package4.B5_UnitCount = 30;

			var bill2 = header.Bills.AddNew();

			var goodsItem4 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			var package5 = goodsItem4.Packages.AddNew();
			package5.B5_UnitCount = 60;

			var goodsItem5 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem5.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var package6 = goodsItem5.Packages.AddNew();
			package6.B5_UnitCount = 10;

			AssertEquals(111, header.ArrivalMovementHeader.TotalUnloadedNumberOfPackages);
		}
	}

	class NctsArrivalMovementHeaderForTest : NctsArrivalMovementHeader
	{
		public NctsArrivalMovementHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IValueSetStrategy GetValueSetStrategyExposed() => base.GetValueSetStrategy();
	}
}
