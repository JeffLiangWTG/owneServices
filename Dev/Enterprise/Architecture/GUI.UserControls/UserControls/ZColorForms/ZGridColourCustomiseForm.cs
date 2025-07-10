using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZGridColourCustomiseForm : ZChildForm, IZGridColourCustomiseForm
	{
		#region Construction

		public ZGridColourCustomiseForm(GridColourScheme scheme, FilterStripBusinessObject filterStripBusinessObject, ZFilterStripCommonControl stripControl, Type businessEntityType)
			: base(scheme)
		{
			InitializeComponent();
			DefaultTabPage.Text = DefaultRuleName;
			IsPostOnly = true;
			this.stripControl = stripControl;
			this.businessEntityType = businessEntityType;

			currentColourScheme = scheme;

			filterStripBO = filterStripBusinessObject;

			if (scheme.IsInDatabase)
			{
				PopulateStrips();
			}

			if (currentColourScheme.S9_IsSystem)
			{
				UpdateStatusBar(Res.GetString("d5a1f8d5-cdb2-4bcb-9002-b1396d35beb7", "System defined schemes cannot be modified."), null);
				DisableAllControlsButTabs();
			}
			else
			{
				if (!EnvProxy.Instance.Security.PublishGlobalGridColorSchemes.IsAllowedForAllBranches)
				{
					UpdateStatusBar(Res.GetString("a70b4cc2-d8ad-41ec-9295-64bf9c290ce2", "You don't have permission to publish schemes across all companies."), null);
					checkBoxIsPublishedAcrossAllCompanies.Enabled = false;
				}

				if (!EnvProxy.Instance.Security.PublishGlobalGridColorSchemes.IsAllowed)
				{
					UpdateStatusBar(Res.GetString("ca3d2357-99e4-44d8-b222-b4db0fac0966", "You don't have permission to publish schemes."), null);
					PublishCheckBox.Enabled = false;
				}

				// other user's schemes
				if (currentColourScheme.S9_RelatedEntityID != EnvProxy.Instance.CurrentUser.PK)
				{
					if (currentColourScheme.PublishAcrossAllCompanies
							&& !EnvProxy.Instance.Security.EditAllGlobalColourSchemes.IsAllowedForAllBranches)
					{
						UpdateStatusBar(Res.GetString("5559C44B-6FC5-4539-95E3-86C0EC538D8B", "You don't have permission to edit other users' schemes published across all companies."), null);
						DisableAllControlsButTabs();
					}
					else if (!EnvProxy.Instance.Security.EditAllGlobalColourSchemes.IsAllowed)
					{
						UpdateStatusBar(Res.GetString("91D124AC-FAF9-41FA-BF63-A7AB292B9A77", "You don't have permission to edit other users' published schemes."), null);
						DisableAllControlsButTabs();
					}
				}
			}

			RulesTabControl.SelectedIndexChanging += delegate
			{ TabChanging(); };
			RulesTabControl.SelectedIndexChanged += delegate
			{ TabChanged(); };

			currentColourScheme.HasChangesChanged += MyScheme_HasChangesChanged;
			currentColourScheme.HasChanges = false;
		}

		internal GridColourScheme currentColourScheme;
		FilterStripBusinessObject filterStripBO;

#if DEBUG
		internal
#endif
		readonly ZFilterStripCommonControl stripControl;
		protected ZCheckBox checkBoxIsPublishedAcrossAllCompanies;
		readonly Type businessEntityType;

		ZFilterStripCommonControl IZGridColourCustomiseForm.StripControl => stripControl;
		DialogResult IZGridColourCustomiseForm.ShowDialog() => ShowDialog();
		IButtonControl IZGridColourCustomiseForm.CancelButton => CancelButton;

		#endregion

		#region Form overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			//loading the strips causes has changes true, reset it when form is loaded.
			if (currentColourScheme != null)
			{
				currentColourScheme.HasChanges = false;
			}
		}

		protected override void ZForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (currentColourScheme.HasChanges && !currentColourScheme.IsDeleted)
			{
				base.ZForm_Closing(sender, e);

				if (!e.Cancel && currentColourScheme.HasChanges)
				{
					currentColourScheme.CancelChanges();
					//reset filter strips
					if (!currentColourScheme.IsDeleted)
					{
						currentColourScheme.ResetStrips();
						currentColourScheme.SetStripsFromFilter(filterStripBO, businessEntityType);
					}
				}
			}
			currentColourScheme.HasChangesChanged -= MyScheme_HasChangesChanged;
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			foreach (GridColourStripBusinessObject strip in currentColourScheme.ColourStrips)
			{
				strip.SaveLayout(strip.RuleName, currentColourScheme.PublishAcrossAllCompanies);
			}

			if (currentColourScheme.IsInDatabase)
			{
				base.Save(factories);
			}
			else
			{
				currentColourScheme.Factory.Save();
			}
		}

		#endregion

		#region Button Clicks

		void RemoveSchemeButton_Click(object sender, EventArgs e)
		{
			RemoveScheme();
		}

		void RenameRuleButton_Click(object sender, EventArgs e)
		{
			RenameRule();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			try
			{
				SaveScheme();
			}
			catch (ZSaveConcurrencyException ex)
			{
				HandleSaveConcurrencyException(ex);
			}
			catch (ZSaveException ex)
			{
				HandleSaveException(ex);
			}
			catch (ZCannotSaveException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void HandleSaveConcurrencyException(ZSaveConcurrencyException ex)
		{
			RowFactory.ClearSpecificTableFromUberFactory(StmModuleFilter.Schema.TableName);
			RowFactory.ClearSpecificTableFromUberFactory(StmModuleFilterUserData.Schema.TableName);

			Globals.Message.ShowError(new ConcurrencyExceptionHandler(ex).UserFriendlyMessage);

			foreach (var control in Controls.Cast<Control>())
			{
				control.Enabled = false;
			}
			CloseButton.Enabled = true;
		}

		void RemoveRuleButton_Click(object sender, EventArgs e)
		{
			RemoveSelectedRule();
		}

		void AddNewRuleButton_Click(object sender, EventArgs e)
		{
			AddNewRule();
		}

		#endregion

		#region RemoveScheme

		internal void RemoveScheme()
		{
			var result = Globals.Message.Show(
				Res.GetString("ZGridColourCustomiseForm|ConfirmSchemeRemove", "This color scheme and all its rules will be removed. Are you sure you want to continue?"),
				Res.GetString("ZGridColourCustomiseForm|RemoveSchemeCaption", "Remove Color Scheme"),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				foreach (var strip in currentColourScheme.ColourStrips)
				{
					var filter = strip.StmModuleFilter;

					if (filter != null)
					{
						filter.Delete();
					}

					strip.Delete();
				}

				currentColourScheme.Delete();
				currentColourScheme.Factory.Save();

				Close();
			}
		}

		#endregion

		#region RenameRule

		internal void RenameRule()
		{
			if (RulesTabControl.SelectedTab.Text != DefaultRuleName)
			{
				var selectedStrip = (GridColourStripBusinessObject)RulesTabControl.SelectedTab.Tag;

				if (selectedStrip != null)
				{
					string oldName = RulesTabControl.SelectedTab.Text;

					var ruleNameForm = new ZColorSchemeRuleForm(selectedStrip);
					var result = ZFormModaliser.ShowDialogAndDispose(ruleNameForm);
					if (result != DialogResult.Cancel && !selectedStrip.HasErrors)
					{
						currentColourScheme.HasChanges = true;
						RulesTabControl.SelectedTab.Text = selectedStrip.RuleName;
					}
					else
					{
						selectedStrip.RuleName = oldName;
					}
				}
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("ZGridColourCustomiseForm|CantRenameDefaultRule", "You can't rename the default rule"));
			}
		}

		#endregion

		#region SwapRule

		bool SwapRule(int colorStripIndex1, int colorStripIndex2)
		{
			if (colorStripIndex1 >= currentColourScheme.ColourStrips.Count || colorStripIndex2 >= currentColourScheme.ColourStrips.Count)
			{
				return false;
			}
			var tempStrip = currentColourScheme.ColourStrips[colorStripIndex1];
			currentColourScheme.ColourStrips[colorStripIndex1] = currentColourScheme.ColourStrips[colorStripIndex2];
			currentColourScheme.ColourStrips[colorStripIndex2] = tempStrip;
			currentColourScheme.MyScheme_HasChanges(this, EventArgs.Empty);
			return true;
		}

		#endregion

		#region DisableAllControlsButTabs

		void DisableAllControlsButTabs()
		{
			foreach (Control ctr in Controls)
			{
				if (!(ctr is ZTabControl || ctr is ZTabPage || ctr == MainStatusBar || ctr == CloseButton))
				{
					ctr.Enabled = false;
				}
			}
			foreach (ZTabPage tabPage in RulesTabControl.TabPages)
			{
				foreach (Control tabControl in tabPage.Controls)
				{
					foreach (Control control in tabControl.Controls)
					{
						control.Enabled = false;
					}
					tabControl.Enabled = false;
				}
			}
		}

		#endregion

		#region SchemeHasChanges

		void MyScheme_HasChanges(object sender, EventArgs e)
		{
			currentColourScheme.HasChanges = true;
		}

		void MyScheme_HasChangesChanged(object sender, EventArgs e)
		{
			SaveButton.Enabled = currentColourScheme.HasChanges;
			SuspendLayout();

			try
			{
				foreach (ZTabPage tab in RulesTabControl.TabPages)
				{
					tab.Refresh();
				}
				RulesTabControl.Refresh();
			}
			finally
			{
				ResumeLayout();
				Invalidate(true); // force a call to OnPaint to remove some lines removing splits cause.
			}
		}

		#endregion

		#region TabChange

		void TabChanged()
		{
			if (!mySchemeHasChangesBeforeTabChange)
			{
				currentColourScheme.HasChanges = false;
			}
		}

		void TabChanging()
		{
			mySchemeHasChangesBeforeTabChange = currentColourScheme.HasChanges;
		}

		bool mySchemeHasChangesBeforeTabChange;

		#endregion

		#region RemoveSelectedRule

		internal void RemoveSelectedRule()
		{
			var selectedTab = RulesTabControl.SelectedTab;

			if (selectedTab != null && selectedTab.GetType() != typeof(ZStmALogTabPage))
			{
				var gridColorStripToRemove = (GridColourStripBusinessObject)selectedTab.Tag;

				if (gridColorStripToRemove != null)
				{
					var filter = gridColorStripToRemove.StmModuleFilter;

					if (filter != null)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						currentColourScheme.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord, "Deleted Rule Name: '" + filter.S9_FilterName + "'");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						filter.Delete();
					}
					currentColourScheme.ColourStrips.Remove(gridColorStripToRemove);
					RulesTabControl.TabPages.Remove(selectedTab);
					currentColourScheme.HasChanges = true;
				}
				if (currentColourScheme.ColourStrips.Count == 0 && selectedTab.Text != DefaultRuleName)
				{
					DefaultTabPage.Text = DefaultRuleName;
					DefaultTabPage.Tag = null;
					RulesTabControl.TabPages.Insert(DefaultTabPage, 0);
					DefaultTabPage.TabVisible = true;
				}
			}
		}

		static string DefaultRuleName { get { return Res.GetString("3a7bcb27-55e4-4861-a1fc-ddf19ae19ba7", "Rule"); } }

		#endregion

		#region PopulateStrips

		public void NewColourStrip(FilterStripBusinessObject filter)
		{
			filterStripBO = filter;
			currentColourScheme = filterStripBO.Factory.New<GridColourScheme>();
		}

		void PopulateStrips()
		{
			RulesTabControl.TabPages.Remove(DefaultTabPage);
			DefaultTabPage.Text = "";

			SchemeNameTextBox.Text = currentColourScheme.S9_FilterNameMultilingual.GetUnresolvedString();
			int count = 0;

			foreach (var gridColorStrip in currentColourScheme.ColourStrips)
			{
				gridColorStrip.LoadLayout(gridColorStrip.StmModuleFilter);

				foreach (var filter in gridColorStrip.ModuleFilters)
				{
					filter.ModuleFilterChanged += MyScheme_HasChanges;
				}

				var ruleTab = new ZTabPage();
				ruleTab.AutoScroll = true;
				ruleTab.Text = gridColorStrip.RuleName;
				ruleTab.Tag = gridColorStrip;
				RulesTabControl.TabPages.Insert(ruleTab, count);
				count++;
				var gridColourStripControl = new ZGridColourStripControl(gridColorStrip, stripControl);
				gridColourStripControl.BGColour = gridColorStrip.BGColor;
				ruleTab.Controls.Add(gridColourStripControl);
			}
		}

		#endregion

		#region Add new rule

		void AddNewRule()
		{
			var colourStrip = new GridColourStripBusinessObject(filterStripBO, currentColourScheme, businessEntityType, true);
			var ruleNameForm = new ZColorSchemeRuleForm(colourStrip);

			if (ZFormModaliser.ShowDialogAndDispose(ruleNameForm) == DialogResult.OK && !colourStrip.HasErrors)
			{
				string ruleName = colourStrip.RuleName;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				currentColourScheme.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord, "Added Rule Name: '" + ruleName + "'");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				ZTabPage ruleTab;
				if (DefaultTabPage.Text == DefaultRuleName)
				{
					DefaultTabPage.Text = ruleName;
					ruleTab = DefaultTabPage;
				}
				else
				{
					ruleTab = new ZTabPage();
					ruleTab.Text = ruleName;
					RulesTabControl.TabPages.Insert(ruleTab, RulesTabControl.TabPages.Count - 1);
				}

				ruleTab.Tag = colourStrip;
				ruleTab.HorizontalScroll.Maximum = 0;
				ruleTab.AutoScroll = false;
				ruleTab.VerticalScroll.Visible = false;
				ruleTab.AutoScroll = true;

				RulesTabControl.SelectTab(ruleTab);
				ruleTab.Select();
				ruleTab.Focus();

				var colourStripControl = new ZGridColourStripControl(colourStrip, stripControl);

				colourStripControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 69);
				ruleTab.Controls.Add(colourStripControl);
				colourStripControl.Show();
				ruleTab.Refresh();
				ruleTab.Update();
				currentColourScheme.ColourStrips.Add(colourStrip);
				currentColourScheme.HasChanges = true;

				colourStrip.LayoutChanged += MyScheme_HasChanges;
			}
		}

		#endregion

		#region Save Scheme

		internal void SaveScheme()
		{
			string name = SchemeNameTextBox.Text;
			currentColourScheme.S9_FilterName = "";
			currentColourScheme.S9_FilterName = name;

			ContinueWithSave continueWithSave = ContinueWithSave.Yes;
			if (currentColourScheme.HasChanges)
			{
				continueWithSave = ValidateAndSave();
			}

			if (continueWithSave == ContinueWithSave.Yes)
			{
				Close();
			}
			else
			{
				//load all StripConrtols so that any validation errors propagate to tabs
				foreach (var tab in RulesTabControl.TabPages.OfType<ZTabPage>())
				{
					tab.Controls.OfType<StripControl>().ToList().ForEach(x => x.OnLoad_Exposed());
				}
			}
		}

		#endregion

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			foreach (GridColourStripBusinessObject gridColorStrip in currentColourScheme.ColourStrips)
			{
				gridColorStrip.RunPreSaveValidation();
				if (gridColorStrip.HasErrors)
				{
					result = ContinueWithSave.No;
				}
			}

			return result;
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (currentColourScheme != null)
				{
					foreach (GridColourStripBusinessObject gridColorStrip in currentColourScheme.ColourStrips)
					{
						gridColorStrip.LayoutChanged -= MyScheme_HasChanges;
						gridColorStrip.LayoutChanged -= currentColourScheme.MyScheme_HasChanges;

						foreach (ModuleFilter filter in gridColorStrip.ModuleFilters)
						{
							filter.ModuleFilterChanged -= MyScheme_HasChanges;
						}

						gridColorStrip.ClearAllNotifications();
					}
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Tab Control Drag and Drop

		void TabControlMouseDown(object sender, MouseEventArgs e)
		{
			if (MouseButtons != MouseButtons.Left)
			{
				SelectedTabPage = null;
				return;
			}

			var tc = (TabControl)sender;
			var hoverIndex = GetHoverTabIndex(tc);
			if (hoverIndex >= 0)
			{
				SelectedTabPage = tc.TabPages[hoverIndex];
				tc.DoDragDrop(SelectedTabPage, DragDropEffects.All);
			}
		}

		void TabControlDragOver(object sender, DragEventArgs e)
		{
			var tc = (TabControl)sender;

			if (e.Data.GetData(typeof(ZTabPage)) == null)
			{
				return;
			}

			var hoverTabIndex = GetHoverTabIndex(tc);
			if (hoverTabIndex < 0)
			{
				e.Effect = DragDropEffects.None;
				return;
			}

			e.Effect = DragDropEffects.Move;
		}

		void TabControlQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			if (SelectedTabPage != null)
			{
				var tc = (TabControl)sender;
				if (MouseButtons != MouseButtons.Left)
				{
					var hoverTabIndex = GetHoverTabIndex(tc);
					if (hoverTabIndex >= 0)
					{
						e.Action = DragAction.Cancel;

						var hoverTab = tc.TabPages[hoverTabIndex];
						if (SelectedTabPage != hoverTab)
						{
							SwapTabPages(tc, SelectedTabPage, hoverTab);
							tc.SelectedIndex = tc.TabPages.IndexOf(SelectedTabPage);
						}
						SelectedTabPage = null;
					}
				}
			}
		}

		int GetHoverTabIndex(TabControl tc)
		{
			for (var i = 0; i < tc.TabPages.Count; i++)
			{
				if (tc.GetTabRect(i).Contains(tc.PointToClient(Cursor.Position)))
				{
					return i;
				}
			}
			return -1;
		}

		internal void SwapTabPages(TabControl tc, TabPage src, TabPage dst)
		{
			var indexSrc = tc.TabPages.IndexOf(src);
			var indexDst = tc.TabPages.IndexOf(dst);
			if (!SwapRule(indexSrc, indexDst))
			{
				return;
			}
			tc.TabPages[indexDst] = src;
			tc.TabPages[indexSrc] = dst;
			tc.Refresh();
		}

		#endregion
	}
}
