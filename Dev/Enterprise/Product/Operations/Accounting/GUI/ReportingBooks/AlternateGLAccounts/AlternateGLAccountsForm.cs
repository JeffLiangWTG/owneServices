using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class AlternateGLAccountsForm : ZForm
	{
		public AlternateGLAccountsForm(AlternateGLAccounts alternateGLAccounts) : base(alternateGLAccounts)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			alternateGLAccounts.ChartPKInfo.ValueChanged += ChartChanged;
			alternateGLAccounts.ParentGLAccountPKInfo.ValueChanged += ParentGLAccountPKChanged;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.ValueChanged += AlternateGLAccountNumChanged;
			alternateGLAccountWithAttributeGridControl.Visible = false;
			singleAlternateGLAccountControl.Visible = true;
			alternateGLAccounts.Factory.Saved += Factory_Saved;
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Audit, 1, () => alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount);
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				IsNewForm = false;
				SetSingleAlternateGLAccountControlReadonly(false);
			}
		}

		protected override void OnShown(EventArgs e)
		{
			IsNewForm = DisplayMode == ODisplayMode.New;

			var alternateGLAccounts = BusinessEntity as AlternateGLAccounts;
			if (alternateGLAccounts != null && (IsNewForm || !alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().Any(x => x.Attributes.Cast<AccAlternateGLAccountAttribute>().Any(y => !string.IsNullOrEmpty(y.AAA_Attribute)))))
			{
				alternateAccountGroupBox.Visible = true;
				editAlternateAccountTabControl.Visible = false;
				MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 611, true);
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 611, true);
			}
			else
			{
				alternateAccountGroupBox.Visible = false;
				MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 761, true);
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 761, true);
				alternateGLAccountWithAttributeGridControl.Visible = false;
				singleAlternateGLAccountControl.Visible = false;
				relatedAlternateAccountsTabPage.TabInitialized += SetRelatedAlternateAccountsAttribute;
			}
		}

		void ChartChanged(object sender, EventArgs e)
		{
			ChartOrParentGLAccountPKChanged(sender as AlternateGLAccounts);
		}

		void ParentGLAccountPKChanged(object sender, EventArgs e)
		{
			if (sender is AlternateGLAccounts newAccAlternateGLAccounts)
			{
				var attributes = newAccAlternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>();
				if (editAlternateAccountTabControl.Visible
					&& attributes.Any(x => !string.IsNullOrEmpty(x.AAA_Attribute)))
				{
					var confirmMessage = Res.GetString("EEB290B8-102A-43BB-B087-CAC70EE4FA06", @"Parent Account '{0}' has dissection configuration with separate number configuration, clearing/changing the value will result in all saved Alternate Accounts be deleted.
Click 'Yes' to continue and system will delete all Alternate Accounts previously mapped against Parent Account '{0}'.
Click 'No' to cancel the change and revert back to original value.", attributes.FirstOrDefault()?.GLHeader.AG_AccountNum);
					if (Globals.Message.Show(confirmMessage, Res.GetString("fd37952c-430e-4a72-89ca-b8c11eca1c01", "Change Parent Account Confirmation"), ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.No) == ZDialogResult.Yes)
					{
						newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
						newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.ParentGLAccountPK = newAccAlternateGLAccounts.ParentGLAccountPK;
						ChartOrParentGLAccountPKChanged(newAccAlternateGLAccounts);
						newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeDeleted = true;
					}
					else
					{
						newAccAlternateGLAccounts.ResetParentGLAccountPK();
					}
				}
				else
				{
					newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.ParentGLAccountPK = newAccAlternateGLAccounts.ParentGLAccountPK;
					ChartOrParentGLAccountPKChanged(newAccAlternateGLAccounts);
				}
			}
		}

		void ChartOrParentGLAccountPKChanged(AlternateGLAccounts newAccAlternateGLAccounts)
		{
			if (newAccAlternateGLAccounts != null
					&& (IsNewForm
						|| (newAccAlternateGLAccounts.OriginalParentGLAccount?.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Any(x => x.ADC_AAC_AlternateChart == newAccAlternateGLAccounts.ChartPK) ?? false)))
			{
				newAccAlternateGLAccounts.ResetAlternateGLAccountsWithAttributeSet(newAccAlternateGLAccounts.ChartPK, newAccAlternateGLAccounts.ParentGLAccountPK);
				SetSingleAlternateGLAccountControlReadonly(false);
				MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 611, true);
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 611, true);
				if ((newAccAlternateGLAccounts.CreateMultipleAlternateGLAccount ?? false) && !alternateGLAccountWithAttributeGridControl.Visible)
				{
					editAlternateAccountTabControl.Visible = false;
					alternateGLAccountWithAttributeGridControl.Visible = true;
					singleAlternateGLAccountControl.Visible = false;
					SetAttributeVisible(alternateGLAccountWithAttributeGridControl, newAccAlternateGLAccounts);
					(alternateGLAccountWithAttributeGridControl.Controls.Find("attributeGrid", true)[0] as ZGrid).RefreshTableStyles();
				}
				else if (!(newAccAlternateGLAccounts.CreateMultipleAlternateGLAccount ?? true) && !singleAlternateGLAccountControl.Visible)
				{
					editAlternateAccountTabControl.Visible = false;
					alternateGLAccountWithAttributeGridControl.Visible = false;
					singleAlternateGLAccountControl.Visible = true;
				}
				newAccAlternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.ValueChanged += AlternateGLAccountNumChanged;
			}
		}

		void SetRelatedAlternateAccountsAttribute(object sender, EventArgs e)
		{
			SetAttributeVisible(alternateGLAccountWithAttributeGridInTabControl, BusinessEntity as AlternateGLAccounts);
		}

		void UnhookEvents()
		{
			if (BusinessEntity is AlternateGLAccounts alternateGLAccounts)
			{
				alternateGLAccounts.ChartPKInfo.ValueChanged -= ChartChanged;
				alternateGLAccounts.ParentGLAccountPKInfo.ValueChanged -= ParentGLAccountPKChanged;
				relatedAlternateAccountsTabPage.TabInitialized -= SetRelatedAlternateAccountsAttribute;
				alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.ValueChanged -= AlternateGLAccountNumChanged;
				alternateGLAccounts.Factory.Saved -= Factory_Saved;
			}
		}

		void SetAttributeVisible(AlternateGLAccountWithAttributeGridControl alternateGLAccountWIthAttributeGridControl, AlternateGLAccounts newAccAlternateGLAccounts)
		{
			var grid = alternateGLAccountWIthAttributeGridControl.Controls.Find("attributeGrid", true)[0] as ZGrid;
			SetAttributeVisibleCore(grid, newAccAlternateGLAccounts, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, "OrganizationCode");
			SetAttributeVisibleCore(grid, newAccAlternateGLAccounts, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCGDescription");
			SetAttributeVisibleCore(grid, newAccAlternateGLAccounts, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, "LFEDescription");
			SetAttributeVisibleCore(grid, newAccAlternateGLAccounts, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, "LFODescription");
			SetAttributeVisibleCore(grid, newAccAlternateGLAccounts, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, "TICDescription");
			SetAttributeVisibleCore(grid, newAccAlternateGLAccounts, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, "SPRDescription");
			grid.RefreshTableStyles();
		}

		void SetAttributeVisibleCore(ZGrid grid, AlternateGLAccounts newAccAlternateGLAccounts, string attributeName, string columnName)
		{
			if (grid != null)
			{
				var column = grid.Columns.FirstOrDefault(x => x.ColumnName == columnName);
				if (column != null)
				{
					if (newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.Attributes.Cast<AccAlternateGLAccountAttribute>().All(y => y.AAA_Attribute != attributeName)))
					{
						column.IsVisible = false;
					}
					else
					{
						column.IsVisible = true;
					}
				}
			}
		}

		void AlternateGLAccountNumChanged(object sender, EventArgs e)
		{
			if (sender is AlternateGLAccountWithAttributeSet alternateGLAccountWithAttributeSet && e is ValueChangedEventArgs valueChangedEventArgs)
			{
				if (alternateGLAccountWithAttributeSet.AlternateGLAccountNumExistsInDatabase(valueChangedEventArgs.NewValue.ToString()))
				{
					var confirmMessage = Res.GetString("1760413E-E011-40C0-A11E-0640C0487BCC", @"Alternate Account '{0}' is no longer mapped to any Parent Account with this update and will be deleted.
Click 'Yes' to continue. 
Click 'No' to cancel the change and revert back to the original value.", valueChangedEventArgs.OldValue.ToString());

					if (!IsNewForm
						&& alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().All(x => string.IsNullOrEmpty(x.AAA_Attribute))
						&& !alternateGLAccountWithAttributeSet.OriginalAlternateGLAccountHasOtherParent()
						&& Globals.Message.Show(confirmMessage, Res.GetString("08b26215-feec-4c44-bd28-3eac05975e59", "Change Alternate Account Number Confirmation"), ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.No) == ZDialogResult.No)
					{
						alternateGLAccountWithAttributeSet.ResetAlternateGLAccountNum();
					}
					else if (IsNewForm && singleAlternateGLAccountControl.Visible && !parentAccountGuidFindBox.ReadOnly)
					{
						SetSingleAlternateGLAccountControlReadonly(true);
					}
				}
				else if (IsNewForm && singleAlternateGLAccountControl.Visible)
				{
					SetSingleAlternateGLAccountControlReadonly(false);
				}
			}
		}

		void SetSingleAlternateGLAccountControlReadonly(bool isReadonly)
		{
			foreach (Control control in singleAlternateGLAccountControl.Controls.Find("singleAlternateGLAccountPanel", true)[0].Controls)
			{
				if (control.Name != "accountNumberTextBox" && control.Name != "existAlternateAccountLabel")
				{
					control.Enabled = !isReadonly;
				}
				else if (control.Name == "existAlternateAccountLabel")
				{
					control.Visible = isReadonly;
				}
			}
		}

		public override string FormCaption
		{
			get
			{
				return ResString.GetMultilingualString("E3612C22-C433-4E2A-85B8-34FE196890E0", "Alternate GL Account");
			}
		}

		protected override DialogResult ShowErrorsDialogCore(bool includeIgnoreOption)
		{
			try
			{
				AlternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().ForEach(x => x.UnRegisterEditableChildObject(x.AlternateGLAccount));
				return base.ShowErrorsDialogCore(includeIgnoreOption);
			}
			finally
			{
				AlternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().ForEach(x => x.RegisterEditableChildObject(x.AlternateGLAccount));
			}
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			if (!Env.Security.AlternateGLAccountsDelete.IsAllowed)
			{
				Env.Security.AlternateGLAccountsDelete.ShowError();
				return ContinueWithDelete.No;
			}
			else if (AlternateGLAccounts != null)
			{
				var currentAltrenateGLAccount = AlternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount;
				var deleteCaption = Res.GetString("44731869-A4AA-4DD9-851B-CC958AF7D0DB", "Delete Alternate GL Account");
				if (!currentAltrenateGLAccount.CanDelete)
				{
					Globals.Message.ShowError(currentAltrenateGLAccount.ReasonForNotAbleToDelete, deleteCaption);
					return ContinueWithDelete.No;
				}
				else if (currentAltrenateGLAccount.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>().Any(x => !string.IsNullOrEmpty(x.AAA_Attribute)))
				{
					var confirmationMessage = Res.GetString("A240B997-5363-40E6-B464-1092FF934303", @"The selected Alternate Account '{0}' has related Alternate Accounts mapped to the same Parent Account '{1}'.
You can view the list of related Alternate Accounts via the 'Related Alternate Accounts' tab.
Click 'Yes' to continue and system will delete the selected Alternate Account '{0}'.
Click 'No' to cancel the action.", currentAltrenateGLAccount.AGA_AccountNum, AlternateGLAccounts.ParentGLAccount.AG_AccountNum);
					if (Globals.Message.Show(confirmationMessage, deleteCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						return ContinueWithDelete.Yes;
					}
					else
					{
						return ContinueWithDelete.No;
					}
				}
				else
				{
					if (currentAltrenateGLAccount.AlternateGLAccountAttributes.Count > 1)
					{
						var parentsMessage = string.Join("\r\n", currentAltrenateGLAccount.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>().Select(x => $"{x.GLHeader.AccountNum} - {x.GLHeader.AG_DescriptionMultilingual}"));
						var confirmationMessage = Res.GetString("4D2B4542-A969-4448-B2A8-927AFC5DE491", @"The selected Alternate Account '{0}' is mapped to the following Parent Accounts:
{1}

Click 'Yes' to continue and system will delete the Alternate Accounts and related mappings.
Click 'No' to cancel the action.", currentAltrenateGLAccount.AGA_AccountNum, parentsMessage);
						if (Globals.Message.Show(confirmationMessage, deleteCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							return ContinueWithDelete.Yes;
						}
						else
						{
							return ContinueWithDelete.No;
						}
					}
					else
					{
						return base.ShowPreDeleteDialogs();
					}
				}
			}
			else
			{
				return base.ShowPreDeleteDialogs();
			}
		}

		protected override void DeleteCore()
		{
			if (AlternateGLAccounts != null)
			{
				AlternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.Delete();
				AlternateGLAccounts.AlternateGLAccountsWithAttributeSet.Where(x => !x.AlternateGLAccount.IsInDatabase).DeleteAll();
			}
			else
			{
				base.DeleteCore();
			}
		}

		public override Guid IdentifierForPersistingForm
		{
			get
			{
				return AlternateGLAccounts?.FirstAlternateGLAccountWithAttributeSet?.Attributes.FirstOrDefault()?.PK.ToGuid() ?? AlternateGLAccounts?.FirstAlternateGLAccountWithAttributeSet?.AlternateGLAccount.PK.ToGuid() ?? Guid.Empty;
			}
		}

		protected override void SaveToRecentItems()
		{
			if (AlternateGLAccounts?.FirstAlternateGLAccountWithAttributeSet?.Attributes.Any() ?? false)
			{
				AccountingMasterFilesUtils.UpdateStmLinkDescription(ModuleIDs.AlternateGLAccounts.Name, AlternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>().ToDictionary(x => x.PK, y => y.HumanReadableShortcutName));
			}

			base.SaveToRecentItems();
		}

		AlternateGLAccounts AlternateGLAccounts => BusinessEntity as AlternateGLAccounts;

		bool IsNewForm;
	}
}
