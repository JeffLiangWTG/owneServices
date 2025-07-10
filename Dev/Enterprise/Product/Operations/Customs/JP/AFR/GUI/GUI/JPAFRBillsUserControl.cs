using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class JPAFRBillsUserControl : ZUserControl
	{
		const string NVOCCContainerGridContext = "JPAFRNVOCCContainers";
		const string VOCCContainerGridContext = "JPAFRVOCCContainers";

		public JPAFRBillsUserControl()
		{
			InitializeComponent();

			if (JPAFRRegistry.Instance.AFRShowBLLFunctions.Value)
			{
				AddBLLFunctionMenuItems();
			}
		}

		BLLFuntionMenuHelper bllFunctionMenuHelper;

		void AddBLLFunctionMenuItems()
		{
			bllFunctionMenuHelper = new BLLFuntionMenuHelper(() => Header, () => CurrentBill);
			var deleteMenuItemindex = BillsGrid.DeleteMenuItem.Index;
			BillsGrid.ContextMenu.MenuItems.Add(deleteMenuItemindex + 1, bllFunctionMenuHelper.RegisterSplitMenuItem);
			BillsGrid.ContextMenu.MenuItems.Add(deleteMenuItemindex + 2, bllFunctionMenuHelper.RegisterSwitchMenuItem);
			BillsGrid.ContextMenu.MenuItems.Add(deleteMenuItemindex + 3, bllFunctionMenuHelper.RegisterMergeMenuItem);
			BillsGrid.ContextMenu.MenuItems.Add(deleteMenuItemindex + 4, bllFunctionMenuHelper.CancelSplitMenuItem);
			BillsGrid.ContextMenu.MenuItems.Add(deleteMenuItemindex + 5, bllFunctionMenuHelper.CancelSwitchMenuItem);
			BillsGrid.ContextMenu.MenuItems.Add(deleteMenuItemindex + 6, bllFunctionMenuHelper.CancelMergeMenuItem);
			BillsGrid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var bill = CurrentBill;

			var isBillAlreadyRegistered = bill?.IsBillAlreadyRegistered ?? false;
			var billBLLFunctionInfo = bill?.BLLFunctionInfo;
			var billBLLFuncitonCode = billBLLFunctionInfo?.JP_FunctionCode ?? -1;

			bllFunctionMenuHelper.CancelSplitMenuItem.Visible = isBillAlreadyRegistered
				&& billBLLFuncitonCode == (int)BLLFunctionCode.RegisterSplit;

			bllFunctionMenuHelper.CancelSwitchMenuItem.Visible = isBillAlreadyRegistered
				&& billBLLFuncitonCode == (int)BLLFunctionCode.RegisterSwitch;

			bllFunctionMenuHelper.CancelMergeMenuItem.Visible = isBillAlreadyRegistered
				&& billBLLFuncitonCode == (int)BLLFunctionCode.RegisterMerge;

			var registrationMenuVisibility = isBillAlreadyRegistered
				&& billBLLFunctionInfo == null;

			bllFunctionMenuHelper.RegisterSplitMenuItem.Visible = registrationMenuVisibility;
			bllFunctionMenuHelper.RegisterSwitchMenuItem.Visible = registrationMenuVisibility;
			bllFunctionMenuHelper.RegisterMergeMenuItem.Visible = registrationMenuVisibility;
		}

		JPAFRBills CurrentBill => (JPAFRBills)BillsGrid.GetCurrent();

		public JPAFRHeader Header => (JPAFRHeader)CurrentDataItem;

		void UpdateControlLayout(object sender, EventArgs e)
		{
			UpdateControlLayout(this.Header.JPH_IsShippingLineEntry);
		}

		void BillsGrid_AfterBind(object sender, EventArgs e)
		{
			BillsGrid.ListManager.PositionChanged += BillsGrid_PositionChanged;
			BillsGrid_PositionChanged(null, null);
		}

		void BillsGrid_PositionChanged(object sender, EventArgs e)
		{
			var listManager = BillsGrid.ListManager;
			if (listManager != null && listManager.Count > 0)
			{
				ContainersGridListManager_PositionChanged(sender, e);
			}
		}

		void ContainersGrid_AfterBind(object sender, EventArgs e)
		{
			ContainersGrid.ListManager.PositionChanged += ContainersGridListManager_PositionChanged;
			ContainersGridListManager_PositionChanged(null, null);
		}

		void ContainersGridListManager_PositionChanged(object sender, EventArgs e)
		{
			var listManager = ContainersGrid.ListManager;
			if (listManager != null)
			{
				if (listManager.Count > 0)
				{
					var current = (JPAFRContainer)listManager.GetCurrent();
					if (current != null)
					{
						bool isDiff = (currentContainer != current);
						if (isDiff)
						{
							UnHookContainerNumberValueChanged(currentContainer);
							currentContainer = current;
							HookContainerNumberValueChanged(currentContainer);
						}
					}
				}
				else
				{
					UnHookContainerNumberValueChanged(currentContainer);
					currentContainer = null;
				}
			}
		}
		JPAFRContainer currentContainer;

		static void UnHookContainerNumberValueChanged(JPAFRContainer container)
		{
			if (container != null)
			{
				container.ShouldChangeMatchedContainersOverride = null;
			}
		}

		static void HookContainerNumberValueChanged(JPAFRContainer container)
		{
			if (container != null)
			{
				container.ShouldChangeMatchedContainersOverride = ShouldChangeMatchedContainers;
			}
		}

		static bool ShouldChangeMatchedContainers(JPAFRContainer container, ZString oldValue, ZString newValue)
		{
			var result = false;
			var bill = container?.Bill;
			var header = bill?.Header;
			if (header != null)
			{
				result = header.GetMatchingContainers(container.PK, oldValue).Any() &&
					Globals.Message.Show(ResString.GetMultilingualString("35742CAD-7AF3-42A8-BB95-940A426D38E5", "Would you like to change all containers with Container Number '{0}' to '{1}' also?", oldValue, newValue), ResString.GetMultilingualString("A62376BA-92F9-49A0-9434-63B1B8651364", "Changing Matching Container"), MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes;
			}
			return result;
		}

		internal void UpdateControlLayout(bool isVOCC)
		{
			if (isVOCC)
			{
				this.BindingSource.SetBindingMember(this.ContainerOperaterCodeTextBox, "Bills.JPB_ContainerOperatorCode");
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JPAFRBills)(((System.Collections.IList)(((JPAFRHeader)(null)).Bills)).SyncRoot)).JPB_ContainerOperatorCode);
				this.BindingSource.SetBindingMember(this.JPB_Calc_GeneralCustomsTransitApprovalNumberTextBox, "Bills.JPB_Calc_GeneralCustomsTransitApprovalNumber");
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JPAFRBills)(((System.Collections.IList)(((JPAFRHeader)(null)).Bills)).SyncRoot)).JPB_Calc_GeneralCustomsTransitApprovalNumber);
				this.BindingSource.SetBindingMember(this.IsMasterBillCheckBox, "Bills.JPB_IsMaterBill");
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JPAFRBills)(((System.Collections.IList)(((JPAFRHeader)(null)).Bills)).SyncRoot)).JPB_IsMaterBill);
			}
			this.ContainerOperaterCodeTextBox.Visible = isVOCC;
			this.IsMasterBillCheckBox.Visible = isVOCC;
			this.JPB_Calc_GeneralCustomsTransitApprovalNumberTextBox.Visible = isVOCC;

			if (!isVOCC)
			{
				this.ContainersGrid.ColumnLayoutContext = NVOCCContainerGridContext;
				this.ContainersGrid.ColumnStyles.RemoveAt(9);
				this.ContainersGrid.ColumnStyles.RemoveAt(8);
				this.ContainersGrid.ColumnStyles.RemoveAt(7);
				this.ContainersGrid.ColumnStyles.RemoveAt(6);

				foreach (Control control in this.TemporaryLandingTabPage.Controls)
				{
					if (control != this.JPB_Calc_GeneralCustomsTransitApprovalNumberTextBox && control != this.OtherRelevantLawGroupBox)
					{
						var currentLocation = control.Location;
						control.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(currentLocation.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(currentLocation.Y) - 26);
					}
				}
			}
			else
			{
				this.ContainersGrid.ColumnLayoutContext = VOCCContainerGridContext;
			}
		}
	}
}
