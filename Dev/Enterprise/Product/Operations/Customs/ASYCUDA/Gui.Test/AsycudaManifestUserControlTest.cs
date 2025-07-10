using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaManifestUserControlTest : TestCaseWithFactory
	{
		public void TestVesselCodeFindBoxPopupSelected()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL";
			vessel.RV_LloydsNumber = "9832343";
			vessel.RV_RadioCallSign = "CALLME";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Australia;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new ZForm(header))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var vesselCodeFindBox = control.FindSingle<ZCodeFindBox>("VesselCodeFindBox");
				((IFindBox)vesselCodeFindBox).Code = "VESSEL";
				vesselCodeFindBox.SelectFromPopupForm(true);
				using (((IFindBox)vesselCodeFindBox).PopupForm)
				{
					CombineAssertions(() =>
					{
						AssertEquals("AMA_VesselName", "VESSEL", header.AMA_VesselName);
						AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
						AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
						AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
					});
				}
			}
		}

		public void TestEnforceOnlyValidManifestTypes()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";

			using (var form = new ManifestForm(header))
			{
				form.Show();

				header.AMA_ManifestType = "MGE";
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				header.AMA_ManifestType = "ASY";
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Manifest Type cannot be set to an invalid value: ASY.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("MGE", header.AMA_ManifestType);
			}
		}

		public void TestEnforceOnlyValidTransportMode()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";

			using (var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header))
			using (var form = new ManifestForm(header))
			{
				form.Show();

				header.AMA_TransportMode = "SEA";
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				header.AMA_TransportMode = "XXX";
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Transport Mode cannot be set to an invalid value: XXX.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("SEA", header.AMA_TransportMode);
			}
		}

		public void TestEnforceOnlyValidSpecificCircumstanceIndicator()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.EUManifest.IAsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";

			using (var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header))
			using (var form = new ManifestForm(header))
			{
				form.Show();

				header.SpecificCircumstanceIndicator = "A";
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				header.SpecificCircumstanceIndicator = "F";
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Specific Circumstance Indicator cannot be set to an invalid value: F.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("A", header.SpecificCircumstanceIndicator);
			}
		}

		public void TestReadOnly_WhenPackMessageStatusIsBlank_ExpectFalse()
		{
			TestReadOnly_Bill(false, "");
		}

		public void TestReadOnly_WhenPackMessageStatusIsNOT_ExpectFalse()
		{
			TestReadOnly_Bill(false, MessageStatusCodeList.Codes.NotSent);
		}

		public void TestReadOnly_WhenPackMessageStatusIsERR_ExpectFalse()
		{
			TestReadOnly_Bill(false, MessageStatusCodeList.Codes.Error);
		}

		public void TestReadOnly_WhenPackMessageStatusIsUPDAndIsSGImport_ExpectTrue()
		{
			TestReadOnly_Bill(true, MessageStatusCodeList.Codes.Updated, ShipmentTypeList.Codes.Import23);
		}

		public void TestReadOnly_WhenPackMessageStatusIsACP_ExpectTrue()
		{
			TestReadOnly_Bill(true, MessageStatusCodeList.Codes.Accepted);
		}

		public void TestReadOnly_WhenPackMessageStatusIsAWA_ExpectTrue()
		{
			TestReadOnly_Bill(true, MessageStatusCodeList.Codes.Awaiting);
		}

		public void TestReadOnly_WhenPackMessageStatusIsSNT_ExpectTrue()
		{
			TestReadOnly_Bill(true, MessageStatusCodeList.Codes.Sent);
		}

		public void TestReadOnly_WhenPackMessageStatusIsUNK_ExpectTrue()
		{
			TestReadOnly_Bill(true, MessageStatusCodeList.Codes.Unknown);
		}

		public void TestCreateDeclarationMenuItem()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
				helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
				Factory.Save();

				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGE";

				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "BILL1";

				var pack = bill.Packs.AddNew();
				var packedItem = pack.PackedItemForTesting();
				Factory.Save();

				using (var form = new ManifestForm(header))
				{
					var control = (AsycudaManifestUserControl)form.Controls.Find("asycudaManifestUserControl", true).Single();
					form.Show();
					var tabControl = (ZTabControl)control.Controls.Find("mainTabControl", false).Single();
					tabControl.SelectTab("billsAndPacksTabPage");

					var grid = (ZGrid)control.Controls.Find("BillsGrid", true).Single();

					control.SelectAndShowBill(bill.PK);
					AssertEquals(bill, grid.ListManager.GetCurrent());

					var billMenuItem = grid.ContextMenu.MenuItems.FindByText("Create Customs Declaration");
					AssertEquals("CreateDeclarationMenuItem should be shown.", true, billMenuItem.Visible);

					grid.ContextMenu.DoPopup();
					billMenuItem.PerformClick();

					Assert("Declaration Job # should exist.", bill.CustomsJobNumber != "");
					control.LastControllerForTesting.LastShownForm.Dispose();
				}
			}
		}

		public void TestSelectAndShowBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			using (var form = new ManifestForm(header))
			{
				var control = (AsycudaManifestUserControl)form.Controls.Find("asycudaManifestUserControl", true).Single();
				form.Show();
				var tabControl = (ZTabControl)control.Controls.Find("mainTabControl", false).Single();
				AssertNotEquals("billsAndPacksTabPage is not selected.", "billsAndPacksTabPage", tabControl.SelectedTab.Name);

				var grid = (ZGrid)control.Controls.Find("BillsGrid", true).Single();

				control.SelectAndShowBill(bill1.PK);
				AssertEquals("should have selected bill1", bill1, grid.ListManager.GetCurrent());
				AssertEquals("billsAndPacksTabPage is selected", "billsAndPacksTabPage", tabControl.SelectedTab.Name);

				control.SelectAndShowBill(bill2.PK);
				AssertEquals("should have selected bill2", bill2, grid.ListManager.GetCurrent());
				AssertEquals("billsAndPacksTabPage is selected", "billsAndPacksTabPage", tabControl.SelectedTab.Name);
			}
		}

		public void TestSailingUserControlVisible()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;

			using (var frm = new ZForm(header))
			using (var ctr = new AsycudaManifestUserControl())
			{
				frm.Controls.Add(ctr);

				frm.Show();
				Application.DoEvents();

				var sailingUserControl = ctr.Controls.Find("SailingUserControl", true).First();
				AssertEquals("The SailingUserControl should only visible when the manifest is VOCC and transport mode is sea.", false, sailingUserControl.Visible);
			}
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			using (var frm = new ZForm(header))
			using (var ctr = new AsycudaManifestUserControl())
			{
				frm.Controls.Add(ctr);

				frm.Show();
				Application.DoEvents();

				var sailingUserControl = ctr.Controls.Find("SailingUserControl", true).First();
				frm.SetDataBinding(header, "");
				AssertEquals("The SailingUserControl should only visible when the manifest is VOCC and transport mode is sea.", true, sailingUserControl.Visible);

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("The SailingUserControl should only visible when the manifest is VOCC and transport mode is sea.", false, sailingUserControl.Visible);
			}
		}

		public void TestSeaControlVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new ZForm(manifest))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var vesselCodeFindBox = control.Controls.Find("VesselCodeFindBox", true).First();
					var mastersNameTextBox = control.Controls.Find("MastersNameTextBox", true).First();
					var radioCallSignTextBox = control.Controls.Find("RadioCallSignTextBox", true).First();

					AssertEquals(true, vesselCodeFindBox.Visible);
					AssertEquals(true, mastersNameTextBox.Visible);
					AssertEquals(true, radioCallSignTextBox.Visible);

					manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals(false, vesselCodeFindBox.Visible);
					AssertEquals(false, mastersNameTextBox.Visible);
					AssertEquals(false, radioCallSignTextBox.Visible);
				}
			}
		}

		public void TestConveyanceCountryCodeVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Fiji);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			var fj = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, Core.Constants.CountryCodes.Fiji, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var er = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(fj.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			helper.CreateNewOrGetExistingCusCodeListAttribute(er.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Fiji, parent: wcoDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, parent: wcoDataGrouping);
			Factory.Save();

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			using (var form = new ZForm(manifest))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var conveyanceCountryCodeFindBox = control.Controls.Find("ConveyanceCountryCodeFindBox", true).First();
				AssertEquals("ConveyanceCountryCode is to be visible for both Sea and Air transport", ZBool.True, conveyanceCountryCodeFindBox.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("ConveyanceCountryCode is to be visible for both Sea and Air transport", ZBool.True, conveyanceCountryCodeFindBox.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals(ZBool.False, conveyanceCountryCodeFindBox.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals(ZBool.False, conveyanceCountryCodeFindBox.Visible);
			}
		}

		public void TestBuyersConsolidationVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ZForm(manifest))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var checkBoxBuyersConsolidation = control.Controls.Find("BuyersConsolidationCheckBox", true).FirstOrDefault();

					manifest.AMA_ContainerMode = Core.Constants.ContainerModes.Empty;
					AssertEquals(false, checkBoxBuyersConsolidation.Visible);

					manifest.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
					AssertEquals(false, checkBoxBuyersConsolidation.Visible);

					manifest.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
					AssertEquals(true, checkBoxBuyersConsolidation.Visible);
				}
			}
		}

		public void TestBbbContainersTabVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			var manifest = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ParentId = consol.PK;
			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifest.AMA_ManifestType = "COM";

			using (var form = new ZForm(manifest))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var containersTabpage = control.Controls.Find("containersTabPage", true).FirstOrDefault() as ZTabPage;
					Assert(containersTabpage.TabVisible);

					manifest.AMA_ManifestType = "BBB";
					Assert(!containersTabpage.TabVisible);
					manifest.AMA_ManifestType = "COM";
				}
			}

			manifest.AMA_ParentId = ZGuid.Empty;
			using (var form = new ZForm(manifest))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var containersTabpage = control.Controls.Find("containersTabPage", true).FirstOrDefault() as ZTabPage;
					Assert(containersTabpage.TabVisible);

					manifest.AMA_ManifestType = "BBB";
					AssertEquals(false, containersTabpage.TabVisible);
				}
			}
		}

		public void TestArrivalTabVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SouthAfrica, "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var manifest = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ManifestType = "COH";

			using (var form = new ZForm(manifest))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var arrivalTab = control.Controls.Find("arrivalTabPage", false).FirstOrDefault() as ZTabPage;
					AssertNull(arrivalTab);
				}
			}
		}

		public void TestEclBillsVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SouthAfrica, "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			var manifest = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifest.AMA_ManifestType = "COM";
			manifest.AMA_ParentId = consol.PK;

			using (var form = new ZForm(manifest))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var billsAndPacksTabPage = control.Controls.Find("billsAndPacksTabPage", true).FirstOrDefault() as ZTabPage;
					Assert(billsAndPacksTabPage.TabVisible);

					manifest.AMA_ManifestType = "ECL";
					Assert(!billsAndPacksTabPage.TabVisible);
					manifest.AMA_ManifestType = "COM";
				}
			}

			manifest.AMA_ParentId = ZGuid.Empty;
			using (var form = new ZForm(manifest))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var billsAndPacksTabPage = control.Controls.Find("billsAndPacksTabPage", true).FirstOrDefault() as ZTabPage;
					Assert(billsAndPacksTabPage.TabVisible);

					manifest.AMA_ManifestType = "ECL";
					AssertEquals(false, billsAndPacksTabPage.TabVisible);
					manifest.AMA_ManifestType = "COM";
				}
			}
		}

		public void TestContainersTabIsHiddenWhenTransportIsAir()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ZForm(manifest))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var containerModeDropEdit = control.FindSingle<ZDropEdit>(nameof(CommonManifestControlBag.ContainerModeDropEdit));
				var containersTabPage = control.Controls.Find("containersTabPage", true).FirstOrDefault() as ZTabPage;

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals(true, containerModeDropEdit.Visible);
				AssertEquals(true, containersTabPage.TabVisible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("dropEditContainerMode should not be visible for Air Transport jobs", false, containerModeDropEdit.Visible);
				AssertEquals("Containers grid should not be visible for Air", false, containersTabPage.TabVisible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals(true, containerModeDropEdit.Visible);
				AssertEquals(true, containersTabPage.TabVisible);
			}
		}

		public void TestPersonsGridColumsDisplay()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
			using (var form = new ZForm(manifest))
			using (var control = new AsycudaManifestUserControl())
			{
				var personsGrid = control.Controls.Find("personsGrid", true).First() as ZGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Passenger", true, personsGrid.GetColumnStyle(CusPerson.Schema.CPN_IsPassenger).IsVisible);
					AssertEquals("Person", true, personsGrid.GetColumnStyle(CusPerson.Schema.CPN_PER_Person).IsVisible);
					AssertEquals("PersonFullName", true, personsGrid.GetColumnStyle(CusPerson.Schema.PersonFullName).IsVisible);
					AssertEquals("PersonGender", true, personsGrid.GetColumnStyle(CusPerson.Schema.PersonGender).IsVisible);
					AssertEquals("PersonBirthDate", true, personsGrid.GetColumnStyle(CusPerson.Schema.PersonBirthDate).IsVisible);
					AssertEquals("PersonCountry", true, personsGrid.GetColumnStyle(CusPerson.Schema.PersonCountry).IsVisible);
					AssertEquals("PersonNationality", true, personsGrid.GetColumnStyle(CusPerson.Schema.PersonNationality).IsVisible);
					AssertEquals("PersonDriversLicenseNumber", true, personsGrid.GetColumnStyle(CusPerson.Schema.PersonIdentificationNumber).IsVisible);

					var personPassportColumnStyle = personsGrid.GetColumnStyle(CusPerson.Schema.PersonPassport);
					AssertEquals("PersonPassport", true, personPassportColumnStyle.IsVisible);
					AssertEquals("PersonPassport Group Key", "f99cb528-98bd-4cdf-89cd-e3f20013832b", personPassportColumnStyle.GroupName.Key);

					var personPassportPlaceOfIssueColumnStyle = personsGrid.GetColumnStyle(CusPerson.Schema.PersonPassportPlaceOfIssue);
					AssertEquals("PersonPassportPlaceOfIssue", true, personPassportPlaceOfIssueColumnStyle.IsVisible);
					AssertEquals("PersonPassportPlaceOfIssue Group Key", "f99cb528-98bd-4cdf-89cd-e3f20013832b", personPassportPlaceOfIssueColumnStyle.GroupName.Key);

					var personPassportExpiryColumnStyle = personsGrid.GetColumnStyle(CusPerson.Schema.PersonPassportExpiry);
					AssertEquals("PersonPassportExpiry", true, personPassportExpiryColumnStyle.IsVisible);
					AssertEquals("PersonPassportExpiry Group Key", "f99cb528-98bd-4cdf-89cd-e3f20013832b", personPassportExpiryColumnStyle.GroupName.Key);
				});
			}
		}

		public void TestColumnVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new ZForm(manifest))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					AssertEquals(false, control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.DiscountValue).IsVisible);
					AssertEquals(false, control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.DiscountValueCurrency).IsVisible);

					AssertEquals(false, control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.OtherChargesValue).IsVisible);
					AssertEquals(false, control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.OtherChargesValueCurrency).IsVisible);

					Assert($"{AsycudaBill.Schema.SellerOrgPK} Should be invisible", !control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.SellerOrgPK).IsVisible);
					Assert($"{AsycudaBill.Schema.ABL_OA_Seller} Should be invisible", !control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OA_Seller).IsVisible);
					Assert($"{AsycudaBill.Schema.ABL_SellerName} Should be invisible", !control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SellerName).IsVisible);
					Assert($"{AsycudaBill.Schema.ABL_SellerStreet1} Should be invisible", !control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SellerStreet1).IsVisible);
					Assert($"{AsycudaBill.Schema.ABL_SellerStreet2} Should be invisible", !control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SellerStreet2).IsVisible);
					Assert($"{AsycudaBill.Schema.ABL_SellerCity} Should be invisible", !control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SellerCity).IsVisible);
					Assert($"{AsycudaBill.Schema.ABL_SellerState} Should be invisible", !control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SellerState).IsVisible);
					Assert($"{AsycudaBill.Schema.ABL_SellerPostcode} Should be invisible", !control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SellerPostcode).IsVisible);
					Assert($"{AsycudaBill.Schema.ABL_RN_NKSellerCountry} Should be invisible", !control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RN_NKSellerCountry).IsVisible);
					Assert($"{AsycudaBill.Schema.ABL_SellerPhone} Should be invisible", !control.BillsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SellerPhone).IsVisible);
				}
			}
		}

		public void TestCustomFieldsTab()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Bills.AddNew();

			Factory.Save();

			using (var form = new ZForm(header))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var customFieldsTab = (ZTabPage)control.Controls.Find("BillCustomTabPage", true).First();
				customFieldsTab.Select();

				var customFieldsControl = (ProcessTemplateCustomFieldsControl)customFieldsTab.Controls.Find("CustomFieldsControl", true).First();
				AssertNotNull(customFieldsControl.Visible);
			}
		}

		public void TestOverrideFreightDefaultsVisibility()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			header.AMA_ParentId = ZGuid.Empty;
			header.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;

			using (var frm = new ZForm(header))
			{
				var control = new AsycudaManifestUserControl();
				frm.Controls.Add(control);

				frm.Show();
				Application.DoEvents();

				var checkBox = control.Controls.Find("OverrideFreightDefaults", true).First();
				Assert("Should default to false.", !checkBox.Visible);

				var consol = Factory.New<ForwardingConsol>();
				header.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
				header.AMA_ParentId = consol.PK;

				Assert("Should be true as the consol is not null.", checkBox.Visible);

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				var sailing = voyage.Sailings.AddNew();

				header.ChangeSailing(sailing.PK);

				Assert("Should be true as the sailing is not null and the transport mode is sea.", checkBox.Visible);

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;

				Assert("Should be false as the consol is null and the transport mode is not sea.", !checkBox.Visible);
			}
		}

		public void TestOverrideFreightDefaultsCaption()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");

			using (var frm = new ZForm(header))
			{
				var control = new AsycudaManifestUserControl();
				frm.Controls.Add(control);

				frm.Show();
				Application.DoEvents();

				var checkBox = (ZCheckBox)control.Controls.Find("OverrideFreightDefaults", true).First();
				Assert("Precondition.", header.IsStandAlone);

				var expectedCaption = "Override Default Values from Sailing";
				var expectedFullDesc = "Override Default Values from Sailing?  Tick this box to ignore the values brought through from the sailing, replacing them with your own.";
				AssertEquals("The caption is reference to sailing and it can not link to a consolidation.", expectedCaption, checkBox.CaptionResourceString.Caption);
				AssertEquals("The caption is reference to sailing and it can not link to a consolidation.", expectedFullDesc, checkBox.CaptionResourceString.FullDescription);
			}

			var consol = Factory.New<ForwardingConsol>();
			header.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
			header.AMA_ParentId = consol.PK;
			using (var frm = new ZForm(header))
			{
				var control = new AsycudaManifestUserControl();
				frm.Controls.Add(control);

				frm.Show();
				Application.DoEvents();

				var checkBox = (ZCheckBox)control.Controls.Find("OverrideFreightDefaults", true).First();
				Assert("Precondition.", !header.IsStandAlone);

				var expectedCaption = "Override Default Values from Consol";
				var expectedFullDesc = "Override Default Values from Consol/Shipment?  Tick this box to ignore the values brought through from the consol and shipment, replacing them with your own.";

				AssertEquals("The caption is reference to consol and it can not link to a sailing.", expectedCaption, checkBox.CaptionResourceString.Caption);
				AssertEquals("The caption is reference to consol and it can not link to a sailing.", expectedFullDesc, checkBox.CaptionResourceString.FullDescription);
			}
		}

		public void TestSealAndSealType3AreNotVisibleByDefault()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new ZForm(manifest))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var grid = (ZGrid)control.Controls.Find("ContainersGrid", true).First();
				Assert(!grid.GetColumnStyle(AsycudaContainer.Schema.ACN_Seal3).IsVisible);
				Assert(!grid.GetColumnStyle(AsycudaContainer.Schema.ACN_SealType3).IsVisible);
			}
		}

		public void TestAMA_TrailerRegNoVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			using (var form = new ZForm(manifest))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var ama_Trailer1RegNoTextBox = control.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.Trailer1RegNoTextBox));
				var ama_Trailer2RegNoTextBox = control.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.Trailer2RegNoTextBox));

				AssertEquals(false, ama_Trailer1RegNoTextBox.Visible);
				AssertEquals(false, ama_Trailer2RegNoTextBox.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals(false, ama_Trailer1RegNoTextBox.Visible);
				AssertEquals(false, ama_Trailer2RegNoTextBox.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals(true, ama_Trailer1RegNoTextBox.Visible);
				AssertEquals(true, ama_Trailer2RegNoTextBox.Visible);
			}
		}

		public void TestAMA_RN_NKTrailerRegCountryVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new ZForm(manifest))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var ama_RN_NKTrailer1RegCountryFindBox = control.FindSingle<ZCodeFindBox>(nameof(CommonManifestControlBag.Trailer1RegCountryCodeFindBox));
					var ama_RN_NKTrailer2RegCountryFindBox = control.FindSingle<ZCodeFindBox>(nameof(CommonManifestControlBag.Trailer2RegCountryCodeFindBox));

					AssertEquals(false, ama_RN_NKTrailer1RegCountryFindBox.Visible);
					AssertEquals(false, ama_RN_NKTrailer1RegCountryFindBox.Visible);

					manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals(false, ama_RN_NKTrailer1RegCountryFindBox.Visible);
					AssertEquals(false, ama_RN_NKTrailer1RegCountryFindBox.Visible);

					manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
					AssertEquals(true, ama_RN_NKTrailer1RegCountryFindBox.Visible);
					AssertEquals(true, ama_RN_NKTrailer1RegCountryFindBox.Visible);
				}
			}
		}

		public void TestAdditionalAsycudaManifestHeaderVisibility()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "CIKONC", ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_Nature = "EXP";
			header.AMA_TransportMode = "SEA";
			using (var form = new ZForm(header))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var mainTabControl = (ZTabControl)control.Controls.Find("mainTabControl", true).First();
					var additionalTabPage = (ZTabPage)mainTabControl.Controls.Find("mainTabControl_TabPage_VisitedPortsForManifestHeaderUserControl", true).FirstOrDefault();
					AssertEquals(true, additionalTabPage.TabVisible);
				}
			}

			header.AMA_ManifestType = "ATAITH";
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			using (var form = new ZForm(header))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var mainTabControl = (ZTabControl)control.Controls.Find("mainTabControl", true).First();
					var additionalTabPage = (ZTabPage)mainTabControl.Controls.Find("mainTabControl_TabPage_VisitedPortsForManifestHeaderUserControl", true).FirstOrDefault();
					AssertNull(additionalTabPage);
				}
			}

			var header2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			using (var form = new ZForm(header2))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var mainTabControl = (ZTabControl)control.Controls.Find("mainTabControl", true).First();
				var isAdditionalTabPageExists = mainTabControl.Controls.ContainsKey("mainTabControl_TabPage_VisitedPortsForManifestHeaderUserControl");
				AssertEquals(false, isAdditionalTabPageExists);
			}
		}

		public void TestBillAdditionalTabPageVisibility()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "DENIHR");
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				var bill = header.Bills.AddNew();
				var control = new AsycudaManifestUserControl();
				using (var form = new ManifestForm(header))
				{
					form.Controls.Add(control);
					form.Show();
					var billAdditionalTabPage = (ZTabPage)control.Controls.Find("billsAndPacksTabControl_TabPage_TRBillAdditionalUserControl", true).First();
					Assert(billAdditionalTabPage.TabVisible);

					header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
					Assert(!billAdditionalTabPage.TabVisible);

					header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
					Assert(billAdditionalTabPage.TabVisible);
				}
			}
			using (var form = new ZForm(AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, "ICR")))
			{
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var isbillAdditionalTabPage = control.Controls.ContainsKey("billsAndPacksTabControl_TabPage_TRBillAdditionalUserControl");
					AssertEquals(false, isbillAdditionalTabPage);
				}
			}
		}

		public void TestSetMainTabPageGroupBoxCaption()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ZForm(header))
			{
				var control = new AsycudaManifestUserControl();
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				ZGroupBox manifestGroupBox = (control.Controls.Find("ManifestGroupBox", true)[0]) as ZGroupBox;
				AssertEquals(NoResourceStringData.GetData("Manifest"), manifestGroupBox.CaptionResourceString);
			}
		}

		public void TestSplitContainerForBillsAndPacksPanel2MinSize()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ZForm(header))
			{
				var control = new AsycudaManifestUserControl();
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var splitContainerForBillsAndPacks = (KSplitContainer)control.Controls.Find("splitContainerForBillsAndPacks", true).First();
				AssertEquals(350, splitContainerForBillsAndPacks.Panel2MinSize);
			}
		}

		void TestReadOnly_Bill(bool expectedReadOnly, ZString messageStatus, string shipmentType = null)
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			header.AMA_ManifestType = "MGI";

			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			if (shipmentType != null)
			{
				bill.ABL_ShipmentType = shipmentType;
			}

			var packedItem = pack.PackedItemForTesting();
			if (!messageStatus.IsEmpty)
			{
				packedItem.API_MessageStatus = messageStatus;
			}

			using (var form = new ManifestForm(header))
			{
				var control = (AsycudaManifestUserControl)form.Controls.Find("asycudaManifestUserControl", true).Single();
				form.Show();

				var tabControl = (ZTabControl)control.Controls.Find("mainTabControl", false).Single();
				tabControl.SelectTab("billsAndPacksTabPage");

				if (expectedReadOnly)
				{
					CombineAssertions(() =>
					{
						foreach (ZPropertyInfo propInfo in bill.ZPropertyInfoHash)
						{
							AssertEquals(propInfo.Name, propInfo.Name != bill.ABL_RemarksInfo.Name, propInfo.ReadOnly);
						}
					});
				}
				else
				{
					AssertEquals(expectedReadOnly, bill.ReadOnly);
					AssertEquals(expectedReadOnly, bill.ABL_RemarksInfo.ReadOnly);
				}
			}
		}

		public void TestTransportTabPage()
		{
			using (var control = new AsycudaManifestUserControl())
			{
				var transportTabPage = control.FindSingle<ZTabPage>("transportTabPage");

				AssertNotNull(transportTabPage);

				AssertEquals(1, transportTabPage.Controls.Count);
				var transportGrid = transportTabPage.Controls[0] as ZGrid;
				AssertNotNull(transportGrid);
				AssertEquals("Transport Means", transportTabPage.CaptionResourceString.Caption);

				var columns = transportGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				AssertContainsExactElementsInExactOrder("Transport Grid Column Names",
					new[]
					{
						"JW_LegOrder",
						"JW_ATD",
						"JW_ETA",
						"JW_RL_NKDiscPort",
						"JW_Vessel",
						"VehicleCountry",
						"TruckKind",
					},
					columns.Select(x => x.ColumnName));
			}
		}

		public void TestSetTransportTabPageVisibility_WhenShowTransportMeansTabIsTrue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Israel))
			using (ObjectFactory.Get<Integration.Customs.IL.IILCustomsDataRegistry>().ILEnableILManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				var manifest = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Israel, "785");

				using (var form = new ZForm(manifest))
				using (var control = new AsycudaManifestUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
					Assert("[PRE-CONDITION]", manifest.ShowTransportMeansTab);
					var transportTabPage = control.FindSingle<ZTabPage>("transportTabPage");

					AssertNotNull(transportTabPage);
					Assert("When Is Road", transportTabPage.TabVisible);

					manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
					Assert("[PRE-CONDITION]", !manifest.ShowTransportMeansTab);
					Assert("When Is not Road", !transportTabPage.TabVisible);
				}
			}
		}

		public void TestSetTransportTabPageVisibility_WhenShowTransportMeansTabIsFalse()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Assert("[PRE-CONDITION]", !manifest.ShowTransportMeansTab);

			using (var form = new ZForm(manifest))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
				var transportTabPage = (ZTabPage)control.Controls.Find("transportTabPage", true).FirstOrDefault();
				AssertNull(transportTabPage);
			}
		}

		public void TestSupportMultipleResourceStringData()
		{
			var manifest = Factory.New<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader>();
			using var control = new AsycudaManifestUserControl();
			control.SetDataBinding(manifest, "");
			AssertSame(manifest, control.SupportMultipleResourceStringData);
		}

		public void TestBillsGridCaptions()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ZForm(manifest))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var grid = (ZGrid)control.Controls.Find("BillsGrid", true).First();
				AssertGridColumnCaptions(grid, AsycudaBill.Schema.ABL_OA_Consignee, "CNE Addr.", "Consignee Addr.", "Consignee Address", "Name of the street of the consignee party's address and the number of the building or facility.");
				AssertGridColumnCaptions(grid, AsycudaBill.Schema.ABL_OA_NotifyParty, "NP Addr.", "Notify Party Addr.", "Notify Party Address", "Name of the street of the notify party's address and the number of the building or facility.");
				AssertGridColumnCaptions(grid, AsycudaBill.Schema.ABL_OA_Seller, "Sell. Addr.", "Seller Addr.", "Seller Address", "Name of the street of the seller party's address and the number of the building or facility.");
				AssertGridColumnCaptions(grid, AsycudaBill.Schema.ABL_OA_Shipper, "Ship. Addr.", "Shipper Addr.", "Shipper Address", "Name of the street of the seller party's address and the number of the building or facility.");
			}
		}

		void AssertGridColumnCaptions(ZGrid grid, string columnName, string shortCaption, string mediumCaption, string caption, string fullDescription)
		{
			var column = grid.GetColumnStyle(columnName);
			AssertEquals($"{columnName} Short Caption", shortCaption, column.CaptionResourceString.ShortCaption);
			AssertEquals($"{columnName} Medium Caption", mediumCaption, column.CaptionResourceString.MediumCaption);
			AssertEquals($"{columnName} Caption", caption, column.CaptionResourceString.Caption);
			AssertEquals($"{columnName} Full Description", fullDescription, column.CaptionResourceString.FullDescription);
		}

		public void TestBillMessagesTab_WhenMessageUserControlIsDefault_ShouldDisplayCorrectly()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "COH";
			header.Bills.AddNew();

			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header);
			applicationGUIProvider.ShouldPositionMessagesTabAccordingToMessageLevelForTesting = true;

			AssertBillMessagesTab<MessagesUserControl>(header);
		}

		public void TestBillMessagesTab_WhenMessageUserControlIsCustomized_ShouldDisplayCorrectly()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "COH";
			header.Bills.AddNew();

			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header);
			applicationGUIProvider.ShouldPositionMessagesTabAccordingToMessageLevelForTesting = true;
			applicationGUIProvider.GetBillMessagesUserControlForTesting = () => new MessagesUserControlForTesting();

			AssertBillMessagesTab<MessagesUserControlForTesting>(header);
		}

		void AssertBillMessagesTab<TMessagesUserControl>(AsycudaManifestHeader header)
		{
			using (var form = new ZForm(header))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var mainTabControl = control.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				mainTabControl.SelectTab("billsAndPacksTabPage");

				var billsAndPacksTabControl = form.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var messagesTab = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name.StartsWith("billsAndPacksTabControl_TabPage_MessagesUserControl"));
				billsAndPacksTabControl.SelectTab(messagesTab);

				var messagesUserControl = messagesTab.FindSingle<TMessagesUserControl>("ManifestSpecificProviderUserControl");

				CombineAssertions(() =>
				{
					AssertNotNull(messagesUserControl);
					AssertType<TMessagesUserControl>(messagesUserControl);
				});
			}
		}

		public void TestBillAndPacksTabControl_TabPageOrder()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "COH";
			header.Bills.AddNew();

			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header);
			applicationGUIProvider.ShouldPositionMessagesTabAccordingToMessageLevelForTesting = true;

			var expectedTabPageNamesInOrder = new List<string>()
			{
				"billsTabPage",
				"billPartiesTabPage",
				"BillCustomTabPage",
				"billsAndPacksTabControl_TabPage_MessagesUserControl",
			};

			using (var form = new ZForm(header))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabPages = form.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl").TabPages;
				var actualTabPageNames = tabPages.ToList<ZTabPage>().Select(x => x.Name);

				CombineAssertions(() =>
				{
					AssertEquals("Tab pages match expected number", expectedTabPageNamesInOrder.Count, tabPages.Count);
					AssertContainsExactElementsInExactOrder("Tab pages are in order", expectedTabPageNamesInOrder, actualTabPageNames);
				});
			}
		}

		public void TestMainTabControl_TabPageOrder()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ESH7.IAsycudaManifestHeader>();
			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header);
			var expectedTabPageNamesInOrder = new List<string>()
			{
				"mainTabPage",
				"billsAndPacksTabPage",
				"mainTabControl_TabPage_MessagesUserControl",
			};

			using (var form = new ZForm(header))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var tabPages = form.FindSingle<ZTabControl>(c => c.Name == "mainTabControl").TabPages;
				var actualTabPageNames = tabPages.ToList<ZTabPage>().Select(x => x.Name);

				CombineAssertions(() =>
				{
					AssertEquals("Tab pages match expected number", expectedTabPageNamesInOrder.Count, tabPages.Count);
					AssertContainsExactElementsInExactOrder("Tab pages are in order", expectedTabPageNamesInOrder, actualTabPageNames);
				});
			}
		}

		sealed class MessagesUserControlForTesting : MessagesUserControl { }
	}
}
