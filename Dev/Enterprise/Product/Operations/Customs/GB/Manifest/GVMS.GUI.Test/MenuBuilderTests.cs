using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(EUManifestTypes))]
namespace Enterprise.Customs.GB.GVMS.GUI.Testing
{
	public class MenuBuilderTests : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001: Simplify name", Justification = "Simplification removes MenuItem in ToList<MenuItem> which is leading to build error.")]
		public void TestPopulateReferenceFromConsolMenuExisting()
		{
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				Factory.Save();

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						AssertEquals(false, form.Menu.MenuItems.ToList<MenuItem>().Any(x => x.Text == "Populate from consol"));
					}
				}

				var consol = Factory.New<ForwardingConsol>();
				header.AMA_ParentId = consol.PK;
				header.AMA_ParentTableCode = consol.Prefix;

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						var populateMenu = form.Menu.MenuItems.FindByText("Populate from consol", true);
						AssertNotNull(populateMenu);

						populateMenu.PerformClick();
						AssertEquals(0, header.GvmsCustomsReferenceCollection.Count);
						AssertEquals(0, header.GvmsTransitReferenceCollection.Count);
					}
				}
			}
		}

		public void TestPopulateReferenceFromConsolWithDeclaration()
		{
			var chester = new RefUNLOCO.Loader(Factory).Load("GBCEG");
			if (chester.CountryStates == null)
			{
				var ni = Factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = "ENGLAND";
				chester.RL_RW = ni.PK;
			}

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = ni.PK;
			}
			Factory.Save();

			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

				var consol = Factory.New<ForwardingConsol>();
				header.AMA_ParentId = consol.PK;
				header.AMA_ParentTableCode = consol.Prefix;

				header.AMA_Nature = GVMSManifestNature.Codes.GBtoNI;
				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "GBCEG";
				shipment1.JS_RL_NKDestination = "GBBEL";
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_JS = shipment1.PK;
				declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
				entryHeader1.MovementReferenceNumberSetter("TST1", new ZDateTime(2021, 04, 01));

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_RL_NKOrigin = "GBBEL";
				shipment2.JS_RL_NKDestination = "GBCEG";
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_JS = shipment2.PK;
				declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader2.MovementReferenceNumberSetter("TST2", new ZDateTime(2021, 04, 02));

				var shipment3 = consol.Shipments.AddNew();
				shipment3.JS_RL_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
				shipment3.JS_RL_NKDestination = Core.Constants.CountryCodes.UnitedKingdom;
				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.JE_JS = shipment3.PK;
				declaration3.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
				declaration3.JE_MessageType = "IMP";
				var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				var entryNumber3 = CusEntryNumber.LoadOrCreate(entryHeader3, "IMP", Core.Constants.CountryCodes.UnitedKingdom);
				entryNumber3.CE_EntryNum = "TST3";
				entryNumber3.CE_IssueDate = new ZDateTime(2021, 04, 03);

				var shipment4 = consol.Shipments.AddNew();
				shipment4.JS_RL_NKOrigin = Core.Constants.CountryCodes.UnitedKingdom;
				shipment4.JS_RL_NKDestination = Core.Constants.CountryCodes.UnitedStates;
				var declaration4 = Factory.New<JobDeclaration>();
				declaration4.JE_JS = shipment4.PK;
				declaration4.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
				var entryHeader4 = declaration4.CustomsEntryHeaders.AddNew();
				var entryNumber4 = CusEntryNumber.LoadOrCreate(entryHeader4, "EXP", Core.Constants.CountryCodes.UnitedKingdom);
				entryNumber4.CE_EntryNum = "TST4";
				entryNumber4.CE_IssueDate = new ZDateTime(2021, 04, 04);
				Factory.Save();

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						var populateMenu = form.Menu.MenuItems.FindByText("Populate from consol", true);
						populateMenu.PerformClick();

						AssertEquals(1, header.GvmsCustomsReferenceCollection.Count);
						AssertEquals(0, header.GvmsTransitReferenceCollection.Count);

						var reference = header.GvmsCustomsReferenceCollection.Cast<GvmsItemReference>().FirstOrDefault();
						AssertEquals(GVMSCustomsReference.Codes.CdsMovementReferenceNumberMrn, reference.CSI_Code);
						AssertEquals("TST1", reference.CSI_ReferenceNumber);
						AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, reference.CSI_RN_NKCountryCode);
						AssertEquals(new ZDateTime(2021, 04, 01), reference.CSI_DateOfIssue);
						AssertEquals(GvmsItemReference.SystemStatus, reference.CSI_IssuerType);
					}
				}

				header.GvmsCustomsReferenceCollection.RemoveAndDeleteAll();
				header.AMA_Nature = GVMSManifestNature.Codes.NItoGB;
				Factory.Save();

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						var populateMenu = form.Menu.MenuItems.FindByText("Populate from consol", true);
						populateMenu.PerformClick();

						AssertEquals(1, header.GvmsCustomsReferenceCollection.Count);
						AssertEquals(0, header.GvmsTransitReferenceCollection.Count);

						var reference = header.GvmsCustomsReferenceCollection.Cast<GvmsItemReference>().FirstOrDefault();
						AssertEquals(GVMSCustomsReference.Codes.CdsMovementReferenceNumberMrn, reference.CSI_Code);
						AssertEquals("TST2", reference.CSI_ReferenceNumber);
						AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, reference.CSI_RN_NKCountryCode);
						AssertEquals(new ZDateTime(2021, 04, 02), reference.CSI_DateOfIssue);
						AssertEquals(GvmsItemReference.SystemStatus, reference.CSI_IssuerType);
					}
				}

				header.GvmsCustomsReferenceCollection.RemoveAndDeleteAll();
				header.AMA_Nature = GVMSManifestNature.Codes.Import;
				Factory.Save();

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						var populateMenu = form.Menu.MenuItems.FindByText("Populate from consol", true);
						populateMenu.PerformClick();

						AssertEquals(1, header.GvmsCustomsReferenceCollection.Count);
						AssertEquals(0, header.GvmsTransitReferenceCollection.Count);

						var reference = header.GvmsCustomsReferenceCollection.Cast<GvmsItemReference>().FirstOrDefault();
						AssertEquals(GVMSCustomsReference.Codes.ChiefImportEntryReferenceNumber, reference.CSI_Code);
						AssertEquals("TST3", reference.CSI_ReferenceNumber);
						AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, reference.CSI_RN_NKCountryCode);
						AssertEquals(new ZDateTime(2021, 04, 03), reference.CSI_DateOfIssue);
						AssertEquals(GvmsItemReference.SystemStatus, reference.CSI_IssuerType);
					}
				}

				header.GvmsCustomsReferenceCollection.RemoveAndDeleteAll();
				header.AMA_Nature = GVMSManifestNature.Codes.Export;
				Factory.Save();

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						var populateMenu = form.Menu.MenuItems.FindByText("Populate from consol", true);
						populateMenu.PerformClick();

						AssertEquals(1, header.GvmsCustomsReferenceCollection.Count);
						AssertEquals(0, header.GvmsTransitReferenceCollection.Count);

						var reference = header.GvmsCustomsReferenceCollection.Cast<GvmsItemReference>().FirstOrDefault();
						AssertEquals(GVMSCustomsReference.Codes.ChiefImportEntryReferenceNumber, reference.CSI_Code);
						AssertEquals("TST4", reference.CSI_ReferenceNumber);
						AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, reference.CSI_RN_NKCountryCode);
						AssertEquals(new ZDateTime(2021, 04, 04), reference.CSI_DateOfIssue);
						AssertEquals(GvmsItemReference.SystemStatus, reference.CSI_IssuerType);
					}
				}
			}
		}

		public void TestPopulateReferenceFromConsolWithDeclarationsIsEmpty()
		{
			var chester = new RefUNLOCO.Loader(Factory).Load("GBCEG");
			if (chester.CountryStates == null)
			{
				var ni = Factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = "ENGLAND";
				chester.RL_RW = ni.PK;
			}

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = ni.PK;
			}
			Factory.Save();

			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

				var consol = Factory.New<ForwardingConsol>();
				header.AMA_ParentId = consol.PK;
				header.AMA_ParentTableCode = consol.Prefix;
				header.AMA_Nature = GVMSManifestNature.Codes.GBtoNI;
				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "GBCEG";
				shipment1.JS_RL_NKDestination = "GBBEL";

				Factory.Save();

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						var populateMenu = form.Menu.MenuItems.FindByText("Populate from consol", true);
						populateMenu.PerformClick();

						AssertEquals(0, header.GvmsCustomsReferenceCollection.Count);
						AssertEquals(0, header.GvmsTransitReferenceCollection.Count);
					}
				}
			}
		}

		public void TestPopulateReferenceFromConsolWithNctsHeader()
		{
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

				var consol = Factory.New<ForwardingConsol>();
				header.AMA_ParentId = consol.PK;
				header.AMA_ParentTableCode = consol.Prefix;

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ParentID = consol.PK;
				nctsHeader.BH_ParentTableCode = consol.Prefix;
				nctsHeader.BH_FTZMove = true;
				var entryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				entryNumber.CE_EntryNum = "TST5";
				entryNumber.CE_IssueDate = new ZDateTime(2021, 04, 05);
				Factory.Save();

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						var populateMenu = form.Menu.MenuItems.FindByText("Populate from consol", true);
						populateMenu.PerformClick();

						AssertEquals(0, header.GvmsCustomsReferenceCollection.Count);
						AssertEquals(1, header.GvmsTransitReferenceCollection.Count);

						var reference = header.GvmsTransitReferenceCollection.Cast<GvmsItemReference>().FirstOrDefault();
						AssertEquals(GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber, reference.CSI_Code);
						AssertEquals("TST5", reference.CSI_ReferenceNumber);
						AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, reference.CSI_RN_NKCountryCode);
						AssertEquals(new ZDateTime(2021, 04, 05), reference.CSI_DateOfIssue);
						AssertEquals(GvmsItemReference.SystemStatus, reference.CSI_IssuerType);
					}
				}
			}
		}

		public void TestPopulateReferenceFromConsolWithICSManifest()
		{
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var consol = Factory.New<ForwardingConsol>();

				var icsHeader = Factory.New<ASYCUDA.Business.AsycudaManifestHeader>();
				icsHeader.AMA_ParentId = consol.PK;
				icsHeader.AMA_ParentTableCode = consol.Prefix;
				icsHeader.AMA_ManifestType = EUManifestTypes.Codes.ICS;

				var entryNumber = CusEntryNumber.LoadOrCreate(icsHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.UnitedKingdom);
				entryNumber.CE_EntryNum = "TST6";
				entryNumber.CE_IssueDate = new ZDateTime(2021, 04, 06);

				var gvmsHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				gvmsHeader.AMA_ParentId = consol.PK;
				gvmsHeader.AMA_ParentTableCode = consol.Prefix;

				Factory.Save();

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(gvmsHeader))
				{
					using (var form = new ZForm(gvmsHeader))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						var populateMenu = form.Menu.MenuItems.FindByText("Populate from consol", true);
						populateMenu.PerformClick();

						AssertEquals(1, gvmsHeader.GvmsCustomsReferenceCollection.Count);
						AssertEquals(0, gvmsHeader.GvmsTransitReferenceCollection.Count);

						var reference = gvmsHeader.GvmsCustomsReferenceCollection.Cast<GvmsItemReference>().FirstOrDefault();
						AssertEquals(GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration, reference.CSI_Code);
						AssertEquals("TST6", reference.CSI_ReferenceNumber2);
						AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, reference.CSI_RN_NKCountryCode);
						AssertEquals(new ZDateTime(2021, 04, 06), reference.CSI_DateOfIssue);
						AssertEquals(GvmsItemReference.SystemStatus, reference.CSI_IssuerType);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001: Simplify name", Justification = "Simplification removes MenuItem in ToList<MenuItem> which is leading to build error.")]
		public void TestFeatureControlProtectionOfMenuItemForGVMSAManifestSending()
		{
			const string ExpectedMessageText =
				"""
				{
				  "direction": "UK_INBOUND",
				  "isUnaccompanied": false,
				  "vehicleRegNum": "ABC123",
				  "trailerRegistrationNums": [
				    "trailer1",
				    "trailer2"
				  ],
				  "plannedCrossing": {
				    "routeId": "",
				    "localDateTimeOfDeparture": "2020-12-02T10:00"
				  }
				}
				""";
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_Nature = GVMSManifestNature.Codes.Import;
				header.AMA_RN_NKCountry = countryCode;
				header.AMA_CustomsOffice = "TR000400";
				header.AMA_VehicleRegistration = "ABC123";
				header.AMA_Trailer1RegNo = "trailer1";
				header.AMA_Trailer2RegNo = "trailer2";
				header.AMA_E_DEP = new ZDateTime(2020, 12, 02, 10, 00, 00);
				Factory.Save();

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						GlbStaff.CurrentUser.GS_EmailAddress = "foo@bar.com";
						AssertEquals(false, menu.MenuItems.ToList<MenuItem>().Any(x => x.Text == "Delete &Manifest"));
						AssertEquals("Send &Manifest", menu.MenuItems[0].Text);
						menu.MenuItems[0].PerformClick();
						AssertEquals(1, header.Messages.Count);
						AssertEquals(Constants.GVMSMessageSubTypes.NEW, header.Messages[0].EM_MessageSubType);
						AssertEquals(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, header.Messages[0].EM_MessageType);
						AssertEquals(ExpectedMessageText, header.Messages[0].EM_MessageText);
						AssertEquals(false, header.HasChanges);
					}
				}

				header.RegistrationNumber = "Reg12345";
				Factory.Save();
				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						GlbStaff.CurrentUser.GS_EmailAddress = "foo@bar.com";

						AssertEquals("Send &Manifest", menu.MenuItems[0].Text);
						AssertEquals("Delete &Manifest", menu.MenuItems[1].Text);

						menu.MenuItems[0].PerformClick();

						AssertEquals(2, header.Messages.Count);
						AssertEquals(Constants.GVMSMessageSubTypes.AMEND, header.Messages[1].EM_MessageSubType);
						AssertEquals(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, header.Messages[1].EM_MessageType);
						AssertEquals(ExpectedMessageText, header.Messages[1].EM_MessageText);

						menu.MenuItems[1].PerformClick();
						AssertEquals(3, header.Messages.Count);
						AssertEquals(Constants.GVMSMessageSubTypes.CANCEL, header.Messages[2].EM_MessageSubType);
						AssertEquals(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, header.Messages[2].EM_MessageType);
						AssertEquals(ExpectedMessageText, header.Messages[2].EM_MessageText);
					}
				}

				header.RegistrationEntryNumber.CE_EntryStatus = GVMSCustomsStatus.Codes.Cancelled;
				Factory.Save();
				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						GlbStaff.CurrentUser.GS_EmailAddress = "foo@bar.com";

						AssertEquals("Re-send &Manifest", menu.MenuItems[0].Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.ClearUserResponses();
						UnitTestUserNotification.Instance.AddUserResponse("yes");
						UnitTestUserNotification.Instance.AddYesAnswer();
						menu.MenuItems[0].PerformClick();

						AssertEquals(4, header.Messages.Count);
						AssertEquals(Constants.GVMSMessageSubTypes.NEW, header.Messages[3].EM_MessageSubType);
						AssertEquals(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, header.Messages[3].EM_MessageType);
						AssertEquals(ExpectedMessageText, header.Messages[3].EM_MessageText);
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "TR000400", "Customs Office", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(10));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
	}
}
