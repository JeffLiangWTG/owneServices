using System.Linq;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class BLSendChileWrapperParticipationTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		OrgHeader commonOrgHeader;

		public void TestBLSendChileWrapperParticipationChilean()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_OA_Carrier = FillOrgAddress("Carrier", "500", Core.Constants.CountryCodes.Chile).PK;

			CreateAndPopulateHouseBill(Core.Constants.CountryCodes.Chile);

			IBLRequest wrapper = new BLSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A);

			IParticipationDocument participation1 = wrapper.ParticipationDocuments.ElementAt(0);
			IParticipationDocument participation2 = wrapper.ParticipationDocuments.ElementAt(1);
			IParticipationDocument participation3 = wrapper.ParticipationDocuments.ElementAt(2);
			IParticipationDocument participation4 = wrapper.ParticipationDocuments.ElementAt(3);
			IParticipationDocument participation5 = wrapper.ParticipationDocuments.ElementAt(4);
			IParticipationDocument participation6 = wrapper.ParticipationDocuments.ElementAt(5);
			IParticipationDocument participation7 = wrapper.ParticipationDocuments.ElementAt(6);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(WrappersConstants.ParticipationName.Alm, participation1.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation1.IDType);
				AssertEquals("211110890017", participation1.IDValue);
				AssertEquals("GLName", participation1.Names);
				AssertEquals(Core.Constants.CountryCodes.Chile, participation1.CountryID);
				AssertEquals("211110890016", participation1.WarehouseCode);
				AssertEquals(ZString.Empty, participation1.Address);
				AssertEquals(ZString.Empty, participation1.Phone);
				AssertEquals(ZString.Empty, participation1.Email);

				AssertEquals(WrappersConstants.ParticipationName.Emi, participation2.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation2.IDType);
				AssertEquals("92048000-4", participation2.IDValue);
				AssertEquals("Company", participation2.Names);
				AssertEquals(Core.Constants.CountryCodes.Chile, participation2.CountryID);
				AssertEquals(ZString.Empty, participation2.WarehouseCode);
				AssertEquals(ZString.Empty, participation2.Address);
				AssertEquals(ZString.Empty, participation2.Phone);
				AssertEquals(ZString.Empty, participation2.Email);

				AssertEquals(WrappersConstants.ParticipationName.Rep, participation3.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation3.IDType);
				AssertEquals("92048000-4", participation3.IDValue);
				AssertEquals("Company", participation3.Names);
				AssertEquals(Core.Constants.CountryCodes.Chile, participation3.CountryID);
				AssertEquals(ZString.Empty, participation3.WarehouseCode);
				AssertEquals(ZString.Empty, participation3.Address);
				AssertEquals(ZString.Empty, participation3.Phone);
				AssertEquals(ZString.Empty, participation3.Email);

				AssertEquals(WrappersConstants.ParticipationName.Emido, participation4.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation4.IDType);
				AssertEquals("211110890018", participation4.IDValue);
				AssertEquals("MEDITERRANEAN SHIPPING COMPANY S.A.", participation4.Names);
				AssertEquals(Core.Constants.CountryCodes.Chile, participation4.CountryID);
				AssertEquals(ZString.Empty, participation4.WarehouseCode);
				AssertEquals(ZString.Empty, participation4.Address);
				AssertEquals(ZString.Empty, participation4.Phone);
				AssertEquals(ZString.Empty, participation4.Email);

				AssertEquals(WrappersConstants.ParticipationName.Emb, participation5.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation5.IDType);
				AssertEquals("300", participation5.IDValue);
				AssertEquals("Shipper", participation5.Names);
				AssertEquals(Core.Constants.CountryCodes.Chile, participation5.CountryID);
				AssertEquals(ZString.Empty, participation5.WarehouseCode);
				AssertEquals("ShipperAddress1 ShipperCity 300", participation5.Address);
				AssertEquals("ShipperPhone", participation5.Phone);
				AssertEquals("Shipper@wisetechglobal.com", participation5.Email);

				AssertEquals(WrappersConstants.ParticipationName.Cons, participation6.Name);
				AssertEquals(OrgCusCode.CodeTypes.PassportID, participation6.IDType);
				AssertEquals("100", participation6.IDValue);
				AssertEquals("Consignee", participation6.Names);
				AssertEquals(Core.Constants.CountryCodes.Chile, participation6.CountryID);
				AssertEquals(ZString.Empty, participation6.WarehouseCode);
				AssertEquals("ConsigneeAddress1 ConsigneeCity 100", participation6.Address);
				AssertEquals("ConsigneePhone", participation6.Phone);
				AssertEquals("Consignee@wisetechglobal.com", participation6.Email);

				AssertEquals(WrappersConstants.ParticipationName.Noti, participation7.Name);
				AssertEquals(OrgCusCode.CodeTypes.PassportID, participation7.IDType);
				AssertEquals("200", participation7.IDValue);
				AssertEquals("NotifyParty", participation7.Names);
				AssertEquals(Core.Constants.CountryCodes.Chile, participation7.CountryID);
				AssertEquals(ZString.Empty, participation7.WarehouseCode);
				AssertEquals("NotifyPartyAddress1 NotifyPartyCity 200", participation7.Address);
				AssertEquals("NotifyPartyPhone", participation7.Phone);
				AssertEquals("NotifyParty@wisetechglobal.com", participation7.Email);
			});
		}

		public void TestBLSendChileWrapperParticipationForeign()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_OA_Carrier = FillOrgAddress("Carrier", "500", Core.Constants.CountryCodes.Uruguay).PK;

			CreateAndPopulateHouseBill(Core.Constants.CountryCodes.Uruguay);

			IBLRequest wrapper = new BLSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A);

			IParticipationDocument participation4 = wrapper.ParticipationDocuments.ElementAt(3);
			IParticipationDocument participation5 = wrapper.ParticipationDocuments.ElementAt(4);
			IParticipationDocument participation6 = wrapper.ParticipationDocuments.ElementAt(5);
			IParticipationDocument participation7 = wrapper.ParticipationDocuments.ElementAt(6);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(WrappersConstants.ParticipationName.Emido, participation4.Name);
				AssertEquals(ZString.Empty, participation4.IDType);
				AssertEquals(ZString.Empty, participation4.IDValue);
				AssertEquals("MEDITERRANEAN SHIPPING COMPANY S.A.", participation4.Names);
				AssertEquals(Core.Constants.CountryCodes.Uruguay, participation4.CountryID);
				AssertEquals(ZString.Empty, participation4.WarehouseCode);
				AssertEquals(ZString.Empty, participation4.Address);
				AssertEquals(ZString.Empty, participation4.Phone);
				AssertEquals(ZString.Empty, participation4.Email);

				AssertEquals(WrappersConstants.ParticipationName.Emb, participation5.Name);
				AssertEquals(ZString.Empty, participation5.IDType);
				AssertEquals(ZString.Empty, participation5.IDValue);
				AssertEquals("Shipper", participation5.Names);
				AssertEquals(Core.Constants.CountryCodes.Uruguay, participation5.CountryID);
				AssertEquals(ZString.Empty, participation5.WarehouseCode);
				AssertEquals("ShipperAddress1 ShipperCity 300", participation5.Address);
				AssertEquals("ShipperPhone", participation5.Phone);
				AssertEquals("Shipper@wisetechglobal.com", participation5.Email);

				AssertEquals(WrappersConstants.ParticipationName.Cons, participation6.Name);
				AssertEquals(ZString.Empty, participation6.IDType);
				AssertEquals(ZString.Empty, participation6.IDValue);
				AssertEquals("Consignee", participation6.Names);
				AssertEquals(Core.Constants.CountryCodes.Uruguay, participation6.CountryID);
				AssertEquals(ZString.Empty, participation6.WarehouseCode);
				AssertEquals("ConsigneeAddress1 ConsigneeCity 100", participation6.Address);
				AssertEquals("ConsigneePhone", participation6.Phone);
				AssertEquals("Consignee@wisetechglobal.com", participation6.Email);

				AssertEquals(WrappersConstants.ParticipationName.Noti, participation7.Name);
				AssertEquals(ZString.Empty, participation7.IDType);
				AssertEquals(ZString.Empty, participation7.IDValue);
				AssertEquals("NotifyParty", participation7.Names);
				AssertEquals(Core.Constants.CountryCodes.Uruguay, participation7.CountryID);
				AssertEquals(ZString.Empty, participation7.WarehouseCode);
				AssertEquals("NotifyPartyAddress1 NotifyPartyCity 200", participation7.Address);
				AssertEquals("NotifyPartyPhone", participation7.Phone);
				AssertEquals("NotifyParty@wisetechglobal.com", participation7.Email);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Chile);

			GlbCompany.CurrentCompany.GC_Address1 = "CompanyAddress1";
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "92048000-4";
			GlbCompany.CurrentCompany.GC_City = "CompanyCity";
			GlbCompany.CurrentCompany.GC_Name = "Company";
			GlbCompany.CurrentCompany.GC_Phone = "CompanyPhone";
			GlbCompany.CurrentCompany.Postcode = "1996";

			GlbStaff.CurrentUser.GS_EmailAddress = "User@wisetechglobal.com";

			commonOrgHeader = Factory.New<OrgHeader>();
			commonOrgHeader.OH_Code = "95";
			commonOrgHeader.OH_FullName = "MEDITERRANEAN SHIPPING COMPANY S.A.";
			commonOrgHeader.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "211110890018");
		}

		void CreateAndPopulateHouseBill(ZString countryCode)
		{
			AsycudaBill bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_GrossWeight = 127.000m;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_ManifestQty = 1;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_RL_NKFinalDestination = "CLSCL";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RoRo = true;
			bill.ABL_Volume = 1000m;
			bill.ABL_VolumeUQ = "M3";

			commonOrgHeader = Factory.New<OrgHeader>();
			commonOrgHeader.OH_Code = "96";
			commonOrgHeader.OH_FullName = "GLName";
			commonOrgHeader.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "211110890017", Core.Constants.CountryCodes.Chile);
			commonOrgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "211110890016", Core.Constants.CountryCodes.Chile);

			bill.ABL_OA_GoodsLocation = commonOrgHeader.MainAddress.PK;

			bill.ABL_OA_Consignee = FillOrgAddress("Consignee", "100", countryCode).PK;
			bill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.PassportID;
			bill.ABL_ConsigneeRegNo = "100";
			bill.ABL_OA_NotifyParty = FillOrgAddress("NotifyParty", "200", countryCode).PK;
			bill.ABL_NotifyPartyRegNoType = OrgCusCode.CodeTypes.PassportID;
			bill.ABL_NotifyPartyRegNo = "200";
			bill.ABL_OA_Shipper = FillOrgAddress("Shipper", "300", countryCode).PK;
			bill.ABL_ShipperRegNo = "300";
		}

		OrgAddress FillOrgAddress(ZString orgType, ZString postcode, string countryCode)
		{
			OrgAddress orgAddress = commonOrgHeader.Addresses.AddNew();
			orgAddress.CompanyName = orgType;
			orgAddress.OA_Address1 = string.Concat(orgType, "Address1");
			orgAddress.OA_City = string.Concat(orgType, "City");
			orgAddress.OA_Email = string.Concat(orgType, "@wisetechglobal.com");
			orgAddress.OA_Phone = string.Concat(orgType, "Phone");
			orgAddress.OA_PostCode = postcode;
			orgAddress.OA_RN_NKCountryCode = countryCode;
			return orgAddress;
		}
	}
}
