using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI.Testing
{
	public class UPEAirCargoShipmentMenuTest : TestCaseWithFactory
	{
		public void TestBaseClassOverriddenForClient()
		{
			AssertEquals("No constructors should exist on the base class, factory method New() should be called", 0, typeof(AirCargoShipmentMenu).GetConstructors().Length);
			using (AirCargoShipmentMenu menu = AirCargoShipmentMenu.New(MessageManager))
			{
				AssertEquals("The UPE client-specific menu should be created", typeof(UPEAirCargoShipmentMenu), menu.GetType());
			}
		}

		public void TestCreateFormalDecMenuItem()
		{
			CusHAWB.CS_GoodsDescription = "Desc transferred from dbo.CusHAWB";
			Factory.Save();
			MenuItem createFormalDecMenuItem = FindMenuItem(Menu, "Create Formal Declaration");
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			createFormalDecMenuItem.PerformClick();
			AssertNotNull("Declaration should be created in memory and the form shown", Menu.LastJobDeclarationControllerCreated.LastShownForm);
			using (ZForm newDeclarationForm = (ZForm)Menu.LastJobDeclarationControllerCreated.LastShownForm)
			{
				Customs.Business.BaseJobDeclaration declarationOnForm = (Customs.Business.BaseJobDeclaration)newDeclarationForm.BusinessEntity;
				AssertEquals("The new declaration on the form should have the information from the air cargo record", declarationOnForm.JE_GoodsDescription, CusHAWB.CS_GoodsDescription);
				AssertEquals(Core.Constants.IncoTerms.FreeOnBoard, declarationOnForm.JE_ShipmentIncoTerm); //this is defaulted in the UPEVersionOnly
				AssertEquals("Any errors/warnings should be shown to the user", true, UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("Test declaration creation warning") != -1);
			}
		}

		public void TestCreateFormalDecMenuItem_ShowsErrorIfAirCargoUnsaved()
		{
			CusHAWB.CS_GoodsDescription = "Air cargo has been changed by user";
			MenuItem createFormalDecMenuItem = FindMenuItem(Menu, "Create Formal Declaration");
			createFormalDecMenuItem.PerformClick();
			AssertEquals("An error should be shown to the user indicating the air cargo is not saved", true, UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("must save the form") != -1);
		}

		public void TestCreateFormalDecMenuItem_ShowsErrorIfDeclarationAlreadyCreatedForHAWB()
		{
			MenuItem createFormalDecMenuItem = FindMenuItem(Menu, "Create Formal Declaration");
			CusHAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(JobDeclaration)).PK;
			Factory.Save();
			createFormalDecMenuItem.PerformClick();
			AssertEquals("An error should be shown to the user indicating a declaration has already been created for the HAWB", "A formal declaration has already been created for this shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateFormalDecMenuItem_ConfirmIfFormalDecNotRequired()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			MenuItem createFormalDecMenuItem = FindMenuItem(Menu, "Create Formal Declaration");
			CusHAWB.CS_GoodsValue = 0.05m;
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Menu, new object[] { EventArgs.Empty });
			createFormalDecMenuItem.PerformClick();
			AssertEquals("The new declaration form should not be shown as the user cancelled the process", null, Menu.LastJobDeclarationControllerCreated);
			AssertEquals("The user should be warned", "A formal declaration is not required for this shipment. Proceed anyway?", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			createFormalDecMenuItem.PerformClick();
			AssertNotNull("The new declaration form should be shown to the user for save", Menu.LastJobDeclarationControllerCreated);
			Menu.LastJobDeclarationControllerCreated.LastShownForm.Dispose();
		}

		public void TestCreateFormalDecMenuItem_AllHAWBsAreNotLoaded()
		{
			UPECusHAWB hAWB1 = Factory.NewWithValidTestData<UPECusHAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			UPECusHAWB hAWB2 = Factory.NewWithValidTestData<UPECusHAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			MenuItem createFormalDecMenuItem = FindMenuItem(Menu, "Create Formal Declaration");
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			createFormalDecMenuItem.PerformClick();
			AssertNotNull("Declaration should be created in memory and the form shown", Menu.LastJobDeclarationControllerCreated.LastShownForm);
			using (ZForm newDeclarationForm = (ZForm)Menu.LastJobDeclarationControllerCreated.LastShownForm)
			{
				UPEJobDeclaration declarationOnForm = (UPEJobDeclaration)newDeclarationForm.BusinessEntity;
				AssertEquals("Declaration should only have 1 CusHAWB", 1, declarationOnForm.RelatedCusHAWBs.Count);
				AssertEquals(CusHAWB.PK, declarationOnForm.RelatedCusHAWBs[0].PK);
			}
		}

		public void TestCreateFormalDecMenuItem_FreightRateIsCalculatedOnDec()
		{
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			loader.LoadOrCreate(CusHAWB.PK, OrgMatchApprovalType.AirCargoConsignee);
			CompanyTariff levelOneTariff = new TestHelper(Factory).NewCompanyTariff();
			RateEntry aIREntry1 = levelOneTariff.AddRateEntry("AIR", "LSE", "US", "AU", "STD", "");
			aIREntry1.TI_RX_NKCurrency = "AUD";
			RateLine aIRRateLine1 = aIREntry1.RateLines[0];
			aIRRateLine1.RateLineItems.RemoveAndDeleteAll();
			aIRRateLine1.Calculator["-10"] = (ZDecimal)100m;
			aIRRateLine1.Calculator["+10"] = (ZDecimal)95m;
			levelOneTariff.Factory.Save();
			CusHAWB.CS_GoodsDescription = "Goods description";
			CusHAWB.CS_RL_NKOrigin = "USLAX";
			CusHAWB.CS_RL_NKDestination = "AUSYD";
			CusHAWB.MAWB.CM_RL_NKDischargePort = "AUSYD";
			CusHAWB.MAWB.CM_RL_NKLoadPort = "USLAX";
			CusHAWB.CS_RS_NK_ServiceLevel = "STD";
			CusHAWB.CS_Weight = 15m;
			CusHAWB.CS_WeightUQ = Core.Constants.Weight.Kilograms;
			CusHAWB.Level1Record = new Level1Record();
			CusHAWB.Level1Record.AddRecordLine("US3295AU9639050704              DAT2773T8Z8W5110001   EA G/RIDGE B/L BANJO BOLT 7/16-24                                                                          1217      USD300                 US8714190060          NLR            AU14138                                                                                                                                            ");
			CusHAWB.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			CusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			CusHAWB.UPEConsigneeAddress.P3_OH_MatchOrg = CusHAWB.Consignee.PK;
			CusHAWB.Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			Factory.Save();
			MenuItem createFormalDecMenuItem = FindMenuItem(Menu, "Create Formal Declaration");
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			createFormalDecMenuItem.PerformClick();
			AssertNotNull("Declaration should be created in memory and the form shown", Menu.LastJobDeclarationControllerCreated.LastShownForm);
			using (ZForm newDeclarationForm = (ZForm)Menu.LastJobDeclarationControllerCreated.LastShownForm)
			{
				Customs.Business.BaseJobDeclaration declaration = (Customs.Business.BaseJobDeclaration)newDeclarationForm.BusinessEntity;
				Customs.Business.BaseJobComInvHeaderCharge baseJobComInvHeaderCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.GetChargeByChargeName(AUChargeCodeList.Codes.OverseasFreight);
				AssertNotNull("Declaration should have an Overseas Freight Rate", baseJobComInvHeaderCharge);
				AssertEquals("Amount should be 1425.00", 1425.00m, baseJobComInvHeaderCharge.J7_Amount);
				AssertEquals("Currency Should be ", Core.Constants.CurrencyCodes.Australia, baseJobComInvHeaderCharge.J7_RX_NKCurrency);
			}
		}

		public void TestMatchAndCreateFormalDecMenuItem()
		{
			Factory.Save();
			MenuItem menuItem = FindMenuItem(Menu, "Requires Formal Declaration");
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			menuItem.PerformClick();
			AssertEquals("Changes should be saved to the database", false, CusHAWB.HasChanges);
			AssertEquals("An informational message should be shown to the user indicating the action taken", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals("An informational message should be shown to the user indicating the action taken", "Shipment queued for matching, or match completed and declaration will be created", UnitTestUserNotification.Instance.LastMessage.Text);
			menuItem.PerformClick();
			AssertEquals("A warning indicating the shipment is already queued should be shown", true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
			AssertEquals("A warning indicating the shipment is already queued should be shown", "Shipment consignee/importer and consignor are already on the matching queue, or matching has been completed", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMatchAndCreateFormalDecMenuItem_WithSaveError()
		{
			Factory.Save();
			MenuItem menuItem = FindMenuItem(Menu, "Requires Formal Declaration");
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Menu.ThrowOnFactorySave = true;
			menuItem.PerformClick();
			AssertEquals("An error message should be shown to the user indicating a save error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Error during save"));
		}

		public void TestMatchAndCreateFormalDecMenuItem_WhenFormNotSaved()
		{
			CusHAWB.CS_GoodsDescription = "changes made";
			MenuItem menuItem = FindMenuItem(Menu, "Requires Formal Declaration");
			menuItem.PerformClick();
			AssertEquals("A warning message should be shown to the user indicating the form not saved", true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
			AssertEquals("A warning message should be shown to the user indicating the form not saved", "You must save the form before you can perform this action", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMatchAndCreateFormalDecMenuItem_ShowsErrorIfDeclarationAlreadyCreatedForHAWB()
		{
			MenuItem menuItem = FindMenuItem(Menu, "Requires Formal Declaration");
			CusHAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(JobDeclaration)).PK;
			Factory.Save();
			menuItem.PerformClick();
			AssertEquals("An error should be shown to the user indicating a declaration has already been created for the HAWB", "A formal declaration has already been created for this shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMatchAndCreateFormalDecMenuItem_MatchesAutomatically()
		{
			OrgHeader orgForMatching = GetOrgForMatching();
			CusHAWB.CS_ConsigneeCity = orgForMatching.MainAddress.OA_City;
			CusHAWB.CS_ConsigneeName = orgForMatching.OH_FullName;
			CusHAWB.CS_ConsigneePhone = orgForMatching.MainAddress.OA_Phone;
			CusHAWB.CS_ConsigneePostcode = orgForMatching.MainAddress.OA_PostCode;
			CusHAWB.CS_ConsigneeState = orgForMatching.MainAddress.OA_State;
			CusHAWB.CS_ConsigneeStreet = orgForMatching.MainAddress.OA_Address1;
			CusHAWB.CS_ConsigneeStreet2 = orgForMatching.MainAddress.OA_Address2;
			CusHAWB.CS_ConsignorCity = orgForMatching.MainAddress.OA_City;
			CusHAWB.CS_ConsignorName = orgForMatching.OH_FullName;
			CusHAWB.CS_ConsignorPhone = orgForMatching.MainAddress.OA_Phone;
			CusHAWB.CS_ConsignorPostcode = orgForMatching.MainAddress.OA_PostCode;
			CusHAWB.CS_ConsignorState = orgForMatching.MainAddress.OA_State;
			CusHAWB.CS_ConsignorStreet = orgForMatching.MainAddress.OA_Address1;
			CusHAWB.CS_ConsignorStreet2 = orgForMatching.MainAddress.OA_Address2;
			CusHAWB.CS_RL_NKOrigin = "SGSIN";
			Factory.Save();
			MenuItem menuItem = FindMenuItem(Menu, "Requires Formal Declaration");
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("Consignor should not be matched initially for the test", true, CusHAWB.CS_OA_ConsignorAddress.IsEmpty);
			menuItem.PerformClick();
			AssertEquals("Consignor should have been auto-matched", false, CusHAWB.CS_OA_ConsignorAddress.IsEmpty);
		}

		public void TestMatchAndCreateFormalDecMenuItem_ConfirmIfFormalDecNotRequired()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			MenuItem createFormalDecMenuItem = FindMenuItem(Menu, "Requires Formal Declaration");
			CusHAWB.CS_GoodsValue = 0.05m;
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			createFormalDecMenuItem.PerformClick();
			AssertEquals("The user should be warned", "A formal declaration is not required for this shipment. Proceed anyway?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("No org should be queued for match because the user cancelled the process", false, CusHAWB.RequiresConsignorMatch);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			createFormalDecMenuItem.PerformClick();
			AssertEquals("Consignor should have been queued for matched", true, CusHAWB.RequiresConsignorMatch);
		}

		#region Test Classes
		class TestUPEAirCargoShipmentMenu : UPEAirCargoShipmentMenu
		{
			public TestUPEAirCargoShipmentMenu(CusHAWBMessageManager manager) : base(manager)
			{
			}

			protected override UPEDeclarationFromAirCargoCreator NewDeclarationFromAirCargoCreator(CusHAWB houseAirCargo)
			{
				return new TestDeclarationFromAirCargoCreator(houseAirCargo);
			}

			public ZController LastJobDeclarationControllerCreated;
			protected override ZController NewJobDeclarationController()
			{
				LastJobDeclarationControllerCreated = base.NewJobDeclarationController();
				return LastJobDeclarationControllerCreated;
			}

			public bool ThrowOnFactorySave;
			protected override void FactorySave(BusinessObjectFactory factory)
			{
				if (ThrowOnFactorySave)
				{
					throw new ZSaveException(new ZDataException(new InvalidOperationException("Error during save"), null, null), factory);
				}
			}
		}

		class TestDeclarationFromAirCargoCreator : UPEDeclarationFromAirCargoCreator
		{
			public TestDeclarationFromAirCargoCreator(CusHAWB houseAirCargo) : base(houseAirCargo)
			{
			}

			protected override void CreateCore(Customs.Business.BaseJobDeclaration declaration, INotifications notify)
			{
				notify.Notify(new WarningNotification("Test declaration creation warning"));
				base.CreateCore(declaration, notify);
			}
		}

		#endregion
		#region Implementation
		MenuItem FindMenuItem(MenuItem menuItem, string menuItemText)
		{
			typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menuItem, new object[] { EventArgs.Empty });
			foreach (MenuItem next in menuItem.MenuItems)
			{
				if (next.Text == menuItemText)
				{
					return next;
				}
			}

			return null;
		}

		OrgHeader GetOrgForMatching()
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.MainAddress.OA_City = "City";
			result.OH_FullName = "FullName";
			result.MainAddress.OA_Phone = "Phone";
			result.MainAddress.OA_PostCode = "PostCode";
			result.MainAddress.OA_State = "State";
			result.MainAddress.OA_Address1 = "Address1";
			result.MainAddress.OA_Address2 = "Address2";
			Factory.Save();
			return result;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			CusMAWB = Factory.New<CusMAWB>();
			CusHAWB = (UPECusHAWB)CusMAWB.ChildBills.AddNew();
			CusHAWB.CS_GoodsValue = 500;
			CusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			MessageManager = new CusHAWBMessageManager(delegate
			{
				return CusHAWB;
			});
			Menu = new TestUPEAirCargoShipmentMenu(MessageManager);
		}

		protected override void TearDown()
		{
			Menu.Dispose();
			base.TearDown();
		}

		CusMAWB CusMAWB;
		UPECusHAWB CusHAWB;
		CusHAWBMessageManager MessageManager;
		TestUPEAirCargoShipmentMenu Menu;
		#endregion
	}
}
