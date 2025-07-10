using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing
{
	public class LicencingAndShedRestrictionsTests : TestCaseWithFactory
	{
		public void TestCheckHasShedLicenceButDoNotDetermineWhetherOneIsNeeded()
		{
			AssertEquals(true, LicenceAndPimaHelper.ShedEnabled);
			var ec = new ErrorCollector();
			LicenceAndPimaHelper.CheckHasShedLicenceButDoNotDetermineWhetherOneIsNeeded(ec, "My cat's breath smells like catfood");
			AssertNotContains("catfood", ec.GetErrorsAsString());
		}

		public void TestProfileIsSuchAndSuch()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKFFW98000LXA";
			RunAgentTypeTest(true, false, false, mawb);

			mawb.Profile = "CUKAIR98LHRSHD";
			RunAgentTypeTest(false, false, true, mawb);

			MakeBadgeAndCredentials(true);
			mawb.Profile = "CUKAIR98LHRLXA";
			RunAgentTypeTest(false, true, false, mawb);
		}

		void RunAgentTypeTest(bool isSimpleAgentProfile,
								bool isFallbackShed,
								bool isFullShed,
								ICcsukCusAwb awb)
		{
			AssertEquals(isSimpleAgentProfile, LicenceAndPimaHelper.IsSimpleAgentProfile(awb));
			AssertEquals(isFallbackShed, LicenceAndPimaHelper.IsFallbackShed(awb));
			AssertEquals(isFullShed, LicenceAndPimaHelper.IsFullShed(awb));
		}

		public static void MakeBadgeAndCredentials(bool alsoMakeFallback, bool setPreferredAgentForShed = false, string fallbackAgentType = "", string shedCode = "BAC", string shedAirport = "LHR", string direction = "", string mucrGenerationStyle = "")
		{
			EnsureAgentLxa();
			var agentBadge = new BadgeCodeSetting();
			agentBadge.Direction = direction;
			agentBadge.BadgeCode = "LXA";
			agentBadge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			agentBadge.RL_PortCode = "GBLHR";
			agentBadge.MasterUcrCalculationMode = mucrGenerationStyle;
			var shedBadge = new BadgeCodeSetting();
			shedBadge.Direction = direction;
			shedBadge.BadgeCode = shedCode;
			shedBadge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			shedBadge.RL_PortCode = "GBLHR";
			shedBadge.MasterUcrCalculationMode = mucrGenerationStyle;
			var badges = new BadgeCodeSettingCollection();
			badges.Add(agentBadge);
			badges.Add(shedBadge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, badges);
			var agentBadgeCredential = new CredentialsSetting();
			agentBadgeCredential.BadgeCode = "LXA";
			agentBadgeCredential.Company = agentBadgeCredential.BadgeCode;
			agentBadgeCredential.PIMA = "CUKFFW98000LXA";
			agentBadgeCredential.CcsukFallbackAgentType = fallbackAgentType;
			var shedBadgeCredential = new CredentialsSetting();
			shedBadgeCredential.BadgeCode = shedCode;
			shedBadgeCredential.PIMA = "CUKAIR98" + shedAirport + shedCode;
			if (setPreferredAgentForShed)
			{
				shedBadgeCredential.PreferredAgent = "DJC";
			}

			var fallbackCredential = new CredentialsSetting();
			fallbackCredential.PIMA = "CUKAIR98LHRLXA";
			fallbackCredential.FallbackForShed = shedCode;
			var credentials = new CredentialsSettingCollection();
			credentials.Add(agentBadgeCredential);
			credentials.Add(shedBadgeCredential);
			if (alsoMakeFallback)
			{
				credentials.Add(fallbackCredential);
			}
			GBCustomsDataRegistry.Instance.Credentials.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, credentials);
		}

		public static void EnsureAgentLxa()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsAgentCode, "Customs Agent");
			var agent = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.CustomsAgentCode, "LXA", "Daniel Test Agent", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public void TestSettingPimaRecordsLicenceHit()
		{
			MakeBadgeAndCredentials(false);
			var shedPima = "CUKAIR98LHRBAC";
			var agentPima = "CUKFFW98000LXA";
			var shedPimaNotInRegistry = "CUKAIR98LHRZZZ";
			Env.Security.AirCcsukShed.IsAllowed = true;
			var shedBasic = Factory.New<CusMAWB>();
			shedBasic.MasterLevelHouseHelper.CcsukLicenceLoginHandler += new Customs.Business.LicenceLoginEventHandler(HandleLicenceLoginForTestToSimulateGui);
			shedBasic.Profile = shedPima;
			AssertEquals("Licence, no red error", false, shedBasic.ProfileInfo.HasErrors());
			AssertEquals("Record an erts hit", 1, ertsLicenceLoginRequestCount);
			AssertEquals("Shed, no agent hit", 0, agentLicenceLoginRequestCount);
			shedBasic.Profile = agentPima;
			AssertEquals("Agent, no red error", false, shedBasic.ProfileInfo.HasErrors());
			AssertEquals("Agent, record an agent hit", 1, agentLicenceLoginRequestCount);
			AssertEquals("Agent, do not record a shed hit", 1, ertsLicenceLoginRequestCount);

			var newFactoryOnceShedLicenceObtained = new BusinessObjectFactory();
			var licencedBasic = newFactoryOnceShedLicenceObtained.New<CusMAWB>();
			licencedBasic.MasterLevelHouseHelper.CcsukLicenceLoginHandler += new Customs.Business.LicenceLoginEventHandler(HandleLicenceLoginForTestToSimulateGui);
			licencedBasic.Profile = shedPima;
			AssertEquals("Has licence, no errors", false, licencedBasic.ProfileInfo.HasErrors());
			AssertEquals("Shed hit bumped", 2, ertsLicenceLoginRequestCount);
			AssertEquals("Agent hit not bumped", 1, agentLicenceLoginRequestCount);
			licencedBasic.Profile = shedPimaNotInRegistry;
			AssertEquals("Has licence, but duff PIMA, has errors", true, licencedBasic.ProfileInfo.HasErrors());
			AssertEquals("No increase in login tries when PIMA has red error", 2, ertsLicenceLoginRequestCount);
			AssertEquals("No increase in login tries when PIMA has red error", 1, agentLicenceLoginRequestCount);
			licencedBasic.Profile = shedPima;
			AssertEquals("Shed hit count bumped", 3, ertsLicenceLoginRequestCount);
			AssertEquals("Agent hit count not bumped", 1, agentLicenceLoginRequestCount);
			Env.Security.AirCcsukShed.IsAllowed = false;
			licencedBasic.Profile = "";
			licencedBasic.Profile = shedPima;
			AssertEquals("Has licence, good PIMA, but no security right", true, licencedBasic.ProfileInfo.HasErrors());
			AssertEquals("No increase in login tries when PIMA has red error", 3, ertsLicenceLoginRequestCount);
			AssertEquals("No increase in login tries when PIMA has red error", 1, agentLicenceLoginRequestCount);
		}

		void HandleLicenceLoginForTestToSimulateGui(object sender, Customs.Business.LicenceLoginEventArgs e)
		{
			e.LoginHasBeenAttempted = true;
			e.LicenceCheckPoint.Login(new TestLicensedComponent());
			var ccsukArgs = e as LicenceAndPimaHelper.CcsukLicenceLoginEventArgs;
			if (ccsukArgs != null && ccsukArgs.CcsukLicenceType == LicenceAndPimaHelper.CcsukLicenceType.Agent)
			{
				agentLicenceLoginRequestCount++;
			}
			else if (ccsukArgs != null && ccsukArgs.CcsukLicenceType == LicenceAndPimaHelper.CcsukLicenceType.ErtsShed)
			{
				ertsLicenceLoginRequestCount++;
			}
		}

		// These are not necessarily what gets recorded in StmActivityLog, the framework magic takes care of the per-user and per-month stuff for us I think
		int ertsLicenceLoginRequestCount;
		int agentLicenceLoginRequestCount;

		class TestLicensedComponent : ILicensedComponent
		{
			public LicensedComponentManager LicensedComponentManager
			{
				get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
			}

			IDisposable ILicensedComponent.LicensedComponentManager
			{
				get { return licensedComponentManager ?? (licensedComponentManager = new LicensedComponentManager(this)); }
			}
			LicensedComponentManager licensedComponentManager;
		}
	}
}
