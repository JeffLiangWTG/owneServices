using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using C = Enterprise.Core.Constants.Customs.Universal;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(AsycudaModule))]
	sealed class AsycudaModuleTest : ASYCUDAManifestBillModuleAbstractTest
	{
		public void TestDefaultMenuItem()
		{
			ApplicationBusinessProvider BuildMockProvider(string countryCode, string description)
			{
				var mockProvider = new Mock<ApplicationBusinessProvider>();
				mockProvider.Protected()
					.Setup<IReadOnlyList<ZString>>("ApplicableCountryCodesCore", ItExpr.IsAny<Directions>(), ItExpr.IsAny<ZString>(), ItExpr.IsAny<string>())
					.Returns(new ReadOnlyCollection<ZString>(new List<ZString>() { countryCode }));
				mockProvider.Setup(m => m.GetManifestDescriptions(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<IEnumerable<ZString>>(),
						It.IsAny<System.Func<IManifestType, bool>>()))
					.Returns(new List<(ZString CountryCode, ZString Description)>() { (countryCode, description) });
				return mockProvider.Object;
			}

			var providers = new List<ApplicationBusinessProvider>()
			{
				BuildMockProvider("US", "United States - Air AMS"),
				BuildMockProvider("GB", "GB Menu Test 001"),
				BuildMockProvider("GB", "GB Menu Test 002"),
			};

			var resourceStringData = new ResourceStringData("AsycudaModuleGrid.NewVOC", "&New Carrier Manifest");
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedStates))
			using (var module = new AsycudaModuleForTest())
			{
				var testMenuItem = module.GetNewMenuItemForTest(resourceStringData, providers, "VOC");
				var usMenuItems = testMenuItem.MenuItems.Cast<MenuItem>().Where(x => x.Text == "United States - Air AMS").ToList();
				AssertEquals("Should have 2 same US menus", 2, usMenuItems.Count);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (var module = new AsycudaModuleForTest())
			{
				var testMenuItem = module.GetNewMenuItemForTest(resourceStringData, providers, "VOC");
				var gbMenuItems = testMenuItem.MenuItems.Cast<MenuItem>().Where(x => x.Text == "United Kingdom").ToList();
				AssertEquals("Should have 2 GB menus", 2, gbMenuItems.Count);
				AssertEquals("Should have 2 GB menuitems", 2, gbMenuItems[0].MenuItems.Count);
				AssertEquals("Should have 2 GB menuitems", 2, gbMenuItems[1].MenuItems.Count);
			}
		}

		public void TestDuplicatedMenuItem()
		{
			ApplicationBusinessProvider BuildMockProvider(string countryCode, string description)
			{
				var mockProvider = new Mock<ApplicationBusinessProvider>();
				mockProvider.Protected()
					.Setup<IReadOnlyList<ZString>>("ApplicableCountryCodesCore", ItExpr.IsAny<Directions>(), ItExpr.IsAny<ZString>(), ItExpr.IsAny<string>())
					.Returns(new ReadOnlyCollection<ZString>(new List<ZString>() { countryCode }));
				mockProvider.Setup(m => m.GetManifestDescriptions(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<IEnumerable<ZString>>(),
						It.IsAny<System.Func<IManifestType, bool>>()))
					.Returns(new List<(ZString CountryCode, ZString Description)>() { (countryCode, description) });
				return mockProvider.Object;
			}

			var providers = new List<ApplicationBusinessProvider>()
			{
				BuildMockProvider("CN", "CN Menu Test 001"),
				BuildMockProvider("SG", "SG Menu Test 001"),
				BuildMockProvider("SG", "SG Menu Test 002"),
				BuildMockProvider("GB", "GB Menu Test 001"),
				BuildMockProvider("GB", "GB Menu Test 002"),
			};

			var resourceStringData = new ResourceStringData("AsycudaModuleGrid.NewVOC", "&New Carrier Manifest");
			using (var module = new AsycudaModuleForTest())
			{
				var testMenuItem = module.GetNewMenuItemForTest(resourceStringData, providers, "VOC");
				AssertEquals("Menu count", 3, testMenuItem.MenuItems.Count);
				AssertNotNull("CN must in menu", testMenuItem.MenuItems.FindByText("CN Menu Test 001"));
				AssertNotNull("SG must in menu", testMenuItem.MenuItems.FindByText("Singapore"));
				AssertNotNull("GB must in menu", testMenuItem.MenuItems.FindByText("United Kingdom"));
			}
		}

		public void TestAllowNew()
		{
			using (var module = new AsycudaModule())
			{
				Assert(!module.AllowNew);
			}
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		[TestDate(2017, 9, 15)]
		public void TestAddNewMenuItemFromTheCountryOfCurrentCompany()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");

			var cgCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.Congo,
				Core.Constants.CountryCodes.Congo,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.NVC, "Desc.", RefCusCodeListTypes.Codes.ManifestCountry, C.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry);
			helper.CreateCusCodeListAttribute(cgCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, string.Empty);

			var egCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.Egypt,
				"Egypt",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.NVC, "Desc.", RefCusCodeListTypes.Codes.ManifestCountry, C.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry);
			helper.CreateCusCodeListAttribute(egCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, string.Empty);
			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Egypt))
			using (var module = new AsycudaModuleForTest())
			{
				GlbBranch.CurrentBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;

				AssertEquals(Core.Constants.CountryCodes.Congo, GlbBranch.CurrentBranch.BaseCountry.Code);
				AssertEquals(Core.Constants.CountryCodes.Egypt, GlbCompany.CurrentCompany.Country.Code);

				var newMenuItem = module.GetNewStandardMenuItems().FindByText("&New Forwarder Manifest");
				AssertNotNull("New menu item", newMenuItem);

				var zaMenuItem = newMenuItem.MenuItems.FindByText("South Africa");
				AssertNotNull("South Africa menu should be found.", zaMenuItem);

				var coMenuItemCount = newMenuItem.MenuItems.Cast<MenuItem>().Count(c => c.Text == "Congo");
				AssertEquals("Should equal to 1 as the current country is EG.", 1, coMenuItemCount);

				var egMenuItemCount = newMenuItem.MenuItems.Cast<MenuItem>().Count(c => c.Text == "Egypt");
				AssertEquals("Should equal to 2 as the current country is EG, and the top item on the drop down list will be the current company (if applicable).", 2, egMenuItemCount);
			}
		}

		[TestDate(2017, 7, 12)]
		public void TestModuleButtonsAndNewForm()
		{
			using (var module = new AsycudaModuleForTest())
			{
				var newMenuItem = module.GetNewStandardMenuItems().FindByText("&New Forwarder Manifest");
				AssertNotNull("New menu item", newMenuItem);

				var zaMenuItem = newMenuItem.MenuItems.FindByText("South Africa");
				AssertNotNull("South Africa menu should be found", zaMenuItem);

				zaMenuItem.PerformClick();

				var controller = module.ControllerForTest as ASYCUDAManifestController;
				AssertNotNull(controller);

				var lastForm = module.ControllerForTest.LastShownForm as ManifestForm;
				AssertNotNull(lastForm);

				module.ControllerForTest.LastShownForm.Dispose();
			}
		}

		[TestDate(2017, 7, 12)]
		public void TestNewForwarderManifestDropDownCountryList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica, "South Africa");

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var bdCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.Bangladesh,
				Core.Constants.CountryCodes.Bangladesh,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.NVC, "Desc.", RefCusCodeListTypes.Codes.ManifestCountry, C.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry);
			helper.CreateCusCodeListAttribute(bdCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, string.Empty);

			helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.Vanuatu,
				Core.Constants.CountryCodes.Vanuatu,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);

			var usCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.UnitedStates,
				Core.Constants.CountryCodes.UnitedStates,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.NVC, "Desc.", RefCusCodeListTypes.Codes.ManifestCountry, C.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry);
			helper.CreateCusCodeListAttribute(usCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, string.Empty);

			var zaCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.SouthAfrica,
				Core.Constants.CountryCodes.SouthAfrica,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.VOC, "Desc.", RefCusCodeListTypes.Codes.ManifestCountry, C.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry);
			helper.CreateCusCodeListAttribute(zaCountry.PK, RefCusCodeListAttributeTypes.Codes.VOC, string.Empty);
			Factory.Save();

			using (var module = new AsycudaModuleForTest())
			{
				var newMenuItem = module.GetNewStandardMenuItems().FindByText("&New Forwarder Manifest");
				AssertNotNull("New menu item", newMenuItem);
				AssertEquals(IconTypes.NewButtonActive, (newMenuItem as ZMenuItem).ActiveIcon);
				AssertEquals(IconTypes.NewButtonRest, (newMenuItem as ZMenuItem).RestIcon);

				var bdMenuItem = newMenuItem.MenuItems.FindByText("Bangladesh");
				AssertNotNull("Bangladesh menu should be found", bdMenuItem);

				bdMenuItem.PerformClick();

				var controller = module.ControllerForTest as ASYCUDAManifestController;
				AssertNotNull(controller);

				var lastForm = module.ControllerForTest.LastShownForm as ManifestForm;
				AssertNotNull(lastForm);

				module.ControllerForTest.LastShownForm.Dispose();
			}
		}

		public void TestForwardingAndCarrierMenuItems_MenuItemsGroupedByCountryCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var gbCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.UnitedKingdom,
				Core.Constants.CountryCodes.UnitedKingdom,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(gbCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, string.Empty);
			helper.CreateCusCodeListAttribute(gbCountry.PK, RefCusCodeListAttributeTypes.Codes.VOC, string.Empty);
			Factory.Save();
			using (var module = new AsycudaModuleForTest())
			{
				var forwardernewMenuItem = module.GetNewStandardMenuItems().FindByText("&New Forwarder Manifest");
				var carrierMenuItem = module.GetNewStandardMenuItems().FindByText("&New Carrier Manifest");
				AssertNotNull("New menu item", forwardernewMenuItem);
				AssertNotNull("New menu item", carrierMenuItem);

				AssertEquals(IconTypes.NewButtonActive, (forwardernewMenuItem as ZMenuItem).ActiveIcon);
				AssertEquals(IconTypes.NewButtonRest, (forwardernewMenuItem as ZMenuItem).RestIcon);

				AssertEquals(IconTypes.NewButtonActive, (carrierMenuItem as ZMenuItem).ActiveIcon);
				AssertEquals(IconTypes.NewButtonRest, (carrierMenuItem as ZMenuItem).RestIcon);

				AssertNullOrEmpty(forwardernewMenuItem.MenuItems.FindByText("Goods vehicle movement system (GVMS)", false)?.Text);
				AssertNullOrEmpty(forwardernewMenuItem.MenuItems.FindByText("Safety and Security Great Britain (S&&S GB)", false)?.Text);
				AssertNullOrEmpty(forwardernewMenuItem.MenuItems.FindByText("Northern Ireland ICS", false)?.Text);

				AssertNullOrEmpty(carrierMenuItem.MenuItems.FindByText("Goods vehicle movement system (GVMS)")?.Text);
				AssertNullOrEmpty(carrierMenuItem.MenuItems.FindByText("Safety and Security Great Britain (S&&S GB)")?.Text);
				AssertNullOrEmpty(carrierMenuItem.MenuItems.FindByText("Northern Ireland ICS")?.Text);

				var forwarderSubMenuItem = forwardernewMenuItem.MenuItems.FindByText("United Kingdom", true).MenuItems;
				AssertNotNull("New menu item", forwarderSubMenuItem);
				Assert("MenuItem should have Nested MenuItems", forwarderSubMenuItem.Count > 0);

				var carrierSubMenuItem = carrierMenuItem.MenuItems.FindByText("United Kingdom", true).MenuItems;
				AssertNotNull("New menu item", carrierSubMenuItem);
				Assert("MenuItem should have Nested MenuItems", carrierSubMenuItem.Count > 0);
				AssertNotNull(carrierMenuItem.MenuItems.FindByText("Goods vehicle movement system (GVMS)", findSubitems: true)?.Text);
				AssertNotNull(carrierMenuItem.MenuItems.FindByText("Northern Ireland ICS", findSubitems: true)?.Text);
				AssertNotNull(carrierMenuItem.MenuItems.FindByText("Safety and Security Great Britain (S&&S GB)", findSubitems: true)?.Text);

				module.Dispose();
			}
		}

		public void TestNewForwarderManifestDropDownCountryList_EntryWithCustomizedDescription()
		{
			using (var module = new AsycudaModuleForTest())
			{
				var newMenuItem = module.GetNewStandardMenuItems().FindByText("&New Forwarder Manifest");
				AssertNotNull("New menu item", newMenuItem);
				AssertEquals(IconTypes.NewButtonActive, (newMenuItem as ZMenuItem).ActiveIcon);
				AssertEquals(IconTypes.NewButtonRest, (newMenuItem as ZMenuItem).RestIcon);

				var usMenuItem = newMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text.Contains("Air AMS"));
				AssertEquals("Has ACEManifest Application provider Manifest Description", "United States - Air AMS (Import)", usMenuItem.Text);
			}
		}

		[TestDate(2017, 7, 12)]
		public void TestNewCarrierManifestMenu()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");

			var bdCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.Bangladesh,
				Core.Constants.CountryCodes.Bangladesh,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.VOC, "Desc.", RefCusCodeListTypes.Codes.ManifestCountry, C.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry);
			helper.CreateCusCodeListAttribute(bdCountry.PK, RefCusCodeListAttributeTypes.Codes.VOC, string.Empty);

			helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.Vanuatu,
				Core.Constants.CountryCodes.Vanuatu,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);

			var usCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.UnitedStates,
				Core.Constants.CountryCodes.UnitedStates,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.VOC, "Desc.", RefCusCodeListTypes.Codes.ManifestCountry, C.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry);
			helper.CreateCusCodeListAttribute(usCountry.PK, RefCusCodeListAttributeTypes.Codes.VOC, string.Empty);

			var zaCountry = helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusCodeListTypes.Codes.ManifestCountry,
				Core.Constants.CountryCodes.SouthAfrica,
				Core.Constants.CountryCodes.SouthAfrica,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.NVC, "Desc.", RefCusCodeListTypes.Codes.ManifestCountry, C.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry);
			helper.CreateCusCodeListAttribute(zaCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, string.Empty);
			Factory.Save();

			using (var module = new AsycudaModuleForTest())
			{
				var newMenuItem = module.GetNewStandardMenuItems().FindByText("&New Carrier Manifest");
				AssertNotNull("New menu item", newMenuItem);
				AssertEquals(IconTypes.NewButtonActive, (newMenuItem as ZMenuItem).ActiveIcon);
				AssertEquals(IconTypes.NewButtonRest, (newMenuItem as ZMenuItem).RestIcon);

				var bdMenuItem = newMenuItem.MenuItems.FindByText("Bangladesh");
				AssertNotNull("Bangladesh menu should be found", bdMenuItem);

				bdMenuItem.PerformClick();

				var controller = module.ControllerForTest as ASYCUDAManifestController;
				AssertNotNull(controller);

				var lastForm = module.ControllerForTest.LastShownForm as ManifestForm;
				AssertNotNull(lastForm);

				module.ControllerForTest.LastShownForm.Dispose();
			}
		}

		public void TestSelectedTabAndBill_StandAlone()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "MAN001";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			Factory.Save();

			var manifestController = AsycudaModule.GetNewControllerFor(header);
			using (var form = manifestController.ShowEditForm(header))
			{
				AsycudaModule.SelectAndShowBill(form, bill1);
				AssertSelectedTabAndBill(form as ManifestForm, bill1);
			}

			using (var form = manifestController.ShowEditForm(header))
			{
				AsycudaModule.SelectAndShowBill(form, bill2);
				AssertSelectedTabAndBill(form as ManifestForm, bill2);
			}
		}

		[TestDate(2017, 9, 15)]
		public void TestSelectedTabAndBill_ConsoleManifest()
		{
			var cusCodeList1 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList1.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry;
			cusCodeList1.ZZD_Code = Core.Constants.CountryCodes.SouthAfrica;
			cusCodeList1.ZZD_Description = "South Africa";
			cusCodeList1.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			cusCodeList1.ZZD_StartDate = new ZDateTime(2017, 7, 1);
			cusCodeList1.ZZD_EndDate = new ZDateTime(2017, 12, 1);

			cusCodeList1.Attributes.AddNew("NVC", string.Empty);

			var cusCodeList2 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList2.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry;
			cusCodeList2.ZZD_Code = Core.Constants.CountryCodes.Fiji;
			cusCodeList2.ZZD_Description = "Fiji";
			cusCodeList2.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			cusCodeList2.ZZD_StartDate = new ZDateTime(2017, 7, 1);
			cusCodeList2.ZZD_EndDate = new ZDateTime(2017, 12, 1);

			cusCodeList2.Attributes.AddNew("NVC", string.Empty);

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ZAAAA";
			consol.JK_RL_NKDischargePort = "FJAAA";
			consol.JK_TransportMode = "SEA";
			var header1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			header1.AMA_OverrideFreightDefaults = true;
			header1.FillWithValidTestData();
			header1.SetParent(consol);
			header1.AMA_ManifestType = "ALH";
			var bill11 = header1.Bills.AddNew();
			var bill12 = header1.Bills.AddNew();

			var header2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Fiji, "ASY");
			header2.AMA_OverrideFreightDefaults = true;
			header2.FillWithValidTestData();
			header2.SetParent(consol);
			header2.AMA_JobReference = "C123457";
			var bill21 = header2.Bills.AddNew();
			var bill22 = header2.Bills.AddNew();

			Factory.Save();

			var manifestController = AsycudaModule.GetNewControllerFor(header1);
			using (var form = manifestController.ShowEditForm(header1))
			{
				AsycudaModule.SelectAndShowBill(form, bill11);
				AssertSelectedTabAndBill(form as ConsolForm, header1.AMA_RN_NKCountry, header1.AMA_ManifestType, bill11);
			}

			using (var form = manifestController.ShowEditForm(header1))
			{
				AsycudaModule.SelectAndShowBill(form, bill12);
				AssertSelectedTabAndBill(form as ConsolForm, header1.AMA_RN_NKCountry, header1.AMA_ManifestType, bill12);
			}

			using (var form = manifestController.ShowEditForm(header2))
			{
				AsycudaModule.SelectAndShowBill(form, bill21);
				AssertSelectedTabAndBill(form as ConsolForm, header2.AMA_RN_NKCountry, header2.AMA_ManifestType, bill21);
			}

			using (var form = manifestController.ShowEditForm(header2))
			{
				AsycudaModule.SelectAndShowBill(form, bill22);
				AssertSelectedTabAndBill(form as ConsolForm, header2.AMA_RN_NKCountry, header2.AMA_ManifestType, bill22);
			}
		}

		public void TestGetNewActionMenuItems()
		{
			const string manifestResponseInterchangeAddActionText = "Add IL Manifest Inbound interchange (CWSupport Only)";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Israel))
			using (var module = GetModule())
			{
				Assert("[Prerequisite] Current user is CWSupport by default.", GlbStaff.CurrentUser.IsSupportUser);

				var actionsMenu = module.FormActionMenu.OfType<ZMenuItem>().FirstOrDefault(s => s.CaptionResourceString != null && s.CaptionResourceString.Key == "ModuleGrid.Actions");
				var menuItem = actionsMenu?.MenuItems.OfType<ZMenuItem>().FirstOrDefault(s => s.Text == manifestResponseInterchangeAddActionText);
				AssertNotNull("The menu item for adding IL Manifest response interchange should be visible when current user is CWSupport and current Country is Israel.", menuItem);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Bangladesh))
			using (var module = GetModule())
			{
				Assert("[Prerequisite] Current user is CWSupport by default.", GlbStaff.CurrentUser.IsSupportUser);

				var actionsMenu = module.FormActionMenu.OfType<ZMenuItem>().FirstOrDefault(s => s.CaptionResourceString != null && s.CaptionResourceString.Key == "ModuleGrid.Actions");
				var menuItem = actionsMenu?.MenuItems.OfType<ZMenuItem>().FirstOrDefault(s => s.Text == manifestResponseInterchangeAddActionText);
				AssertNull("The menu item for adding IL Manifest response interchange should be invisible when current Country is not Israel.", menuItem);
			}

			GlbStaff.CurrentUser.GS_LoginName = "Dummy";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Israel))
			using (var module = GetModule())
			{
				Assert("[Prerequisite] Current user is not CWSupport.", !GlbStaff.CurrentUser.IsSupportUser);

				var actionsMenu = module.FormActionMenu.OfType<ZMenuItem>().FirstOrDefault(s => s.CaptionResourceString != null && s.CaptionResourceString.Key == "ModuleGrid.Actions");
				var menuItem = actionsMenu?.MenuItems.OfType<ZMenuItem>().FirstOrDefault(s => s.Text == manifestResponseInterchangeAddActionText);
				AssertNull("The menu item for adding IL Manifest response interchange should be invisible when current user is not CWSupport.", menuItem);
			}
		}

		public void TestOperationalActionsPlugInAdded()
		{
			using var module = (AsycudaModule)GetModule();
			AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
		}

		public void TestOperationalActionSupporter()
		{
			using var module = (AsycudaModule)GetModule();
			AssertType<AsycudaManifestOperationalActionSupporter>(module.OperationalActionSupporter);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.ASYCUDA.Manifest;

		protected override bool HasController() => true;

		protected override void BashModule(ZFilterModule module)
		{
			var testHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			testHeader.AMA_JobReference = "1";
			Factory.Save();

			base.BashModule(module);
		}

		void AssertSelectedTabAndBill(ManifestForm manifestForm, AsycudaBill bill)
		{
			var control = (AsycudaManifestUserControl)manifestForm.Controls.Find("asycudaManifestUserControl", true).Single();
			var tabControl = (ZTabControl)control.Controls.Find("mainTabControl", true).Single();
			var grid = (ZGrid)control.Controls.Find("BillsGrid", true).Single();
			AssertEquals("billsAndPacksTabPage", tabControl.SelectedTab.Name);
			AssertEquals(bill.PK, grid.GetCurrentPK());
		}

		void AssertSelectedTabAndBill(ConsolForm consolForm, string countryCode, string manifestType, AsycudaBill bill)
		{
			var mainTabPageName = countryCode + manifestType + "TabPage";
			var control = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest).UserControl;
			var mainTabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
			AssertEquals(mainTabPageName, mainTabControl.SelectedTab.Name);

			var mainTabPage = mainTabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == mainTabPageName);
			var userControl = mainTabPage.Controls.OfType<AsycudaManifestUserControl>().First();
			var tabControl = (ZTabControl)userControl.Controls.Find("mainTabControl", true).Single();
			var grid = (ZGrid)tabControl.Controls.Find("BillsGrid", true).Single();
			AssertEquals("billsAndPacksTabPage", tabControl.SelectedTab.Name);
			AssertEquals(bill.PK, grid.GetCurrentPK());
		}

		sealed class AsycudaModuleForTest : AsycudaModule
		{
			protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				ControllerForTest = base.GetNewController(selectedBusinessObject);
				return ControllerForTest;
			}

			internal ZController ControllerForTest;

			public new MenuItem[] GetNewStandardMenuItems() => base.GetNewStandardMenuItems();

			public MenuItem GetNewMenuItemForTest(ResourceStringData menuText, IList<ApplicationBusinessProvider> providers, ZString manifestStyle) => GetNewMenuItem(menuText, (sender, e) => { }, providers, manifestStyle);
		}
	}
}
