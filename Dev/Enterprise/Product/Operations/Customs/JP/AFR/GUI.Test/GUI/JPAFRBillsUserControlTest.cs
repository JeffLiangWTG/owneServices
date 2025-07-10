using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	sealed class JPAFRBillsUserControlTest : TestCaseWithFactory
	{
		public void TestDangerousGoods()
		{
			var header = Factory.New<JPAFRHeader>();
			header.Bills.AddNew();

			using (var form = new ZForm(header))
			using (var control = new JPAFRBillsUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(header, "");
				form.Show();

				var tabPage = control.FindSingleOrDefault<ZTabPage>("DangerousGoodsTabPage");
				AssertNotNull(tabPage);

				CombineAssertions(() =>
				{
					AssertNotNull("DangerousGoodsTabPage-JPB_IMOClassTextBox", tabPage.FindSingleOrDefault<ZTextBox>("JPB_IMOClassTextBox"));
					AssertNotNull("DangerousGoodsTabPage-JPB_DGGuidFindBox", tabPage.FindSingleOrDefault<ZGuidFindBox>("JPB_DGGuidFindBox"));
					AssertNotNull("DangerousGoodsTabPage-JPB_SpecialCargoCodeCodeFindBox", tabPage.FindSingleOrDefault<ZCodeFindBox>("JPB_SpecialCargoCodeCodeFindBox"));

					var extraUNDGsGrid = tabPage.FindSingleOrDefault<ZGrid>("ExtraUNDGsGrid");
					AssertNotNull("DangerousGoodsTabPage-ExtraUNDGsGrid", extraUNDGsGrid);

					Assert("ExtraUNDGsGrid-DI_DG", extraUNDGsGrid.ColumnStyles.OfType<ZGuidFindBoxColumnStyleInfo>().Any(a => a.ColumnName == "DI_DG" && !a.IsReadOnly));
					Assert("ExtraUNDGsGrid-DI_Description", extraUNDGsGrid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Any(a => a.ColumnName == "DI_Description" && a.IsReadOnly));
					Assert("ExtraUNDGsGrid-DI_IMOClass", extraUNDGsGrid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Any(a => a.ColumnName == "DI_IMOClass" && a.IsReadOnly));
				});
			}
		}

		public void TestBLLFunctionMenuItem()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var form = new ZForm(header))
				using (var control = new JPAFRBillsUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(header, "");
					form.Show();

					var billsGrid = (ZGrid)control.Controls.Find("BillsGrid", true).First();
					var registerSplitMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Register Split Bill", true);
					var registerSwitchMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Register Switch Bill", true);
					var registerMergeMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Register Merge Bill", true);
					var cancelSplitMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Split Bill", true);
					var cancelSwitchMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Switch Bill", true);
					var cancelMergeMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Merge Bill", true);
					CombineAssertions(() =>
					{
						AssertNull(registerSplitMenuItem);
						AssertNull(registerSwitchMenuItem);
						AssertNull(registerMergeMenuItem);
						AssertNull(cancelSplitMenuItem);
						AssertNull(cancelSwitchMenuItem);
						AssertNull(cancelMergeMenuItem);
					});
				}
			}

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm(header))
				using (var control = new JPAFRBillsUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(header, "");
					form.Show();

					var billsGrid = (ZGrid)control.Controls.Find("BillsGrid", true).First();
					var registerSplitMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Register Split Bill", true);
					var registerSwitchMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Register Switch Bill", true);
					var registerMergeMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Register Merge Bill", true);
					var cancelSplitMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Split Bill", true);
					var cancelSwitchMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Switch Bill", true);
					var cancelMergeMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Merge Bill", true);
					CombineAssertions(() =>
					{
						AssertNotNull(registerSplitMenuItem);
						AssertNotNull(registerSwitchMenuItem);
						AssertNotNull(registerMergeMenuItem);
						AssertNotNull(cancelSplitMenuItem);
						AssertNotNull(cancelSwitchMenuItem);
						AssertNotNull(cancelMergeMenuItem);
					});
				}
			}
		}

		public void TestRegisterationMenuItem_Visibility()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm(header))
				using (var control = new JPAFRBillsUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(header, "");
					form.Show();

					var billsGrid = (ZGrid)control.Controls.Find("BillsGrid", true).First();
					var registerSplitMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Register Split Bill", true);
					var registerSwitchMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Register Switch Bill", true);
					var registerMergeMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Register Merge Bill", true);

					billsGrid.ContextMenu.DoPopup();
					CombineAssertions(() =>
					{
						AssertNotNull(registerSplitMenuItem);
						AssertNotNull(registerSwitchMenuItem);
						AssertNotNull(registerMergeMenuItem);
						Assert(registerSplitMenuItem.Visible);
						Assert(registerSwitchMenuItem.Visible);
						Assert(registerMergeMenuItem.Visible);
					});
				}
			}
		}

		public void TestCancelSplitMenuItem_Visibility()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			var bllFunctionInfo = Factory.New<BLLFunctionInfo>();
			bllFunctionInfo.B7_ParentID = bill1.PK;
			bllFunctionInfo.B7_ParentTableCode = bill1.TablePrefix;
			bllFunctionInfo.B7_Type = CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction;
			bllFunctionInfo.JP_FunctionCode = (int)BLLFunctionCode.RegisterSplit;
			Factory.Save();

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm(header))
				using (var control = new JPAFRBillsUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(header, "");
					form.Show();

					var billsGrid = (ZGrid)control.Controls.Find("BillsGrid", true).First();

					var cancelSplitMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Split Bill", true);
					var cancelSwitchMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Switch Bill", true);
					var cancelMergeMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Merge Bill", true);
					billsGrid.ContextMenu.DoPopup();

					CombineAssertions(() =>
					{
						AssertNotNull(cancelSplitMenuItem);
						AssertNotNull(cancelSwitchMenuItem);
						AssertNotNull(cancelMergeMenuItem);
						Assert(cancelSplitMenuItem.Visible);
						Assert(!cancelSwitchMenuItem.Visible);
						Assert(!cancelSwitchMenuItem.Visible);
					});
				}
			}
		}

		public void TestCancelSwitchMenuItem_Visibility()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			var bllFunctionInfo = Factory.New<BLLFunctionInfo>();
			bllFunctionInfo.B7_ParentID = bill1.PK;
			bllFunctionInfo.B7_ParentTableCode = bill1.TablePrefix;
			bllFunctionInfo.B7_Type = CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction;
			bllFunctionInfo.JP_FunctionCode = (int)BLLFunctionCode.RegisterSwitch;
			Factory.Save();

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm(header))
				using (var control = new JPAFRBillsUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(header, "");
					form.Show();

					var billsGrid = (ZGrid)control.Controls.Find("BillsGrid", true).First();

					var cancelSplitMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Split Bill", true);
					var cancelSwitchMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Switch Bill", true);
					var cancelMergeMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Merge Bill", true);

					billsGrid.ContextMenu.DoPopup();
					CombineAssertions(() =>
					{
						AssertNotNull(cancelSplitMenuItem);
						AssertNotNull(cancelSwitchMenuItem);
						AssertNotNull(cancelMergeMenuItem);
						Assert(!cancelSplitMenuItem.Visible);
						Assert(cancelSwitchMenuItem.Visible);
						Assert(!cancelMergeMenuItem.Visible);
					});
				}
			}
		}

		public void TestCancelMergeMenuItem_Visibility()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			var bllFunctionInfo = Factory.New<BLLFunctionInfo>();
			bllFunctionInfo.B7_ParentID = bill1.PK;
			bllFunctionInfo.B7_ParentTableCode = bill1.TablePrefix;
			bllFunctionInfo.B7_Type = CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction;
			bllFunctionInfo.JP_FunctionCode = (int)BLLFunctionCode.RegisterMerge;
			Factory.Save();

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm(header))
				using (var control = new JPAFRBillsUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(header, "");
					form.Show();

					var billsGrid = (ZGrid)control.Controls.Find("BillsGrid", true).First();

					var cancelSplitMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Split Bill", true);
					var cancelSwitchMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Switch Bill", true);
					var cancelMergeMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Cancel Merge Bill", true);

					billsGrid.ContextMenu.DoPopup();
					CombineAssertions(() =>
					{
						AssertNotNull(cancelSplitMenuItem);
						AssertNotNull(cancelSwitchMenuItem);
						AssertNotNull(cancelMergeMenuItem);
						Assert(!cancelSplitMenuItem.Visible);
						Assert(!cancelSwitchMenuItem.Visible);
						Assert(cancelMergeMenuItem.Visible);
					});
				}
			}
		}

		public void TestContainerUpdateCheck()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			var bill1Container1 = bill1.Containers.AddNew();
			bill1Container1.JPC_ContainerNum = "CONT1";
			var bill1Container2 = bill1.Containers.AddNew();
			bill1Container2.JPC_ContainerNum = "CONT1";
			var bill2 = header.Bills.AddNew();
			var bill2Container1 = bill2.Containers.AddNew();
			bill2Container1.JPC_ContainerNum = "CONT1";
			var bill2Container2 = bill2.Containers.AddNew();
			bill2Container2.JPC_ContainerNum = "CONT1";
			var bill3 = header.Bills.AddNew();
			var bill3Container1 = bill3.Containers.AddNew();
			bill3Container1.JPC_ContainerNum = "CONT1";
			using (var form = new ZForm(header))
			using (var control = new JPAFRBillsUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(header, "");
				form.Show();
				var billDetailsTabControl = (ZTabControl)control.Controls["BillDetailsTabControl"];
				var containerDetailsTabPage = (ZTabPage)billDetailsTabControl.Controls["ContainerDetailsTabPage"];
				billDetailsTabControl.SelectTab(containerDetailsTabPage);
				var containersGrid = (ZGrid)containerDetailsTabPage.Controls["ContainersGrid"];
				var container = (JPAFRContainer)containersGrid.ListManager.GetCurrent();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				container.JPC_ContainerNum = "CONT2";
				AssertEquals("Would you like to change all containers with Container Number 'CONT1' to 'CONT2' also?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(bill1Container1, container);
				AssertEquals("CONT1", bill1Container2.JPC_ContainerNum);
				AssertEquals("CONT1", bill2Container1.JPC_ContainerNum);
				AssertEquals("CONT1", bill2Container2.JPC_ContainerNum);
				AssertEquals("CONT1", bill3Container1.JPC_ContainerNum);

				container.JPC_ContainerNum = "CONT1";
				AssertEquals("CONT1", bill1Container2.JPC_ContainerNum);
				AssertEquals("CONT1", bill2Container1.JPC_ContainerNum);
				AssertEquals("CONT1", bill2Container2.JPC_ContainerNum);
				AssertEquals("CONT1", bill3Container1.JPC_ContainerNum);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				container.JPC_ContainerNum = "CONT2";
				AssertEquals("CONT2", bill1Container2.JPC_ContainerNum);
				AssertEquals("CONT2", bill2Container1.JPC_ContainerNum);
				AssertEquals("CONT2", bill2Container2.JPC_ContainerNum);
				AssertEquals("CONT2", bill3Container1.JPC_ContainerNum);
			}
		}

		public void TestDifferentControlsForDifferentJobTypes()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = false;
			using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
			{
				header.InBondDetailInitiator = inbondInitiator;
				var bill = header.Bills.AddNew();
				bill.JPB_ContainerOperatorCode = "COC01";
				bill.JPB_IsMaterBill = true;
				bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "GTN01";

				using (var form = new ZForm(header))
				using (var control = new JPAFRBillsUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(header, "");
					form.Show();

					var billsTabControl = control.Controls.Find("BillDetailsTabControl", true).FirstOrDefault() as ZTabControl;
					billsTabControl.SelectedIndex = 0;

					var containerOperaterCodeTextBox = control.Controls.Find("ContainerOperaterCodeTextBox", true).FirstOrDefault() as ZTextBox;
					AssertNotNull(containerOperaterCodeTextBox);
					AssertEquals(false, containerOperaterCodeTextBox.Visible);
					AssertEquals(string.Empty, containerOperaterCodeTextBox.BindTo);
					AssertEquals(string.Empty, containerOperaterCodeTextBox.Text);

					var isMasterBillCheckBox = control.Controls.Find("IsMasterBillCheckBox", true).FirstOrDefault() as ZCheckBox;
					AssertNotNull(isMasterBillCheckBox);
					AssertEquals(false, isMasterBillCheckBox.Visible);
					AssertEquals(string.Empty, isMasterBillCheckBox.BindTo);
					AssertEquals(false, isMasterBillCheckBox.Checked);

					billsTabControl.SelectedIndex = 2;
					var jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox = control.Controls.Find("JPB_Calc_GeneralCustomsTransitApprovalNumberTextBox", true).FirstOrDefault() as ZTextBox;
					AssertNotNull(jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox);
					AssertEquals(false, jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox.Visible);
					AssertEquals(string.Empty, isMasterBillCheckBox.BindTo);
					AssertEquals(false, isMasterBillCheckBox.Checked);

					var containersGrid = control.Controls.Find("ContainersGrid", true).FirstOrDefault() as ZGrid;
					AssertNotNull(containersGrid);
					AssertEquals("JPAFRNVOCCContainers", containersGrid.ColumnLayoutContext);
					AssertEquals(6, containersGrid.ColumnStyles.Count);
					AssertEquals(false, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_TypeOfService"));
					AssertEquals(false, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_VanningType"));
					AssertEquals(false, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_CCCApplicationId"));
					AssertEquals(false, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_SearchExclusionIdForCheckBox"));
				}
			}

			header.JPH_IsShippingLineEntry = true;
			using (var form = new ZForm(header))
			using (var control = new JPAFRBillsUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(header, "");
				form.Show();

				var billsTabControl = control.Controls.Find("BillDetailsTabControl", true).FirstOrDefault() as ZTabControl;
				billsTabControl.SelectedIndex = 0;

				var containerOperaterCodeTextBox = control.Controls.Find("ContainerOperaterCodeTextBox", true).FirstOrDefault() as ZTextBox;
				AssertNotNull(containerOperaterCodeTextBox);
				AssertEquals(true, containerOperaterCodeTextBox.Visible);
				AssertEquals("Bills.JPB_ContainerOperatorCode", containerOperaterCodeTextBox.BindTo);
				AssertEquals("COC01", containerOperaterCodeTextBox.Text);

				var isMasterBillCheckBox = control.Controls.Find("IsMasterBillCheckBox", true).FirstOrDefault() as ZCheckBox;
				AssertNotNull(isMasterBillCheckBox);
				AssertEquals(true, isMasterBillCheckBox.Visible);
				AssertEquals("Bills.JPB_IsMaterBill", isMasterBillCheckBox.BindTo);
				AssertEquals(true, isMasterBillCheckBox.Checked);

				billsTabControl.SelectedIndex = 2;
				var jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox = control.Controls.Find("JPB_Calc_GeneralCustomsTransitApprovalNumberTextBox", true).FirstOrDefault() as ZTextBox;
				AssertNotNull(jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox);
				AssertEquals(true, jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox.Visible);
				AssertEquals("Bills.JPB_Calc_GeneralCustomsTransitApprovalNumber", jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox.BindTo);
				AssertEquals("GTN01", jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox.Text);

				var containersGrid = control.Controls.Find("ContainersGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(containersGrid);
				AssertEquals("JPAFRVOCCContainers", containersGrid.ColumnLayoutContext);
				AssertEquals(10, containersGrid.ColumnStyles.Count);
				AssertEquals(true, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_TypeOfService"));
				AssertEquals(true, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_VanningType"));
				AssertEquals(true, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_CCCApplicationId"));
				AssertEquals(true, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_SearchExclusionIdForCheckBox"));
			}
		}
	}
}
