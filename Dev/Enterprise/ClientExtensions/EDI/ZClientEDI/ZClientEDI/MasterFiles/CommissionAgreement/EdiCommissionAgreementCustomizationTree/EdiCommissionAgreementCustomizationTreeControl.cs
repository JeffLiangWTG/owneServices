using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class EdiCommissionAgreementCustomizationTreeControl : ZUserControl, IReadOnlyToggleControl
	{
		public EdiCommissionAgreementCustomizationTreeControl()
		{
			InitializeComponent();

			SetupTree();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupAddCountryDropDownButton();
		}

		#region CurrentDataItem

		EdiCommissionAgreementCustomization ComissionAgreementCustomization
		{
			get { return (EdiCommissionAgreementCustomization)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (ComissionAgreementCustomization != null)
			{
				ComissionAgreementCustomization.EZN_IsAllCompaniesInfo.ValueChanged -= EZN_IsAllCompaniesInfo_ValueChanged;
				ComissionAgreementCustomization.EZN_IsAllDatabasesInfo.ValueChanged -= EZN_IsAllDatabasesInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (ComissionAgreementCustomization != null)
			{
				var model = new EdiCommissionAgreementCustomizationTreeModel(ComissionAgreementCustomization);
				treeView.Model = new EdiCommissionAgreementCustomizationTreeModelView(model);
				ExpandTreeToCountryLevel();

				treeView.SetSortColumn(codeTreeColumn, System.Windows.Forms.SortOrder.Ascending);
				ComissionAgreementCustomization.EZN_IsAllCompaniesInfo.ValueChanged += EZN_IsAllCompaniesInfo_ValueChanged;
				ComissionAgreementCustomization.EZN_IsAllDatabasesInfo.ValueChanged += EZN_IsAllDatabasesInfo_ValueChanged;
			}
			else
			{
				treeView.Model = null;
			}

			RefreshAddCountryMenuItems();
		}

		void EZN_IsAllCompaniesInfo_ValueChanged(object sender, EventArgs e)
		{
			treeView.EndUpdate();
			RefreshAddCountryDropDownButtonReadOnly();
		}

		void EZN_IsAllDatabasesInfo_ValueChanged(object sender, EventArgs e)
		{
			treeView.EndUpdate();
		}

		void ExpandTreeToCountryLevel()
		{
			foreach (var rootChildren in treeView.Root.Children)
			{
				rootChildren.Expand();
			}
		}

		#endregion

		#region Tree

		void SetupTree()
		{
			foreach (var column in treeView.Columns)
			{
				column.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(column.Width);
			}

			treeView.SortPropertiesNeeded += TreeView_SortPropertiesNeeded;

#if !WINZOR
			treeView.RowDraw += TreeView_RowDraw;
#endif

			codeTextBox.DrawText += CodeTextBox_DrawText;
			selectedCheckBox.IsEditEnabledValueNeeded += SelectedCheckBox_IsEditEnabledValueNeeded;
			selectedCheckBox.IsVisibleValueNeeded += SelectedCheckBox_IsVisibleValueNeeded;
			autoAddCheckBox.IsEditEnabledValueNeeded += AutoAddCheckBox_IsEditEnabledValueNeeded;
			autoAddCheckBox.IsVisibleValueNeeded += AutoAddCheckBox_IsVisibleValueNeeded;
			includeDatabaseUsageCheckBox.IsEditEnabledValueNeeded += IncludeDatabaseUsageCheckBox_IsEditEnabledValueNeeded;
			includeDatabaseUsageCheckBox.IsVisibleValueNeeded += IncludeDatabaseUsageCheckBox_IsVisibleValueNeeded;
		}

		void TreeView_SortPropertiesNeeded(object sender, ZTreeViewSortEventArgs e)
		{
			if (e.Column == codeTreeColumn)
			{
				// only sort on 'Code' property (not 'Selected' + 'Code')
				e.SortProperties = new List<Tuple<string, System.Windows.Forms.SortOrder>>()
				{
					Tuple.Create("Code", e.SortOrder)
				};
			}

			// Always made 'new databases' appear on the bottom
			e.SortProperties.Insert(0, Tuple.Create("IsForNewDatabases", System.Windows.Forms.SortOrder.Ascending));
		}

#if !WINZOR
		static void TreeView_RowDraw(object sender, TreeViewRowDrawEventArgs e)
		{
			var node = (EdiCommissionAgreementCustomizationTreeNode)e.Node.Tag;
			if (node != null)
			{
				var countryWrapper = node.BizObj as EdiCommissionAgreementCountryWrapper;
				if (countryWrapper != null)
				{
					DrawRowBackground(e, SystemBrushes.Menu);
				}
			}
		}

		static void DrawRowBackground(TreeViewRowDrawEventArgs e, Brush brush)
		{
			var rect = ControlDpiScalingHelper.NewScaledRectangle(0, e.RowRect.Y, e.ClipRectangle.Width, e.RowRect.Height, false);
			e.Graphics.FillRectangle(brush, rect);
		}
#endif

		static void CodeTextBox_DrawText(object sender, DrawEventArgs e)
		{
			var node = (EdiCommissionAgreementCustomizationTreeNode)e.Node.Tag;
			if (node != null && node.BizObj is EdiCommissionAgreementDatabaseWrapper)
			{
				e.Font = new Font(e.Font, FontStyle.Bold);
			}
		}

		static void SelectedCheckBox_IsEditEnabledValueNeeded(object sender, NodeControlValueEventArgs e)
		{
			var node = (EdiCommissionAgreementCustomizationTreeNode)e.Node.Tag;
			if (node != null && node.Selected_ReadOnly)
			{
				e.Value = false;
			}
		}

		static void SelectedCheckBox_IsVisibleValueNeeded(object sender, NodeControlValueEventArgs e)
		{
			var node = (EdiCommissionAgreementCustomizationTreeNode)e.Node.Tag;
			if (node != null && !node.Selected_Visible)
			{
				e.Value = false;
			}
		}

		static void AutoAddCheckBox_IsEditEnabledValueNeeded(object sender, NodeControlValueEventArgs e)
		{
			var node = (EdiCommissionAgreementCustomizationTreeNode)e.Node.Tag;
			if (node != null && node.ShouldAutoAdd_ReadOnly)
			{
				e.Value = false;
			}
		}

		static void AutoAddCheckBox_IsVisibleValueNeeded(object sender, NodeControlValueEventArgs e)
		{
			var node = (EdiCommissionAgreementCustomizationTreeNode)e.Node.Tag;
			if (node != null && !node.ShouldAutoAdd_Visible)
			{
				e.Value = false;
			}
		}

		static void IncludeDatabaseUsageCheckBox_IsEditEnabledValueNeeded(object sender, NodeControlValueEventArgs e)
		{
			var node = (EdiCommissionAgreementCustomizationTreeNode)e.Node.Tag;
			if (node != null && node.IncludeDatabaseUsage_ReadOnly)
			{
				e.Value = false;
			}
		}

		static void IncludeDatabaseUsageCheckBox_IsVisibleValueNeeded(object sender, NodeControlValueEventArgs e)
		{
			var node = (EdiCommissionAgreementCustomizationTreeNode)e.Node.Tag;
			if (node != null && !node.IncludeDatabaseUsage_Visible)
			{
				e.Value = false;
			}
		}

#endregion

#region Add Countries Button

		void SetupAddCountryDropDownButton()
		{
			var addButtonImage = Icons.GetImage(IconTypes.BlackWhite_Add);
			addCountriesDropDownButton.Image = addButtonImage;
			RefreshAddCountryDropDownButtonReadOnly();
		}

		void RefreshAddCountryDropDownButtonReadOnly()
		{
			var shouldBeReadOnly =
				(ParentForm != null && ((ZForm)ParentForm).DisplayMode == ODisplayMode.ReadOnly) ||
				ComissionAgreementCustomization == null ||
				ComissionAgreementCustomization.EZN_IsAllCompanies;

			addCountriesDropDownButton.Enabled = !shouldBeReadOnly;
		}

		void RefreshAddCountryMenuItems()
		{
			for (var i = addCountryForAllExistingDatabasesMenuItem.DropDownItems.Count - 1; i > addCountryForAllExistingDatabasesMenuItem.DropDownItems.IndexOf(addCountryForExistingDatabasesStripSeparator); i--)
			{
				addCountryForAllExistingDatabasesMenuItem.DropDownItems.RemoveAt(i);
			}

			if (ComissionAgreementCustomization == null)
			{
				return;
			}

			var licEnterprise = ComissionAgreementCustomization.LicenceEnterprise;
			if (licEnterprise == null)
			{
				return;
			}

			foreach (var database in licEnterprise.Databases.Cast<LicenceDatabase>().OrderBy(x => x.LD_ServerCode))
			{
				var text = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", database.LD_ServerCode, database.LD_LicenceType);
				var databaseMenuItem = new ZToolStripMenuItem(text, (sender, e) => ShowAddCompanyCountryForm(database));
				addCountryForExistingDatabasesMenuItem.DropDownItems.Add(databaseMenuItem);
			}
		}

		void AddCountryForNewDatabasesMenuItem_Click(object sender, EventArgs e)
		{
			ShowAddCompanyCountryForm(null);
		}

		void AddCountryForAllExistingDatabasesMenuItem_Click(object sender, EventArgs e)
		{
			if (ComissionAgreementCustomization == null)
			{
				return;
			}

			var licEnterprise = ComissionAgreementCustomization.LicenceEnterprise;
			if (licEnterprise == null)
			{
				Globals.Message.ShowError("No enterprise licence exists for this customer");
				return;
			}

			var addCompanyCountries = new AddEdiCommissionAgreementCompanyAutoAddCountriesAction(ComissionAgreementCustomization, licEnterprise.Databases.Cast<LicenceDatabase>().Select(x => x.PK).ToArray());
			var form = new AddEdiCommissionAgreementCompanyAutoAddCountriesForm(addCompanyCountries);
			form.FormClosed += AddCompanyCountryForm_FormClosed;
			ZFormModaliser.Show(form, ParentForm);
		}

		void ShowAddCompanyCountryForm(LicenceDatabase database)
		{
			if (ComissionAgreementCustomization == null)
			{
				return;
			}

			var addCompanyCountries = new AddEdiCommissionAgreementCompanyAutoAddCountriesAction(ComissionAgreementCustomization, database != null ? database.PK : ZGuid.Empty);
			var form = new AddEdiCommissionAgreementCompanyAutoAddCountriesForm(addCompanyCountries);
			form.FormClosed += AddCompanyCountryForm_FormClosed;
			ZFormModaliser.Show(form, ParentForm);
		}

		void AddCompanyCountryForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			var form = (ZForm)sender;
			form.FormClosed -= AddCompanyCountryForm_FormClosed;

			if (form.DialogResult == DialogResult.OK)
			{
				var modelView = (EdiCommissionAgreementCustomizationTreeModelView)treeView.Model;
				if (modelView != null)
				{
					modelView.Rebuild();
					ExpandTreeToCountryLevel();
				}
			}
		}

#endregion

#region ReadOnly

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				if (readOnly != value)
				{
					readOnly = value;
					resetToDefaultButton.Visible = !readOnly;
					treeView.ReadOnly = value;
				}
			}
		}
		bool readOnly;

#endregion

		void ResetToDefaultButton_Click(object sender, EventArgs e)
		{
			if (ComissionAgreementCustomization != null)
			{
				var dialogResult = Globals.Message.Show(
					"Are you sure you want to reset all customer filters?",
					"Reset Customer Filters",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Exclamation);

				if (dialogResult == DialogResult.Yes)
				{
					ComissionAgreementCustomization.ResetToDefault();
				}
			}
		}
	}
}
