using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using TransactionTypes = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	[TestedType(typeof(TempStorageRegisterForm))]
	class TempStorageRegisterFormTest : ZFormBasherTest
	{
		public void TestAddReOpenTempStorageRegisterMenuOption()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			var regLine1 = header.CusTempStorageRegLines.AddNew();
			var transaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_PackageQty = 1;
			CombineAssertions(() =>
			{
				AssertEquals("Pre-requisite : Temp. Storage Register Header should be OPN", TempStorageDeclarationStatusList.Codes.Open, header.SRH_Status);
				AssertReOpenMenuItemExistOrNot(header, true);
			});

			var transaction2 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_PackageQty = -1;
			CombineAssertions(() =>
			{
				AssertEquals("Pre-requisite : Temp. Storage Register Header should be CLS", TempStorageDeclarationStatusList.Codes.Closed, header.SRH_Status);
				AssertReOpenMenuItemExistOrNot(header, false);
			});

			void AssertReOpenMenuItemExistOrNot(CusTempStorageRegHeader regHeader, bool isNull)
			{
				using (var form = new TempStorageRegisterForm(header))
				{
					form.Show();
					var menu = (ZMenuItem)form.Menu.GetMainMenu().MenuItems.Find("ActionsMenuItem", true).First();
					var menuItemText = "Re-open for manual adjustment";
					if (isNull)
					{
						AssertNull("Re-open Menu Item should not exist in Action Menus", menu.MenuItems.FindByText(menuItemText));
					}
					else
					{
						AssertNotNull("Re-open Menu Item shoud exist in the Action Menus", menu.MenuItems.FindByText(menuItemText));
					}
				}
			}
		}

		public void TestReOpenTempStorageRegisterMenuItemClickedToSecurityRight()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "Job001";
			var regLine1 = header.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			var transaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_PackageQty = 10;
			transaction1.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			var transaction2 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_PackageQty = -10;
			transaction2.SRT_TransactionType = TransactionTypes.Codes.Transaction;
			AssertEquals("Pre-requisite : Temp. Storage Register Header should be CLS.", TempStorageDeclarationStatusList.Codes.Closed, header.SRH_Status);

			Env.Security.FRTempStorageRegisterReOpening.IsAllowed = false;
			using (var form = new TempStorageRegisterForm(header))
			{
				form.Show();
				var reOpenMenuItem = (ZMenuItem)form.Menu.GetMainMenu().MenuItems.Find("ActionsMenuItem", true).First().MenuItems.FindByText("Re-open for manual adjustment");
				reOpenMenuItem.PerformClick();
				AssertEquals("Invalid security right error message", "You don't have the proper security right to re-open this Temp. Storage Register Header.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Temp. Storage Register Header cannot be opened.", TempStorageDeclarationStatusList.Codes.Closed, header.SRH_Status);
			}

			Env.Security.FRTempStorageRegisterReOpening.IsAllowed = true;
			using (var form = new TempStorageRegisterForm(header))
			{
				form.Show();
				var reOpenMenuItem = (ZMenuItem)form.Menu.GetMainMenu().MenuItems.Find("ActionsMenuItem", true).First().MenuItems.FindByText("Re-open for manual adjustment");
				reOpenMenuItem.PerformClick();
				AssertEquals("Temp. Storage Register Header can be opened.", TempStorageDeclarationStatusList.Codes.Open, header.SRH_Status);
				var transaction3 = regLine1.CusTempStorageRegLineTransactions.AddNew();
				transaction3.SRT_PackageQty = 5;
				transaction3.SRT_ReferenceType = TransactionTypes.Codes.Adjustment;
				header.Factory.Save();
				header.Reload();
				Assert("UCK log is correctly added to this Temp. Reg. Header.", header.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == "UCK"));
			}
		}

		public void TestAllowNew()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			using (var form = new TempStorageRegisterForm(header))
			{
				var provider = form as IPostingButtonsProvider;
				AssertNotNull(provider);
				AssertEquals("User should not be able to create new register, the system does instead", false, provider.AllowNew);
			}
		}

		public void TestFormCaption()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			using (var form = new TempStorageRegisterForm(header))
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("No Reference", "Temp. Storage Register", form.FormCaption);
					header.SRH_Reference = "ATB150002110520195876";
					header.SRH_InternalReference = "FRJ00480021";
					AssertEquals("Reference Entered", "Temp. Storage Register - ATB150002110520195876/FRJ00480021", form.FormCaption);
				});
			}
		}

		public void TestCheckOnShowPreSaveDialogs()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			using (var form = new TempStorageRegisterForm(header))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var line = header.CusTempStorageRegLines.AddNew();
				line.SRL_LocationOfGoods = "CN";
				line.SRL_LimitDate = ZDateTime.Now.Date;
				line.SRL_LineNumber = 1;

				var transaction = line.CusTempStorageRegLineTransactions.AddNew();
				transaction.SRT_InternalReferenceNumber = "T001";
				transaction.SRT_PackageQty = 10;
				transaction.SRT_GrossWeight = 10m;
				transaction.SRT_PackageQty = 100;

				form.FireSaveButton();
				AssertEquals("Transactions cannot be amended once saved. Do you want to continue saving the transactions?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			Factory.Save();
			var result = new TempStorageRegisterForm(header);
			result.ControllerID = ControllerIDs.Customs.EU.TempStorageRegister;
			return result;
		}
	}
}
