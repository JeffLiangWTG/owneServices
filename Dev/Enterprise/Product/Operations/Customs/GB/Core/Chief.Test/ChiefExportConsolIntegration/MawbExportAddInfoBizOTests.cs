using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.MultiLineAddInfos.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	[TestedType(typeof(CusAddInfo<MawbExportAddInfo>))]
	class CusAddInfo_MawbExportAddInfoTest : CusAddInfoTest<CusAddInfo<MawbExportAddInfo>>
	{
		protected override IEnumerable<CusAddInfo<MawbExportAddInfo>> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var consol = factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var result = factory.New<CusAddInfo<MawbExportAddInfo>>();
			result.B7_ParentID = consol.PK;
			result.B7_ParentTableCode = consol.TablePrefix;
			yield return result;
		}
	}

	[TestedType(typeof(MawbExportAddInfo))]
	public class MawbExportAddInfoTest : PersistentBusinessObjectTestCase
	{
		public void TestReadOnly()
		{
			var meai = GetMawbExportAddInfo();
			meai.ME_UseAntiSmugglingTrptid = true;
			Assert(meai.ME_TransportCountryInfo.ReadOnly);
			Assert(meai.ME_TransportIDInfo.ReadOnly);
			Assert(meai.ME_TransportModeInfo.ReadOnly);
			Assert(meai.ME_TransportCountryInfo.ReadOnly);
			Assert(!meai.ME_CommunityTransitStatusInfo.ReadOnly);
			Assert(!meai.ME_PartMovementIndicatorInfo.ReadOnly);
			meai.ME_UseAntiSmugglingTrptid = false;
			Assert(!meai.ME_TransportCountryInfo.ReadOnly);
			Assert(!meai.ME_TransportIDInfo.ReadOnly);
			Assert(!meai.ME_TransportModeInfo.ReadOnly);
			Assert(!meai.ME_TransportCountryInfo.ReadOnly);
			Assert(meai.ME_CommunityTransitStatusInfo.ReadOnly);
			Assert(!meai.ME_PartMovementIndicatorInfo.ReadOnly);
		}

		public void TestME_ChiefMasterRouteOfEntryConvertedToEnterpriseForBinding()
		{
			var meai = GetMawbExportAddInfo();
			meai.ME_ChiefMasterRouteOfEntry = "6";
			AssertEquals("6", meai.ME_ChiefMasterRouteOfEntry);
			AssertEquals("RT6", meai.ME_ChiefMasterRouteOfEntryConvertedToEnterpriseForBinding);
		}

		public void TestLoadFromConsol()
		{
			consol.JK_RL_NKLoadPort = "GBSTN";
			var dvg = Factory.New<OrgHeader>();
			dvg.CustomsCodes.AddNew(OrgCusCode.UnitedKingdomCodeTypes.CTOShed, "DVG", Core.Constants.CountryCodes.UnitedKingdom);
			dvg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", Core.Constants.CountryCodes.UnitedKingdom);
			MawbTestHelper.MakeBadge("XYZ", GatewayList.Codes.CCSUKviaNTMsgGW, "", true, "", false, false, BadgeDirectionList.Codes.EXP, "GBSTN", Registry.MucrGenerationStyles.Codes.Air);
			consol.JK_TransportMode = "AIR";
			consol.Transports[0].JW_VoyageFlight = "BA123";
			consol.JK_MasterBillNum = "12512345678";
			consol.JK_OA_DepartureCTOAddress = dvg.MainAddress.PK;
			var mawbExportAddInfo = GetMawbExportAddInfo();

			AssertEquals("A:12512345678", mawbExportAddInfo.ME_MasterUCR);
			AssertEquals("GBSTN for Stansted is converted to CHIEF code LSA", "LSA", mawbExportAddInfo.ME_ExportLocation);
			AssertEquals("GB", mawbExportAddInfo.ME_TransportCountry);
			AssertEquals("BA123", mawbExportAddInfo.ME_TransportID);
			AssertEquals("AIR", mawbExportAddInfo.ME_TransportMode);
			AssertEquals("XYZ", mawbExportAddInfo.ME_Profile);
			AssertEquals("DVG", mawbExportAddInfo.ME_ExportShed);

			consol.JK_TransportMode = "SEA";
			consol.JK_OA_SendingForwarderAddress = dvg.MainAddress.PK;
			consol.JK_UniqueConsignRef = "C000123";
			mawbExportAddInfo = GetMawbExportAddInfo();
			mawbExportAddInfo.CalculateMUCR();
			AssertEquals("A:12512345678", mawbExportAddInfo.ME_MasterUCR);

			consol.JK_RL_NKLoadPort = ZString.Empty;
			AssertNoExceptionThrown(() =>
			{
				GetMawbExportAddInfo().Initialize();
			});

			GlbBranch.CurrentBranch.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ZAJNB";

			consol.JK_RL_NKLoadPort = "ZAJNB";
			AssertNoExceptionThrown(() =>
			{
				GetMawbExportAddInfo().Initialize();
			});
		}

		public void TestMUCRIsUpdatedWhenProfileIsChanged()
		{
			consol.JK_RL_NKLoadPort = "GBSTN";
			var dvg = Factory.New<OrgHeader>();
			dvg.CustomsCodes.AddNew(OrgCusCode.UnitedKingdomCodeTypes.CTOShed, "DVG", Core.Constants.CountryCodes.UnitedKingdom);
			dvg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", Core.Constants.CountryCodes.UnitedKingdom);
			MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CCSUKviaNTMsgGW, "", true, "", true, false, BadgeDirectionList.Codes.EXP, "GBSTN", Registry.MucrGenerationStyles.Codes.Air);
			MawbTestHelper.MakeBadge("DEF", GatewayList.Codes.CCSUKviaNTMsgGW, "", true, "", false, false, BadgeDirectionList.Codes.EXP, "GBSTN", Registry.MucrGenerationStyles.Codes.CSPExport);

			consol.JK_TransportMode = "AIR";
			consol.Transports[0].JW_VoyageFlight = "BA123";
			consol.JK_MasterBillNum = "12512345678";
			consol.JK_OA_DepartureCTOAddress = dvg.MainAddress.PK;
			consol.JK_UniqueConsignRef = "C00000001";
			var mawbExportAddInfo = GetMawbExportAddInfo();
			mawbExportAddInfo.ME_Profile = "ABC";
			AssertEquals("A:12512345678", mawbExportAddInfo.ME_MasterUCR);
			mawbExportAddInfo.ME_Profile = "DEF";
			AssertEquals("GB/CCSUK-C00000001", mawbExportAddInfo.ME_MasterUCR);
		}

		public void TestDeclarant()
		{
			var mawbExportAddInfo = GetMawbExportAddInfo();

			var agent = Factory.New<OrgHeader>();
			var agentAddress = agent.Addresses.AddNewMainAddress();

			var currentBranchId = mawbExportAddInfo.Branch.PK.ToGuid();
			var currentBranchOrgProxyMainAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress;

			AssertNotEquals("Pre-requisite: agentAddress must not equal branch OrgProxy address for this test to be valid", currentBranchOrgProxyMainAddress, agentAddress);

			using (GBCustomsDataRegistry.Instance.GB_UseBranchEoriForMucrOnConsols.SetTemporaryValue(Guid.Empty, currentBranchId, Guid.Empty, false))
			{
				AssertEquals("GB_UseBranchEoriForMucrOnConsols false, SendingAgent not set, Declarant should be Branch OrgProxy", currentBranchOrgProxyMainAddress, mawbExportAddInfo.Declarant);
			}
			using (GBCustomsDataRegistry.Instance.GB_UseBranchEoriForMucrOnConsols.SetTemporaryValue(Guid.Empty, currentBranchId, Guid.Empty, true))
			{
				AssertEquals("GB_UseBranchEoriForMucrOnConsols true, SendingAgent not set, Declarant should be Branch OrgProxy", currentBranchOrgProxyMainAddress, mawbExportAddInfo.Declarant);
			}

			consol.JK_OA_SendingForwarderAddress = agentAddress.PK;

			using (GBCustomsDataRegistry.Instance.GB_UseBranchEoriForMucrOnConsols.SetTemporaryValue(Guid.Empty, currentBranchId, Guid.Empty, false))
			{
				AssertEquals("GB_UseBranchEoriForMucrOnConsols false, SendingAgent set, Declarant should be SendingAgent", agentAddress, mawbExportAddInfo.Declarant);
			}
			using (GBCustomsDataRegistry.Instance.GB_UseBranchEoriForMucrOnConsols.SetTemporaryValue(Guid.Empty, currentBranchId, Guid.Empty, true))
			{
				AssertEquals("GB_UseBranchEoriForMucrOnConsols true, SendingAgent set, Declarant should be Branch OrgProxy", currentBranchOrgProxyMainAddress, mawbExportAddInfo.Declarant);
			}
		}

		public void TestSetAirportAndShedFromProfile_AgentProfile()
		{
			MawbTestHelper.MakeBadge("LXA", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "LXA", false, false, BadgeDirectionList.Codes.Both, "GBABC");
			consol.JK_RL_NKLoadPort = "GBABC";
			var mawbExportAddInfo = GetMawbExportAddInfo();
			AssertEquals("Single PIMA so is selected automatically", "LXA", mawbExportAddInfo.ME_Profile);
			AssertEquals("ABC", mawbExportAddInfo.ME_ExportLocation);
			AssertEquals("", mawbExportAddInfo.ME_ExportShed);
		}

		public void TestProfiles_Multiple()
		{
			MawbTestHelper.MakeBadge("ONE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false);
			MawbTestHelper.MakeBadge("TWO", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false);
			consol.JK_RL_NKLoadPort = "GBABC";
			var mawbExportAddInfo = GetMawbExportAddInfo();
			AssertEquals("Should select the first profile", "ONE", mawbExportAddInfo.ME_Profile);
		}

		public void TestProfiles_MultipleWithPrimary()
		{
			MawbTestHelper.MakeBadge("ONE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false, false, BadgeDirectionList.Codes.Both, "GBABC");
			MawbTestHelper.MakeBadge("TWO", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: true, false, BadgeDirectionList.Codes.Both, "GBABC");
			MawbTestHelper.MakeBadge("TRE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false, false, BadgeDirectionList.Codes.Both, "GBABC");
			MawbTestHelper.MakeBadge("FOR", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false, false, BadgeDirectionList.Codes.Both, "GBABC");
			consol.JK_RL_NKLoadPort = "GBABC";
			var mawbExportAddInfo = GetMawbExportAddInfo();
			AssertEquals("Primary PIMA is selected automatically", "TWO", mawbExportAddInfo.ME_Profile);
		}

		public void TestLoadFromConsolWithBadRefLocoMap()
		{
			consol.JK_TransportMode = "AIR";
			var badlyMappedLoco = Factory.New<RefUNLOCO>();
			badlyMappedLoco.RL_Code = "GBDJC";
			var crapMap = Factory.New<RefLocoMap>();
			crapMap.RY_LocalPortCode = "TOO LONG";
			crapMap.RY_RL_NKLocoPort = badlyMappedLoco.RL_Code;
			crapMap.RY_RN = Core.CountryGuids.Instance.UnitedKingdom;
			crapMap.RY_SystemUsage = "AIR";
			consol.JK_RL_NKLoadPort = badlyMappedLoco.RL_Code;
			var mawbExportAddInfo = GetMawbExportAddInfo();
			AssertEquals("TOO", mawbExportAddInfo.ME_ExportLocation);
		}

		[ExpectNoExceptions]
		public void TestLoadFromConsolWithInvalidLoadPort()
		{
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "GB123";
			var mawbExportAddInfo = GetMawbExportAddInfo();
		}

		public void TestCommunityTransitStatus()
		{
			consol.Shipments.CountChanged += new CollectionCountChangedEventHandler(Shipments_CountChanged); // simulating how the plugin works
			try
			{
				var mawbExportAddInfo = GetMawbExportAddInfo();
				AssertEquals("Out of the box the CTS is blank", "", mawbExportAddInfo.ME_CommunityTransitStatus);
				mawbExportAddInfo.ME_CommunityTransitStatus = "X";
				AssertEquals("If set manually, can be retrieved", "X", mawbExportAddInfo.ME_CommunityTransitStatus);
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_CommunityTransitStatus = "T2";
				consol.Shipments.Add(shipment);
				AssertEquals("Adding a shipment to the consol pulls forward the shipment's CTS", "T2", mawbExportAddInfo.ME_CommunityTransitStatus);
				mawbExportAddInfo.ME_UseAntiSmugglingTrptid = true;
				mawbExportAddInfo.ME_CommunityTransitStatus = "Q";
				AssertEquals("Ticking the 'Use TRPT-ID' box lets user set own CTS", "Q", mawbExportAddInfo.ME_CommunityTransitStatus);
				mawbExportAddInfo.ME_UseAntiSmugglingTrptid = false;
				AssertEquals("And unticking 'Use TRPT-ID' recalculates the CTS for us, to be the most severe of the consol's shipments", "T2", mawbExportAddInfo.ME_CommunityTransitStatus);
			}
			finally
			{
				consol.Shipments.CountChanged -= new CollectionCountChangedEventHandler(Shipments_CountChanged);
			}
		}

		public void TestProfileIsSetWhenLoadPortChangesOnConsolChangedAndShowsCorrectLookupList()
		{
			using (GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_RL_NKLoadPort = "GBLHR";
				MawbTestHelper.MakeBadge("ONE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
				MawbTestHelper.MakeBadge("TWO", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: true, false, BadgeDirectionList.Codes.EXP, "GBMAN");
				MawbTestHelper.MakeBadge("TRE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false, false, BadgeDirectionList.Codes.Both, "GBLHR");
				MawbTestHelper.MakeBadge("FOR", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false, false, BadgeDirectionList.Codes.IMP, "GBLHR");

				var mawbExportAddInfo = GetMawbExportAddInfo();

				AssertEquals(2, mawbExportAddInfo.Lookups.ProfilesListForExports.Count);
				AssertEquals("ONE", mawbExportAddInfo.ME_Profile);

				consol.JK_RL_NKLoadPort = "GBMAN";
				AssertEquals(1, mawbExportAddInfo.Lookups.ProfilesListForExports.Count);
				AssertEquals("TWO", mawbExportAddInfo.ME_Profile);

				consol.JK_RL_NKLoadPort = "GBABC";
				AssertEquals(0, mawbExportAddInfo.Lookups.ProfilesListForExports.Count);
				AssertEquals("", mawbExportAddInfo.ME_Profile);
			}
		}

		void Shipments_CountChanged(object sender, CollectionCountChangedEventArgs collectionCountChangedEventArgs)
		{
			wrapper.HandleShipmentAddedToOrRemovedFromConsol(collectionCountChangedEventArgs);
		}

		MawbExportAddInfo GetMawbExportAddInfo()
		{
			wrapper = new CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsShutterUpperer());
			return wrapper.MawbExportHelper;
		}
		CustomsExportConsolIntegrationWrapper wrapper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBizO(Factory).Data;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBizO(factory);
		}

		CusAddInfo<MawbExportAddInfo> GetNewBizO(BusinessObjectFactory factory)
		{
			if (consol == null)
			{
				SetUp();
			}
			var mawbExportAddInfosCollectionOfOneItem = new CusAddInfoCollection<MawbExportAddInfo>(consol);
			mawbExportAddInfosCollectionOfOneItem.Load();
			mawbExportAddInfosCollectionOfOneItem.AddNew();
			return mawbExportAddInfosCollectionOfOneItem[0];
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
		}

		ForwardingConsol consol;
	}
}
