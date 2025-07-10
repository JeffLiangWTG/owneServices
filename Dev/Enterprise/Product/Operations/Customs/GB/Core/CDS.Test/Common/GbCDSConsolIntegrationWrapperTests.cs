using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Chief.CusDec;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class GbCDSConsolIntegrationWrapperTests : TestCaseWithFactory
	{
		public void TestICDSRequestMessageDataProvider()
		{
			var consol = CreateSampleWrapper(Factory);
			var provider = new ChiefExportConsolIntegrationWrapperToIUkCinvWrapper(new CustomsExportConsolIntegrationWrapper(consol, null));
			CombineAssertions(() =>
			{
				AssertEquals("A:MUCR1234567", provider.MasterUniqueConsignmentReference);
				AssertEquals("", provider.DeclarationUniqueConsignmentReference);
				AssertEquals("LOC", provider.LocationOfGoods);
				AssertEquals(new ZDateTime(2018, 6, 6), provider.DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises);
				AssertEquals(new ZDateTime(2018, 6, 6), provider.DateAndTimeTheGoodsWillBeLeavingLCPPremises);
				AssertEquals("06Jun0000PART", provider.MovementReference);
				AssertEquals("She", provider.ShedCode);
				AssertEquals("X", provider.MasterOpt);
				AssertEquals("TransportId", provider.TransportIdentityAtTheBorderBox21);
				AssertEquals("4", provider.TransportModeAtTheBorderBox25);
				AssertEquals("GB", provider.TransportNationalityAtTheBorderBox21);
			});
		}

		public static ForwardingConsol CreateSampleWrapper(BusinessObjectFactory factory)
		{
			MawbTestHelper.MakeBadge("ZPE", GatewayList.Codes.CDS, "CUKFFW98000", true, "ZPE", false, false, BadgeDirectionList.Codes.EXP, "GBLHR", MucrGenerationStyles.Codes.Air);
			MawbTestHelper.MakeBadge("ZPF", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "ZPF", false, true, BadgeDirectionList.Codes.EXP, "GBMAN", MucrGenerationStyles.Codes.Air);

			var badges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badge = badges.FindByBadgeCodeOnly("ZPE");
			badge.ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);

			TestDataHelper.CreateCredentials(factory, GlbCompany.CurrentCompany.PK, "ZPE", "GB12345654321", PasswordTypesList.Codes.CDS);

			var sendingForwarder = factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", Core.Constants.CountryCodes.UnitedKingdom);

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "Consol123";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "MUCR1234567";
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			var mawbExportAddInfo = wrapper.MawbExportHelper;
			mawbExportAddInfo.ME_Profile = "GB12345654321.ZPE";
			mawbExportAddInfo.ME_ExportLocation = "LOC";
			mawbExportAddInfo.ME_ExportShed = "She";
			mawbExportAddInfo.ME_MovementDate = new ZDateTime(2018, 6, 6, DateTimeKind.Local);
			mawbExportAddInfo.ME_TransportMode = TransportTypeList.Codes.Air;
			mawbExportAddInfo.ME_TransportCountry = Core.Constants.CountryCodes.UnitedKingdom;
			mawbExportAddInfo.ME_TransportID = "TransportId";
			mawbExportAddInfo.ME_MasterOpt = "X";
			mawbExportAddInfo.ME_PartMovementIndicator = true;
			factory.Save();

			return consol;
		}
	}
}
