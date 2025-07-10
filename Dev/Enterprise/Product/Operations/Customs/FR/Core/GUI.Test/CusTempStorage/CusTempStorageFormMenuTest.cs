using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using CusAuthorisationRuleTypeList = Enterprise.Customs.FR.Business.CusAuthorisationRuleTypeList;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.FR.Business.CusAuthorizationHeaderTypeList;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	class CusTempStorageFormMenuTest : TestCaseWithFactory
	{
		public void TestCreateMenuItemsBasedOnTempStorageApplicationCode()
		{
			AssertMenuItems(FRConstants.TemporaryStorage.AppCodeFRC, new string[] { "Receive into Transit Shed", "Transfer to Onward Transit Shed", "Report De-consolidation" });
			AssertMenuItems(FRConstants.TemporaryStorage.AppCodeIST, new string[] { "Send DDT" });
			AssertMenuItems(FRConstants.TemporaryStorage.AppCodeLAD, new string[] { "Send DDT" });
			AssertMenuItems("XXX", new string[] { "Send DDT" });
		}

		void AssertMenuItems(string appCode, string[] expectedMenus)
		{
			var jobHeader = CusTempStorageJobHeader.New(Factory, appCode);

			using (var menu = new CusTempStorageFormMenu())
			{
				menu.Header = jobHeader;

				CombineAssertions($"{appCode} Application Code", () =>
				{
					Assert("expectedMenus must contain at least one element", expectedMenus.Any());
					AssertEquals("MenuItems.Count", expectedMenus.Length, menu.MenuItems.Count);

					foreach (var menuCaption in expectedMenus)
					{
						AssertNotNull($"'{menuCaption}' menu item", menu.MenuItems.FindByText(menuCaption));
					}
				});
			}
		}

		public void TestReceiveIntoTransitShedMenu()
		{
			RunMenuItemTest(CreateFrcTestJobHeader(), "Receive into Transit Shed", assertionAction: header =>
			{
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestTransferToOnwardTransitShedMenu()
		{
			RunMenuItemTest(CreateFrcTestJobHeader(), "Transfer to Onward Transit Shed", assertionAction: header =>
			{
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestReportDeconsolidationMenu()
		{
			RunMenuItemTest(CreateFrcTestJobHeader(), "Report De-consolidation", assertionAction: header =>
			{
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestSendDdtMenu()
		{
			RunMenuItemTest(CreateIstTestJobHeader(), "Send DDT", assertionAction: header =>
			{
				AssertEquals("IST Application Code", "Temp. Storage Register JAC000012/FRJ00000001 was created.", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			RunMenuItemTest(CreateLadtTestJobHeader(), "Send DDT", assertionAction: header =>
			{
				AssertEquals("LAD Application Code", "Please select one valid customs profile.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestSendDdt_ShouldAskUserToSave()
		{
			RunMenuItemTest(CreateIstTestJobHeader(), "Send DDT",
				setupHeader: header =>
				{
					header.SJH_CustomsOffice = "FR001";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				},
				assertionAction: header =>
				{
					AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				});

			RunMenuItemTest(CreateIstTestJobHeader(), "Send DDT",
				setupHeader: header =>
				{
					header.SJH_CustomsOffice = "FR001";
					Factory.Save();
				},
				assertionAction: header =>
				{
					AssertNotEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				});
		}

		public void TestSendDdt_ShouldNotifyUserTheDeclarationWillBeLocked()
		{
			RunMenuItemTest(CreateIstTestJobHeader(), "Send DDT",
				setupHeader: header =>
				{
					header.SJH_CustomsOffice = "FR001";
					Factory.Save();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				},
				assertionAction: header =>
				{
					AssertContains("At sending DDT Register is created. No modification possible on this IST. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				});

			RunMenuItemTest(CreateIstTestJobHeader(), "Send DDT",
				setupHeader: header =>
				{
					header.SJH_CustomsOffice = "FR001";
					Factory.Save();
				},
				assertionAction: header =>
				{
					AssertNotContains("At sending DDT Register is created. No modification possible on this IST. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				});
		}

		public void TestSendDdt_ShouldAddISTEvents()
		{
			var header = CreateIstTestJobHeader();
			header.SJH_CustomsOffice = "FR001";
			header.SJH_CustomsProfile = "TST_ATH_001";
			Factory.Save();
			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();
				var messagesMenu = form.Menu.MenuItems.FindByText("Messages");
				var sendDdtMenu = messagesMenu.MenuItems.FindByText("Send DDT");
				sendDdtMenu.PerformClick();
				AssertEquals(true, header.Logs.Find(x => x.SL_SE_NKEvent == Events.InStoreCode).Any());
			}
		}

		public void TestSendDdt_ShouldLockDeclaration()
		{
			var header = CreateIstTestJobHeader();
			header.SJH_CustomsOffice = "FR001";
			header.SJH_CustomsProfile = "TST_ATH_001";
			Factory.Save();
			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();
				var customsOfficeBox = form.FindSingle<ZCodeFindBox>("CustomsOfficeCodeFindBox");
				AssertEquals("PRE: Control is not read only.", false, customsOfficeBox.ReadOnly);

				var messagesMenu = form.Menu.MenuItems.FindByText("Messages");
				var sendDdtMenu = messagesMenu.MenuItems.FindByText("Send DDT");
				sendDdtMenu.PerformClick();
				AssertEquals("Control is read only after sending DDT.", true, customsOfficeBox.ReadOnly);
			}
		}

		public void TestSendDdtMenu_Visibility()
		{
			var header = CreateIstTestJobHeader();
			using (var form = new CusTempStorageForm(header))
			{
				var messagesMenu = form.Menu.MenuItems.FindByText("Messages");
				var sendDdtMenu = messagesMenu.MenuItems.FindByText("Send DDT");
				AssertEquals("Send DDT is visible when declaration is Open.", true, sendDdtMenu.Visible);
			}

			header.Logs.AddNew(Events.InStore);
			using (var form = new CusTempStorageForm(header))
			{
				var messagesMenu = form.Menu.MenuItems.FindByText("Messages");
				var sendDdtMenu = messagesMenu.MenuItems.FindByText("Send DDT");
				AssertEquals("Send DDT is hidden when declaration is Closed.", false, sendDdtMenu.Visible);
			}
		}

		void RunMenuItemTest(CusTempStorageJobHeader header, string caption, Action<CusTempStorageJobHeader> setupHeader = null, Action<CusTempStorageJobHeader> assertionAction = null)
		{
			setupHeader?.Invoke(header);
			using (var frm = new CusTempStorageForm(header))
			{
				var topLevelMenu = frm.Menu.MenuItems.FindByText("Messages");
				AssertNotNull("'Messages' menu not found", topLevelMenu);
				var actionMenu = topLevelMenu.MenuItems.FindByText(caption);
				AssertNotNull($"Action menu '{caption}' not found", actionMenu);

				UnitTestUserNotification.Instance.ClearMessages();
				actionMenu.PerformClick();

				assertionAction?.Invoke(header);
			}
		}

		CusTempStorageJobHeader CreateFrcTestJobHeader() => CreateTestJobHeader(FRConstants.TemporaryStorage.AppCodeFRC);
		CusTempStorageJobHeader CreateIstTestJobHeader() => CreateTestJobHeader(FRConstants.TemporaryStorage.AppCodeIST);
		CusTempStorageJobHeader CreateLadtTestJobHeader() => CreateTestJobHeader(FRConstants.TemporaryStorage.AppCodeLAD);

		CusTempStorageJobHeader CreateTestJobHeader(string applicationCode)
		{
			var jobHeader = CusTempStorageJobHeader.New(Factory, applicationCode);
			jobHeader.SJH_OH_Customer = customer.PK;

			CusTempStorageLine line = jobHeader.SJH_AppCode == FRConstants.TemporaryStorage.AppCodeFRC ? (CusTempStorageLine)jobHeader.CusTempStorageDec.CusTempStorageLines.AddNew() : (CusTempStorageLine)jobHeader.CusTempStorageDec.CusTempStorageLines.First();

			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_PackageQty = 1;

			return jobHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupCustomer();
			SetupAuthorisation();
			SetUpCustomsOffice();
		}

		void SetupCustomer()
		{
			customer = Factory.NewWithValidTestData<OrgHeader>();
		}

		void SetupAuthorisation()
		{
			authorisation = Factory.New<CusAuthorisationHeader>();
			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			authorisation.CPH_Number = "TST_ATH_001";
			authorisation.CPH_OH_PermitHolder = customer.PK;
			authorisation.CPH_StartDate = ZDate.Today.AddMonths(-1);
			authorisation.CPH_EndDate = ZDate.Today.AddMonths(1);
			var rule = authorisation.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.USE;
			rule.CPR_ValueFrom = FRConstants.TemporaryStorage.AppCodeIST;

			var stmNums = authorisation.CustomsNumberProvider.CustomsNumbers.AddNew();
			stmNums.SN_Type = CusAuthorisationHeaderCustomsNumberRangeTypeList.Codes.TemporaryStorageInstallationDdtNumberFrance;
			stmNums.SN_FountainName = "JAC";
			stmNums.SN_MinimumValue = 1;
			stmNums.SN_Count = 100;
			stmNums.SN_Value = 12;
			Factory.Save();
		}

		void SetUpCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "CustomsOffice");
			var codeList = helper.CreateCusCodeList("FR", "CUSOF", "FR001", "FR001 Customs", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("EmailAddress", "EmailAddress", "CUSOF", "FR");
			codeList.Attributes.AddNew("EmailAddress", "Check.Yao@wisetechglobal.com");
			Factory.Save();
		}

		OrgHeader customer;
		CusAuthorisationHeader authorisation;
	}
}
