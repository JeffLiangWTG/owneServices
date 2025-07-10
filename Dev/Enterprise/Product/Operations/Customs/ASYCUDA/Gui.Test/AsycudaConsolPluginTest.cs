using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaConsolPluginTest : TestCaseWithFactory
	{
		public void TestEnableAndDisableFromZz()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry("AIR", "VU"), new ModeAndCountry("SEA", "VU"), new ModeAndCountry("AIR", "PG"), new ModeAndCountry("ROA", "PG"), new ModeAndCountry("", "FJ"));
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AAA";
			using (var plugIn = new AsycudaConsolPlugin(consol))
			{
				// AU & VU
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "VUVLI";
				consol.JK_TransportMode = "AIR";
				AssertEquals(true, plugIn.Enabled);
				consol.JK_TransportMode = "SEA";
				AssertEquals(true, plugIn.Enabled);
				consol.JK_TransportMode = "ROA";
				AssertEquals(false, plugIn.Enabled);

				// AU & PG
				consol.Transports.RemoveAndDeleteAll();
				consol.JK_RL_NKDischargePort = "PGXXX";
				consol.JK_TransportMode = "AIR";
				AssertEquals(true, plugIn.Enabled);
				consol.JK_TransportMode = "SEA";
				AssertEquals(false, plugIn.Enabled);
				consol.JK_TransportMode = "ROA";
				AssertEquals(true, plugIn.Enabled);

				// AU & FJ
				consol.Transports.RemoveAndDeleteAll();
				consol.JK_RL_NKDischargePort = "FJXXX";
				consol.JK_TransportMode = "AIR";
				AssertEquals(true, plugIn.Enabled);
				consol.JK_TransportMode = "SEA";
				AssertEquals(true, plugIn.Enabled);
				consol.JK_TransportMode = "ROA";
				AssertEquals(true, plugIn.Enabled);

				// VU & PG
				consol.Transports.RemoveAndDeleteAll();
				consol.JK_RL_NKLoadPort = "VUXXX";
				consol.JK_RL_NKDischargePort = "PGYYY";
				consol.JK_TransportMode = "AIR";
				AssertEquals(true, plugIn.Enabled);
				consol.JK_TransportMode = "SEA";
				AssertEquals("VU - SEA", true, plugIn.Enabled);
				consol.JK_TransportMode = "ROA";
				AssertEquals("PG - ROA", true, plugIn.Enabled);
			}
		}

		public void TestMenu()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "VUAUY";
			using (var plugIn = new AsycudaConsolPlugin(consol))
			{
				var menuItem = plugIn.TopLevelMenu;
				AssertNotNull(menuItem);
				AssertType(typeof(AsycudaMenu), menuItem);
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "ERAUY";

			using (new AsycudaConsolPlugin(consol))
			{
				consol.Delete();
				Factory.Save();
			}
		}

		public void TestVesselControlVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var erCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping, C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(erCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, manifestCountryAttributeValue);
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "ERXXX";
			consol.JK_UniqueConsignRef = "C456";
			consol.JK_TransportMode = "SEA";

			using (var form = new ZForm(consol))
			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				var manifestCountries = plugIn.GetAllPossibleConsolCountries().Where(x => !x.IsEmpty && plugIn.HeaderWrapper.CountryCodes.ContainsCode(x));
				foreach (var country in manifestCountries)
				{
					// Assume user has decided to "create" these manifests
					var header = plugIn.HeaderWrapper.Headers.AddNew(AsycudaManifestHeader.TypeDecider.GetGlobalManifestType(Factory, country, ZString.Empty, ZString.Empty));
					header.FillWithValidTestData();
					header.AMA_RN_NKCountry = country;
					header.SynchroniseWithSourceIfNeeded();
				}

				var control = (AsycudaManifestMainControl)plugIn.UserControl;
				control.SetDataBinding(plugIn.HeaderWrapper, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var tabControl = control.FindSingle<ZTemplateTabControl>();
				var tabPage = tabControl.GetTabPage("ERASYTabPage");
				tabControl.SelectedTab = tabPage;

				var vesselCodeFindBox = (ZCodeFindBox)tabPage.Controls.Find("VesselCodeFindBox", true).First();
				consol.JK_TransportMode = "SEA";
				AssertEquals(true, vesselCodeFindBox.Visible);

				consol.JK_TransportMode = "AIR";
				AssertEquals(false, vesselCodeFindBox.Visible);
			}
		}

		public void TestSynchronisationIsFired()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var erCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping, C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(erCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, manifestCountryAttributeValue);
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "ERXXX";
			consol.JK_UniqueConsignRef = "C456";
			consol.JK_MasterBillNum = "BILL2018";

			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				var manifestCountries = plugIn.GetAllPossibleConsolCountries().Where(x => !x.IsEmpty && plugIn.HeaderWrapper.CountryCodes.ContainsCode(x));
				foreach (var country in manifestCountries)
				{
					// Assume user has decided to "create" these manifests
					var header = plugIn.HeaderWrapper.Headers.AddNew(AsycudaManifestHeader.TypeDecider.GetGlobalManifestType(Factory, country, ZString.Empty, ZString.Empty));
					header.AMA_RN_NKCountry = country;
					header.SynchroniseWithSourceIfNeeded();
				}

				AssertEquals("Synching has occurred", consol.JK_MasterBillNum, plugIn.HeaderWrapper.Headers[0].AMA_MasterBill);
			}
		}

		public void TestCreateNewManifestControlsDoNotActivateTheSaveButton()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var erCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping, C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(erCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, manifestCountryAttributeValue);
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "ERXXX";
			consol.JK_UniqueConsignRef = "C456";
			consol.JK_MasterBillNum = "BILL2018";
			Factory.Save();

			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				AssertEquals(false, plugIn.BusinessEntity.HasChanges);

				plugIn.HeaderWrapper.WR_CountryCode = "FJ";
				AssertEquals("Changing WR_CountryCode does not set HasChanges on the Plugin", false, plugIn.BusinessEntity.HasChanges);

				plugIn.HeaderWrapper.WR_ManifestType = "ASY";
				AssertEquals("Changing WR_ManifestType does not set HasChanges on the Plugin", false, plugIn.BusinessEntity.HasChanges);

				plugIn.HeaderWrapper.WR_KeywordCombination = "SG|MGI";
				AssertEquals("Changing WR_KeywordCombination does not set HasChanges on the Plugin", false, plugIn.BusinessEntity.HasChanges);
			}
		}

		public void TestExcludeZAOutturnGateInOut()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var erCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping, C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(erCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, manifestCountryAttributeValue);
			Factory.Save();

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKDischargePort = "ERAUY";
			consol1.JK_MasterBillNum = "C456";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKDischargePort = "ERAUY";
			consol2.JK_MasterBillNum = "C789";

			var testHeader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			testHeader1.AMA_MasterBill = "ER1";
			testHeader1.AMA_ParentId = consol1.PK;
			testHeader1.AMA_ParentTableCode = consol1.TablePrefix;

			var testHeader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			testHeader2.AMA_ApplicationCode = "OUT";
			testHeader2.AMA_MasterBill = "ER2";
			testHeader2.AMA_ParentId = consol2.PK;
			testHeader2.AMA_ParentTableCode = consol2.TablePrefix;

			Factory.Save();

			using (var plugIn = new AsycudaConsolPluginForTest(consol1))
			{
				plugIn.Enabled = true;
				AssertEquals("ER1", plugIn.HeaderWrapper.Headers[0].AMA_MasterBill);
			}

			using (var plugIn = new AsycudaConsolPluginForTest(consol2))
			{
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				var manifestCountries = plugIn.GetAllPossibleConsolCountries().Where(x => !x.IsEmpty && plugIn.HeaderWrapper.CountryCodes.ContainsCode(x));
				foreach (var country in manifestCountries)
				{
					// Assume user has decided to "create" these manifests
					var header = plugIn.HeaderWrapper.Headers.AddNew(AsycudaManifestHeader.TypeDecider.GetGlobalManifestType(Factory, country, ZString.Empty, ZString.Empty));
					header.AMA_RN_NKCountry = country;
					header.SynchroniseWithSourceIfNeeded();
				}

				AssertEquals("Synching has occurred", "C789", plugIn.HeaderWrapper.Headers[0].AMA_MasterBill);
			}
		}

		public void TestHeadersTypeWhenCreateManifest()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory,
				new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.SouthAfrica),
				new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.Eritrea));

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ERXXX";
			consol.JK_RL_NKDischargePort = "ZAXXX";

			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				AssertEquals("Manifests are not auto created any more", 0, plugIn.HeaderWrapper.Headers.Count);
			}
		}

		public void TestAnotherUserCreatesDefaultManifest()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.Fiji));
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "FJXXX";
			consol.JK_RL_NKDischargePort = "AUXXX";
			Factory.Save();
			new ManifestHeadersWrapper(consol);

			using (var form = new ZForm(consol))
			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				var control = (AsycudaManifestMainControl)plugIn.UserControl;
				control.SetDataBinding(plugIn.HeaderWrapper, string.Empty);

				form.Controls.Add(control);
				form.Show();

				plugIn.Enabled = false;
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				Assert(!plugIn.HeaderWrapper.Headers.Any());

				using (var plugIn2 = new AsycudaConsolPluginForTest(consol))
				{
					plugIn2.Enabled = false;
					plugIn2.Enabled = true;
					plugIn2.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
					var expectedMutexMessage = "is already in the process of creating a Customs Manifest for this Consol.\r\nYou should be able to access the Customs Manifest when the person has saved the record. Please try later.";
					AssertEquals(true, plugIn2.PlugInNotDisplayedMessage.Contains(expectedMutexMessage));
				}
			}
		}

		public void TestQueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var erCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping, C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, Core.Constants.CountryCodes.Fiji, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(erCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, manifestCountryAttributeValue);
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "FJXXX";

			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				plugIn.Enabled = true;
				Assert(plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed());
			}
		}

		public void TestSingaporeDefaultManifestNotCreated()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory,
				new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.Singapore),
				new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.Fiji));
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "SGXXX";
			consol.JK_RL_NKDischargePort = "FJXXX";

			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				AssertEquals("Manifest is no longer be auto created", 0, plugIn.HeaderWrapper.Headers.Count);
			}
		}

		public void TestManifestsAreCreatedFromConsolAndTranshipmentDetails()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(
				Factory,
				new ModeAndCountry(Core.Constants.TransportModes.Sea, Core.Constants.CountryCodes.Fiji),
				new ModeAndCountry(Core.Constants.TransportModes.Sea, Core.Constants.CountryCodes.Vanuatu),
				new ModeAndCountry(Core.Constants.TransportModes.Sea, Core.Constants.CountryCodes.Bangladesh));

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "BDXXX";
			consol.JK_RL_NKDischargePort = "VUXXX";
			var transportLeg1 = consol.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "BDXXX";
			transportLeg1.JW_RL_NKDiscPort = "FJXXX";
			var transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "FJXXX";
			transportLeg2.JW_RL_NKDiscPort = "VUXXX";
			Factory.Save();
			new ManifestHeadersWrapper(consol);

			using (var form = new ZForm(consol))
			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				plugIn.Enabled = true;
				var control = (AsycudaManifestMainControl)plugIn.UserControl;
				control.SetDataBinding(plugIn.HeaderWrapper, string.Empty);
				form.Controls.Add(control);
				form.Show();

				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				var headers = plugIn.HeaderWrapper.Headers;
				AssertEquals("Manifests are no longer auto created any more", 0, headers.Count);

				var manifestCountries = plugIn.GetAllPossibleConsolCountries().Where(x => !x.IsEmpty && plugIn.HeaderWrapper.CountryCodes.ContainsCode(x));
				foreach (var country in manifestCountries)
				{
					// Assume user has decided to "create" these manifests
					var header = plugIn.HeaderWrapper.Headers.AddNew(AsycudaManifestHeader.TypeDecider.GetGlobalManifestType(Factory, country, ZString.Empty, ZString.Empty));
					header.AMA_RN_NKCountry = country;
					header.SynchroniseWithSourceIfNeeded();
				}

				AssertEquals(3, headers.Count);
				Assert("We should create a header for FJXXX port", headers.Cast<AsycudaManifestHeader>().Any(x => x.AMA_RN_NKCountry == Core.Constants.CountryCodes.Fiji));
				Assert("We should create a header for VUXXX port", headers.Cast<AsycudaManifestHeader>().Any(x => x.AMA_RN_NKCountry == Core.Constants.CountryCodes.Vanuatu));
				Assert("We should create a header for BDXXX port", headers.Cast<AsycudaManifestHeader>().Any(x => x.AMA_RN_NKCountry == Core.Constants.CountryCodes.Bangladesh));
			}
		}

		public void TestDefaultCountryCodeWhenTheCodeIsInEuropeanUnion()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(
				Factory,
				new ModeAndCountry(Core.Constants.TransportModes.Air, Core.Constants.CountryCodes.Germany));

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "PTTST";

			Factory.Save();

			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				AssertEquals("Should default to EU when the country code is not in the list and in the EU country codes.", Core.Constants.CountryCodes.EuropeanUnion, plugIn.HeaderWrapper.WR_CountryCode);
			}

			consol.JK_RL_NKDischargePort = "DETST";

			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				AssertEquals("Should default to Germany when the country code is in the list.",
					Core.Constants.CountryCodes.Germany, plugIn.HeaderWrapper.WR_CountryCode);
			}
		}

		public void TestMostAppropriateManifest()
		{
			var localCountryCode = GlbBranch.CurrentBranch.HomePort.Country.Code;
			var remoteCountryCode1 = "PG";
			var remoteCountryCode2 = "VU";
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(
				Factory,
				new ModeAndCountry("AIR", localCountryCode),
				new ModeAndCountry("AIR", remoteCountryCode1),
				new ModeAndCountry("AIR", remoteCountryCode2));

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			TestMostAppropriateManifestScenario("export", consol, localCountryCode, remoteCountryCode1, localCountryCode);
			TestMostAppropriateManifestScenario("import", consol, remoteCountryCode1, localCountryCode, localCountryCode);
			TestMostAppropriateManifestScenario("transit", consol, remoteCountryCode1, remoteCountryCode2, remoteCountryCode2);
		}

		static void TestMostAppropriateManifestScenario(string description, ForwardingConsol consol, string loadPortCountry, string dischargePortCountry, string expectedPortCountry)
		{
			using (var plugIn = new AsycudaConsolPluginForTest(consol))
			{
				consol.JK_RL_NKLoadPort = $"{loadPortCountry}XXX";
				consol.JK_RL_NKDischargePort = $"{dischargePortCountry}XXX";
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				AssertEquals($"The country code should fit the {description} direction logic", expectedPortCountry, plugIn.HeaderWrapper.WR_CountryCode);
			}
		}

		sealed class AsycudaConsolPluginForTest : AsycudaConsolPlugin
		{
			public AsycudaConsolPluginForTest(ForwardingConsol hostBusinessEntity)
			: base(hostBusinessEntity)
			{
			}

			public bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed()
			{
				return base.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
			}

			public IEnumerable<ZString> GetAllPossibleConsolCountries()
			{
				var allPossibleConsolCountries = new List<ZString> { HostBusinessEntity.JK_RL_NKLoadPort.Left(2), HostBusinessEntity.JK_RL_NKDischargePort.Left(2) };
				foreach (Transport leg in HostBusinessEntity.Transports)
				{
					allPossibleConsolCountries.Add(leg.JW_RL_NKLoadPort.Left(2));
					allPossibleConsolCountries.Add(leg.JW_RL_NKDiscPort.Left(2));
				}
				return allPossibleConsolCountries.Distinct();
			}
		}

		const string manifestCountryAttributeValue = "17.3.29.001";
	}
}
