using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBSendChileWrapperParticipationTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		OrgHeader commonOrgHeader;

		public void TestAWBSendChileWrapperParticipation()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();
			CreateAndPopulateHouseBill();

			IAWBRequest wrapper = new AWBSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Mot);

			IDocParticipations participation1 = wrapper.DocParticipations.ElementAt(0);
			IDocParticipations participation2 = wrapper.DocParticipations.ElementAt(1);
			IDocParticipations participation3 = wrapper.DocParticipations.ElementAt(2);
			IDocParticipations participation4 = wrapper.DocParticipations.ElementAt(3);
			IDocParticipations participation5 = wrapper.DocParticipations.ElementAt(4);
			IDocParticipations participation6 = wrapper.DocParticipations.ElementAt(5);
			IDocParticipations participation7 = wrapper.DocParticipations.ElementAt(6);

			Factory.Save();

			CombineAssertions(() =>
			{
				Assert(wrapper.DocParticipations.IsCountEqualTo(7));

				AssertEquals(WrappersConstants.ParticipationName.Alm, participation1.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation1.IDType);
				AssertEquals("96888200-7", participation1.IDValue);
				AssertEquals("DEPOCARGO LTDA.", participation1.Names);

				AssertEquals(WrappersConstants.ParticipationName.Caer, participation2.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation2.IDType);
				AssertEquals("77491900-7", participation2.IDValue);
				AssertEquals("TRANS AMERICAN AIR LINES S.A.", participation2.Names);

				AssertEquals(WrappersConstants.ParticipationName.Emi, participation3.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation3.IDType);
				AssertEquals("76006380-9", participation3.IDValue);
				AssertEquals("SACO SHIPPING S.A.", participation3.Names);

				AssertEquals(WrappersConstants.ParticipationName.Emido, participation4.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation4.IDType);
				AssertEquals("76006281-9", participation4.IDValue);
				AssertEquals("NEW CHARTER SRL", participation4.Names);

				AssertEquals(WrappersConstants.ParticipationName.Cons, participation5.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation5.IDType);
				AssertEquals("77144766-K", participation5.IDValue);
				AssertEquals("COMERCIAL Y SERVICIOS HYDROVAK S.P.A.", participation5.Names);

				AssertEquals(WrappersConstants.ParticipationName.Noti, participation6.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation6.IDType);
				AssertEquals("77144744-3", participation6.IDValue);
				AssertEquals("ACRICIEL S.A.", participation6.Names);

				AssertEquals(WrappersConstants.ParticipationName.Cnte, participation7.Name);
				AssertEquals(ChileOrgCusCodeInfo.OrgCusCodes.RUT, participation7.IDType);
				AssertEquals("99999999-9", participation7.IDValue);
				AssertEquals("JUROP S.P.A.", participation7.Names);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Chile);

			GlbCompany.CurrentCompany.GC_BusinessRegNo = "76006380-9";
			GlbCompany.CurrentCompany.GC_Name = "SACO SHIPPING S.A.";
		}

		void PopulateManifestHeader()
		{
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			commonOrgHeader = Factory.New<OrgHeader>();
			commonOrgHeader.OH_Code = "95";
			commonOrgHeader.OH_FullName = "TRANS AMERICAN AIR LINES S.A.";
			commonOrgHeader.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "77491900-7");
			header.AMA_OA_Carrier = FillOrgAddress().PK;

			commonOrgHeader = Factory.New<OrgHeader>();
			commonOrgHeader.OH_Code = "96";
			commonOrgHeader.OH_FullName = "NEW CHARTER SRL";
			commonOrgHeader.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "76006281-9");
			header.AMA_OA_ShippingAgent = FillOrgAddress().PK;
		}

		void CreateAndPopulateHouseBill()
		{
			commonOrgHeader = Factory.New<OrgHeader>();
			commonOrgHeader.OH_Code = "97";
			commonOrgHeader.OH_FullName = "DEPOCARGO LTDA.";
			commonOrgHeader.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "96888200-7", Core.Constants.CountryCodes.Chile);
			commonOrgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "96888201-7", Core.Constants.CountryCodes.Chile);

			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_OA_GoodsLocation = commonOrgHeader.MainAddress.PK;
			bill.ABL_OA_Consignee = FillOrgAddress("COMERCIAL Y SERVICIOS HYDROVAK S.P.A.").PK;
			bill.ABL_ConsigneeRegNo = "77144766-K";
			bill.ABL_OA_NotifyParty = FillOrgAddress("ACRICIEL S.A.").PK;
			bill.ABL_NotifyPartyRegNo = "77144744-3";
			bill.ABL_OA_Shipper = FillOrgAddress("JUROP S.P.A.").PK;
			bill.ABL_ShipperRegNo = "99999999-9";
		}

		OrgAddress FillOrgAddress(string orgType = "")
		{
			OrgAddress orgAddress = commonOrgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = string.Concat(orgType, "Address1");
			orgAddress.CompanyName = orgType;
			return orgAddress;
		}
	}
}
