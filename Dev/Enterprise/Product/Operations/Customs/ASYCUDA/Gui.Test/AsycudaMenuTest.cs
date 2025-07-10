using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;
using C = Enterprise.Core.Constants.Customs.Universal;
using ManifestValidationRuleCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaMenuTest : TestCaseWithFactory
	{
		public void TestLinkSailingMenu()
		{
			var menuItemName = "Import Bills of Lading linked to the same sailing";

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;

			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();

			var sailing = Factory.New<JobSailing>();
			sailing.JX_JB = destination.PK;
			sailing.JX_JA = origin.PK;

			using (var form = new ManifestForm(header))
			{
				form.Show();
				Application.DoEvents();

				var topMenu = form.Menu.MenuItems.FindByText("Manifest");
				topMenu.ShowPopupMenu();

				var linkSailingMenu = topMenu.MenuItems.FindByName(menuItemName);
				AssertNull(linkSailingMenu);
			}

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;

			using (var form = new ManifestForm(header))
			{
				form.Show();
				Application.DoEvents();

				var topMenu = form.Menu.MenuItems.FindByText("Manifest");

				topMenu.ShowPopupMenu();

				var linkSailingMenu = topMenu.MenuItems.FindByName(menuItemName);
				AssertNotNull(linkSailingMenu);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				linkSailingMenu.PerformClick();

				var message = "There is no Sailing Schedule to import Bills Of Lading.";

				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);

				header.ChangeSailing(sailing.PK);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				linkSailingMenu.PerformClick();
				AssertNotEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRefreshBillDataMenuForStandaloneManifest()
		{
			using (var form = new ZForm())
			using (var menu = new AsycudaMenu(Factory.GetNull<AsycudaManifestHeader>()))
			{
				form.Menu.MenuItems.Add(menu);
				AssertEquals("Manifest", menu.Text);

				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_RN_NKCountry = "ZA";
				menu.SingleManifestHeader = header;
				menu.BuildMenu();
				AssertNull(menu.MenuItems.FindByText("Refresh Bill Data"));
			}
		}

		public void TestRefreshBillDataMenuForConsolLinkedManifest()
		{
			using (var form = new ZForm())
			using (var menu = new AsycudaMenu(Factory.GetNull<AsycudaManifestHeader>()))
			{
				form.Menu.MenuItems.Add(menu);
				AssertEquals("Manifest", menu.Text);

				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_RN_NKCountry = "ZA";
				menu.SingleManifestHeader = header;
				var consol = Factory.New<ForwardingConsol>();
				header.SetParent(consol);
				menu.BuildMenu();
				AssertNotNull(menu.MenuItems[0].MenuItems.FindByText("Refresh Bill Data"));

				header.AMA_OverrideFreightDefaults = false;
				menu.BuildMenu();
				AssertEquals("Refresh bill data should be disabled", false, menu.MenuItems[0].MenuItems.FindByText("Refresh Bill Data").Enabled);

				header.AMA_OverrideFreightDefaults = true;
				menu.BuildMenu();
				AssertEquals("Refresh bill data should be enabled", true, menu.MenuItems[0].MenuItems.FindByText("Refresh Bill Data").Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.MenuItems[0].MenuItems.FindByText("Refresh Bill Data").PerformClick();
				var message = "Existing Bill records will not be overwritten, only additional shipments that are found on parent Consol Job but not found in the list of Bills in the Manifest will be added.";
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);

				header.AMA_OverrideFreightDefaults = false;
				menu.BuildMenu();
				AssertEquals("Refresh bill data should be re-disabled", false, menu.MenuItems[0].MenuItems.FindByText("Refresh Bill Data").Enabled);
			}
		}

		public void TestSendManifest_Caption()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.NVC, "NVC");
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");

			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList("SB", C.RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara Point Cruz Seaport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "SEA", "SBHIR");

			var vuVSEA = helper.CreateNewOrGetExistingCusCodeList("VU", C.RefCusCodeListTypes.Codes.CustomsOffice, "VSEA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVSEA.PK, "SEA", "VUVLI");

			Factory.Save();

			using (var form = new ZForm())
			using (var menu = new AsycudaMenu(Factory.GetNull<AsycudaManifestHeader>()))
			{
				form.Menu.MenuItems.Add(menu);

				AssertEquals("Manifest", menu.Text);
				AssertEquals("The system should always have one empty menu so that the OnPopup to work", 1, menu.MenuItems.Count);
				AssertEquals("-", menu.MenuItems[0].Text);
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(1, menu.MenuItems.Count);
				AssertEquals("Send &Manifest", menu.MenuItems[0].Text);

				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_RN_NKCountry = "SB";

				menu.SingleManifestHeader = header;
				var consol = Factory.New<ForwardingConsol>();
				header.SetParent(consol);
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_RL_NKPortOfLoading = "SBAFT";
				header.AMA_RL_NKPortOfDischarge = "VUVLI";
				menu.BuildMenu();
				AssertEquals(1, menu.MenuItems.Count);

				var solomonIslands = menu.MenuItems[0];
				AssertEquals("ASYCUDA Manifest (&Solomon Islands)", solomonIslands.Text);

				AssertNotNull(solomonIslands.MenuItems.FindByText("Send &Manifest"));

				header.AMA_RL_NKPortOfLoading = ZString.Empty;
				header.AMA_RL_NKPortOfDischarge = ZString.Empty;
				header.AMA_RN_NKCountry = "AU";
				menu.BuildMenu();
				AssertEquals(1, menu.MenuItems.Count);
				AssertEquals("ASYCUDA Manifest (&Australia)", menu.MenuItems[0].Text);
			}

			using (var form = new ZForm())
			using (var menu = new AsycudaMenu(Factory.GetNull<AsycudaManifestHeader>(), Gui.ResString.GetMultilingualString("AsycudaMenu1|MainMenuItem", "Manifest1")))
			{
				form.Menu.MenuItems.Add(menu);
				AssertEquals("Manifest1", menu.Text);
			}
		}

		public void TestCannotSendWithoutEmailAddressExceptZA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Vanuatu, "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SolomonIslands, "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SolomonIslands, C.RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara Point Cruz Seaport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("SEA", "Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.SolomonIslands, C.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "SEA", "SBHIR", false);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var cusCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SolomonIslands, C.RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.CurrentUserEmailAddress, "An email address is required for the current user.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.Mandatory, "Desc.", C.RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SolomonIslands, C.RefCusCodeListTypes.Codes.ManifestValidationRule);
			cusCodeList.Attributes.AddNew(ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SolomonIslands, parent: wcoDataGrouping);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				GlbStaff.CurrentUser.GS_EmailAddress = ZString.Empty;

				var orgHeader = GlbCompany.CurrentCompany.OrgProxy;
				orgHeader.CustomsCodes.AddNew("AGT", "0000", Core.Constants.CountryCodes.SolomonIslands);
				orgHeader.CustomsCodes.AddNew("AGT", "0001", Core.Constants.CountryCodes.SouthAfrica);

				var consol = Factory.New<ForwardingConsol>();
				var headerSB = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
				headerSB.SetParent(consol);
				headerSB.AMA_RN_NKCountry = Core.Constants.CountryCodes.SolomonIslands;
				headerSB.AMA_ManifestType = "ASY";
				headerSB.AMA_CustomsOffice = "Y";
				headerSB.AMA_RL_NKPortOfDischarge = "SBHIR";
				headerSB.AMA_E_ARV = ZDateTime.Now;

				var containerSB = headerSB.Containers.AddNew();
				containerSB.ACN_ContainerNumber = "123";
				var billSB = headerSB.Bills.AddNew();
				billSB.ABL_BillIssuer = "ABL";

				using (var menu = new AsycudaMenuForTest(headerSB))
				{
					using (var form = new ZForm(headerSB))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						menu.MenuItems[0].MenuItems[0].PerformClick();
						AssertContains("Your staff profile requires an email address as this is needed for messaging", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(0, headerSB.Messages.Count);
					}
				}
				Factory.Save();

				var headerZA = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				headerZA.SetParent(consol);
				headerZA.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				headerZA.AMA_TransportMode = Core.Constants.TransportModes.Road;
				headerZA.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
				headerZA.AMA_CustomsOffice = "X";
				headerZA.AMA_RL_NKPortOfDischarge = "ZAHIL";
				headerZA.AMA_E_ARV = ZDateTime.Now;

				var containerZA = headerZA.Containers.AddNew();
				containerZA.ACN_ContainerNumber = "123";
				var billZA = headerZA.Bills.AddNew();
				billZA.ABL_BillIssuer = "ABL";

				using (var menu = new AsycudaMenuForTest(headerZA))
				{
					using (var form = new ZForm(headerZA))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);

						menu.BuildMenu();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						menu.MenuItems[0].MenuItems[0].PerformClick();
						AssertEquals(1, headerZA.Messages.Count);
					}
				}
				Factory.Save();

				var headerVU = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
				headerVU.SetParent(consol);
				headerVU.AMA_RN_NKCountry = Core.Constants.CountryCodes.Vanuatu;
				headerVU.AMA_ManifestType = "ASY";
				headerVU.AMA_CustomsOffice = "X";
				headerVU.AMA_RL_NKPortOfDischarge = "VUSON";
				headerVU.AMA_E_ARV = ZDateTime.Now;

				var containerVU = headerVU.Containers.AddNew();
				containerVU.ACN_ContainerNumber = "123";
				var billVU = headerVU.Bills.AddNew();
				billVU.ABL_BillIssuer = "ABL";

				using (var menu = new AsycudaMenuForTest(headerVU))
				{
					using (var form = new ZForm(headerVU))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						menu.BuildMenu();
						GlbStaff.CurrentUser.GS_EmailAddress = "foo@bar.com";
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						menu.MenuItems[0].MenuItems[0].PerformClick();
						AssertEquals(1, headerVU.Messages.Count);
					}
				}
			}
		}

		public void TestSendManifest_NoHeader()
		{
			using (var menu = new AsycudaMenu(Factory.GetNull<AsycudaManifestHeader>()))
			{
				menu.OnPopup(EventArgs.Empty);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.MenuItems[0].PerformClick();
				AssertContains("No valid manifest header exists", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
				helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
				Factory.Save();

				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGE";

				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillNumber = "BILL1";
				var bill2 = header.Bills.AddNew();
				bill2.ABL_BillNumber = "BILL2";
				var pack = bill1.Packs.AddNew();
				var packedItem = pack.PackedItemForTesting();
				Factory.Save();

				using (var menu = new AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
						{
							var dialog = (AsycudaItemSelectionDialog)obj;
							dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == bill1.PK);
						});
						AssertEquals("Create/Update Declarations from the Bill (Singapore)", menu.MenuItems[2].Text);
						menu.MenuItems[2].PerformClick();
						AssertContains("Declaration has been created", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestCreateDeclarationWithMutex()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
				helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
				Factory.Save();

				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGE";

				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillNumber = "BILL1";
				var bill2 = header.Bills.AddNew();
				bill2.ABL_BillNumber = "BILL2";
				var pack = bill1.Packs.AddNew();
				var packedItem = pack.PackedItemForTesting();
				Factory.Save();

				var factory = new BusinessObjectFactory();
				var newBill2 = factory.Load<AsycudaBill>(bill2.PK);

				using (var menu = new AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

						var menuItem = menu.MenuItems[2];
						AssertEquals("Create/Update Declarations from the Bill (Singapore)", menuItem.Text);

						try
						{
							newBill2.SendToManifestMutex.Lock();
							Assert(!bill1.SendToManifestMutex.IsLocked);
							Assert(bill2.SendToManifestMutex.IsLocked);

							UnitTestUserNotification.Instance.ClearMessages();
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
							menuItem.PerformClick();
							AssertContains("Declarations are currently being created/updated by", UnitTestUserNotification.Instance.LastMessage.Text);
							Assert(!bill1.SendToManifestMutex.IsLocked);
							Assert(bill2.SendToManifestMutex.IsLocked);
						}
						finally
						{
							if (newBill2.SendToManifestMutex.HasLock)
							{
								newBill2.SendToManifestMutex.Unlock();
							}
						}
					}
				}
			}
		}

		public void TestCreateDeclarationWhenNoBills()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
				helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
				Factory.Save();

				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGE";
				Factory.Save();

				using (var menu = new AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						AssertEquals("Create/Update Declarations from the Bill (Singapore)", menu.MenuItems[2].Text);
						menu.MenuItems[2].PerformClick();
						AssertContains("There are no bills that have a bill number", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestUpdateDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
				helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
				Factory.Save();

				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGE";

				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillNumber = "BILL1";
				var bill2 = header.Bills.AddNew();
				bill2.ABL_BillNumber = "BILL2";
				var pack = bill1.Packs.AddNew();
				var packedItem = pack.PackedItemForTesting();
				Factory.Save();

				AssertEquals("CustomsJobNumber", ZString.Empty, bill1.CustomsJobNumber);

				using (var menu = new AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
						{
							var dialog = (AsycudaItemSelectionDialog)obj;
							dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == bill1.PK);
						});
						AssertEquals("Create/Update Declarations from the Bill (Singapore)", menu.MenuItems[2].Text);
						menu.MenuItems[2].PerformClick();
						AssertContains("First Click: go create", "Declaration has been created/updated", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertNotNullOrEmpty("Fill in CustomsJobNumber", bill1.CustomsJobNumber);

						bill1.ABL_BillNumber = "BILL123";
						Factory.Save();
						menu.MenuItems[2].PerformClick();
						AssertContains("Second Click: go update", "Declaration has been created/updated", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestCanNotUpdateDeclarationWhenDeclarationSent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
				helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
				Factory.Save();

				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGE";

				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillNumber = "BILL1";
				var bill2 = header.Bills.AddNew();
				bill2.ABL_BillNumber = "BILL2";
				var pack = bill1.Packs.AddNew();
				var packedItem = pack.PackedItemForTesting();
				Factory.Save();

				AssertEquals("CustomsJobNumber", ZString.Empty, bill1.CustomsJobNumber);

				using (var menu = new AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
						{
							var dialog = (AsycudaItemSelectionDialog)obj;
							dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == bill1.PK);
						});
						AssertEquals("Create/Update Declarations from the Bill (Singapore)", menu.MenuItems[2].Text);
						menu.MenuItems[2].PerformClick();
						AssertContains("First Click: go create", "Declaration has been created/updated", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertNotNullOrEmpty("Fill in CustomsJobNumber", bill1.CustomsJobNumber);

						var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, bill1.CustomsJobNumber));
						var message = declaration.Messages.AddNew();
						message.EM_ReceiveTransmit = "RCV"; // to make GetMessageReferenceNumber STFU
						message.EM_LinkUniqueID = declaration.PK;
						message.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;

						bill1.ABL_BillNumber = "BILL123";
						Factory.Save();
						menu.MenuItems[2].PerformClick();
						AssertContains("Cannot Update Declaration When Declaration has been sent", "Declaration could not be created/updated from Bill", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestManifestHeaderWrapperMenuBuild()
		{
			var consol = Factory.New<ForwardingConsol>();
			var sgHeader = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			sgHeader.AMA_RL_NKPortOfLoading = "SGSIN";
			var vuHeader = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			vuHeader.AMA_RL_NKPortOfLoading = "VUAWD";
			var zaHeader = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			zaHeader.AMA_RL_NKPortOfLoading = "ZAAMZ";

			foreach (var header in new AsycudaManifestHeader[] { sgHeader, vuHeader, zaHeader })
			{
				header.SetParent(consol);
				for (var a = 0; a < 4; a++)
				{
					for (var b = 0; b < 2; b++)
					{
						var bill = header.Bills.AddNew();
						var pack = bill.Packs.AddNew();
						_ = pack.PackedItemForTesting();
						_ = bill.BillScreenings.AddNew();
					}
				}
			}
			Factory.Save();

			//3 headers so 3 hits for each child table.
			var expectedHitCounts = new Dictionary<string, int>()
			{
				{ AsycudaBillSchema.Constants.TableName, 4 },
				{ AsycudaContainerBillOrPackageLinkSchema.Constants.TableName, 1 },
				{ AsycudaContainerSchema.Constants.TableName, 1 },
				{ AsycudaPackedItemSchema.Constants.TableName, 2 },
				{ AsycudaPackPackedItemPivotSchema.Constants.TableName, 1 },
				{ AsycudaPackSchema.Constants.TableName, 1 },
				{ CusEntryNumSchema.Constants.TableName, 17 },
				{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ RefDataGroupingSchema.Constants.TableName, 2 },
				{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 1 }
			};

			var newFactory = new BusinessObjectFactory();
			newFactory.EnableTableHitQueryCollection(new[] { CusEntryNumSchema.Constants.TableName });
			var consolReloaded = newFactory.Load<ForwardingConsol>(consol.PK);
			consolReloaded.Factory.DropHints();

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAManifestCaseNumbers,
							Core.Constants.CountryCodes.SouthAfrica, ZDate.Today, true))
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var menu = new AsycudaMenu(new ManifestHeadersWrapper(consolReloaded)))
			{
				using (AssertDbHitsForAllFactories("LoadChildEditableObjects FetchStrategy should not result in multiple DB hits per table.", expectedHitCounts, true, false, 1))
				{
					form.Menu.MenuItems.Add(menu);
					menu.BuildMenu();
				}
				CombineAssertions(() =>
				{
					AssertNotNull(menu.MenuItems.FindByText("SG Access").MenuItems.FindByText("Send &Manifest"));
					AssertNotNull(menu.MenuItems.FindByText("ASYCUDA Manifest (&Vanuatu)").MenuItems.FindByText("Send &Manifest"));
					AssertNotNull(menu.MenuItems.FindByText("ZA Manifest").MenuItems.FindByText("Send &Manifest"));
					AssertNotNull(menu.MenuItems.FindByText("ZA Manifest").MenuItems.FindByText("Send Supporting &Documents"));
					AssertNotNull(menu.MenuItems.FindByText("ZA Manifest").MenuItems.FindByText("Refresh UCR and LRN data"));
				});
			}
		}

		public void TestFetchForLoadChildEditableObjectsOnlyWhenHeaderIsPackedItemLevel()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "ZA";
			Factory.Save();

			using (var form = new ZForm())
			using (var menu = new AsycudaMenu(Factory.GetNull<AsycudaManifestHeader>()))
			{
				form.Menu.MenuItems.Add(menu);
				menu.SingleManifestHeader = header;
				menu.BuildMenu();
				Assert(!header.IsPackedItemLevelManifestType);
				AssertEquals(0, Factory.ActiveTableFetchHints);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbStaff.CurrentUser.GS_EmailAddress = "foo@bar.com";
		}

		sealed class AsycudaMenuForTest : AsycudaMenu
		{
			public AsycudaMenuForTest(AsycudaManifestHeader header)
			: base(header)
			{ }

			protected override DialogResult ShowSaveDialog(ZSaveFileDialog dialog) => DialogResult.OK;
		}
	}
}
