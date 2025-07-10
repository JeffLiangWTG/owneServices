using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	[SuppressControlRequiresTextBasher]
	public partial class TriageAssistForm : ZChildForm
	{
		public TriageAssistForm(TriageAssistBusinessObject triageAssistBusinessObject, SupportIncidentForm parentIncidentForm = null) : base(triageAssistBusinessObject)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			ParentIncidentForm = parentIncidentForm;
			BusinessEntity.SearchedCriteriaCollection.CountChanged += (_, __) =>
			{
				Color? panelColor = null;
				if (!string.IsNullOrWhiteSpace(SymptomInputTextBox.Text))
				{
					panelColor = Color.LightSkyBlue;
				}
				SetCollapsiblePanel(DiagnosticCriteriaSearchResultsCollapsiblePanel, BusinessEntity.SearchedCriteriaCollection, panelColor);
			};
			BusinessEntity.LinkedCriteriaCollection.CountChanged += (_, __) =>
			{
				SetCollapsiblePanel(DiagnosticCriteriaLinkedCollapsiblePanel, BusinessEntity.LinkedCriteriaCollection);
			};
			BusinessEntity.FilteredSuggestedCriteriaCollection.CountChanged += (_, __) =>
			{
				SetCollapsiblePanel(DiagnosticCriteriaSuggestedCollapsiblePanel, BusinessEntity.FilteredSuggestedCriteriaCollection);
			};
			BusinessEntity.FilteredTriageAssistTreeWrapperCollection.CountChanged += (_, __) =>
			{
				((TriageAssistTreeModelView)TriageNodeTreeView.Model).Rebuild();
				TriageNodeTreeView.ExpandAll();
				CollapseTreeViewNode(TriageNodeTreeView);

				var totalRowCount = BusinessEntity.FilteredTriageAssistTreeWrapperCollection.Count;
				var nonExcludedRowCount = BusinessEntity.FilteredTriageAssistTreeWrapperCollection.OfType<TriageAssistTreeTriageWrapper>().Count(x => x.TriageStatus != TriageAssistTreeTriageWrapper.TriageNodeStatus.Excluded);
				SetCollapsiblePanel(TriageNodeCollapsiblePanel, nonExcludedRowCount);
				TriageNodeCollapsiblePanel.IsCollapsed = totalRowCount == 0;
				TriageNodeCollapsiblePanel.Update();
			};
			TriageNodeTreeView.Model = new TriageAssistTreeModelView(BusinessEntity);
			BusinessEntity.DiagnosticGuideInternalSupportNoteRTFInfo.ValueChanged += (_, __) =>
			{
				SetCollapsiblePanel(DiagnosticGuideCollapsiblePanel, BusinessEntity.DiagnosticGuideItemCount);
			};
			BusinessEntity.FinalisedTriageNodeInternalSupportActionRTFInfo.ValueChanged += (_, __) =>
			{
				SetCollapsiblePanel(TriageNodeActionCollapsiblePanel, BusinessEntity.FinalisedTriageNodeActionCount);
			};
			BusinessEntity.OnTriageAssistSaved += (_, __) =>
			{
				CollapseTreeViewNode(TriageNodeTreeView);
			};
			BusinessEntity.OnShowFocusedObjectsOnlyChanged += (_, __) =>
			{
				ShowFocusedObjectsOnlyCheckBox.CheckState = (BusinessEntity.ShowFocusedSuggestedCriteriaOnly && BusinessEntity.ShowFocusedTriageNodesOnly) ? CheckState.Checked
								: (BusinessEntity.ShowFocusedSuggestedCriteriaOnly || BusinessEntity.ShowFocusedTriageNodesOnly) ? CheckState.Indeterminate
								: CheckState.Unchecked;
			};
			InputDebouncingTimer = new ProxyWindowsTimer();
			InputDebouncingTimer.Interval = 300;
			InputDebouncingTimer.Tick += InputDebouncingTimer_Tick;
			InputDebouncingTimer.Enabled = false;
			ShowFocusedObjectsOnlyCheckBox.AllowOverlap(DiagnosticCriteriaProductDropEdit);
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(SymptomInputTextBox);
			MissingResourceStringChecker.ExcludeFromTest(SearchTermOperatorDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(TriageAssistSearchBar.Controls.Find("SearchOptionsGroupBox", true).Single());
			TriageNodeCollapsiblePanel.AllowOutsideOfParent();
			TriageNodeActionCollapsiblePanel.AllowOutsideOfParent();
			DiagnosticCriteriaSearchResultsCollapsiblePanel.AllowOutsideOfParent();
			DiagnosticCriteriaLinkedCollapsiblePanel.AllowOutsideOfParent();
			DiagnosticCriteriaSuggestedCollapsiblePanel.AllowOutsideOfParent();
			DiagnosticGuideCollapsiblePanel.AllowOutsideOfParent();
#endif
		}

		public SupportIncidentForm ParentIncidentForm { get; }

		public IncidentTriage Triage => null;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CollapsePanels(DiagnosticCriteriaTableLayoutPanel);
			CollapsePanels(TriageNodeTableLayoutPanel);
			BusinessEntity.LinkedCriteriaCollection.LoadLinkedCriteria();
			BusinessEntity.TriageAssistTreeWrapperCollection.Load();
			BusinessEntity.SuggestedCriteriaCollection.LoadSuggestedCriteria();
			DiagnosticCriteriaSearchResultsCollapsiblePanel.BackColor = Color.LightGray;
			BusinessEntity.FilteredTriageAssistTreeWrapperCollection.Rebuild();
			BusinessEntity.FilteredSuggestedCriteriaCollection.Rebuild();
			BusinessEntity.RefreshTextProperties();
			SymptomInputTextBox.Focus();
			LoadUserControlLayout();
		}

		protected override void OnClosed(EventArgs e)
		{
			SaveUserControlLayout();
			base.OnClosed(e);
		}

		#region Control Events

		void DiagnosticPanel_SizeChanged(object sender, EventArgs e) => SetTableLayoutPanelAutoSize(DiagnosticCriteriaTableLayoutPanel);

		void TriageNodePanel_SizeChanged(object sender, EventArgs e) => SetTableLayoutPanelAutoSize(TriageNodeTableLayoutPanel);

		void DiagnosticCriteriaProductDropEdit_TextChanged(object sender, EventArgs e)
		{
			RefreshSearchTerm();
			BusinessEntity.TriageAssistTreeWrapperCollection.Load();
			BusinessEntity.SuggestedCriteriaCollection.LoadSuggestedCriteria();
			BusinessEntity.FilteredTriageAssistTreeWrapperCollection.Rebuild();
			BusinessEntity.FilteredSuggestedCriteriaCollection.Rebuild();
		}

		void SymptomInputTextBox_TextChanged(object sender, EventArgs e)
		{
			InputDebouncingTimer.Stop();
			InputDebouncingTimer.Start();
		}

		void InputDebouncingTimer_Tick(object sender, EventArgs e)
		{
			InputDebouncingTimer.Stop();
			RefreshSearchTerm();
		}

		void DiagnosticCriteriaGrid_DoubleClick(object sender, EventArgs e)
		{
			if (sender is ZGrid grid && grid.ListManager.Position > -1)
			{
				var item = (RelevantDiagnosticCriteria)grid.ListManager.GetCurrent();
				var controller = ZControllerFactory.Create(ClientControllerRegistration.IncidentDiagnosticCriteria);
				controller.ShowEditForm(item.DiagnosticCriteria);
			}
		}

		void DiagnosticCriteriaGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			if (e.ObjectAtRow is RelevantDiagnosticCriteria criteria)
			{
				if (criteria.Confirm)
				{
					e.Colour = Color.PaleGreen;
				}
				else if (criteria.Negate)
				{
					e.Colour = Color.LightPink;
				}
				else if (criteria.Investigate)
				{
					e.Colour = Color.White;
				}
			}
		}

		void TreeNodeTextBox_DrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			if (e.Node.Tag is IZNode node)
			{
				var triageWrapper = node.BizObjForBinding as TriageAssistTreeTriageWrapper;
				var criteriaWrapper = node.BizObjForBinding as TriageAssistTreeRelevantCriteriaWrapper;
				var isStatusColumn = sender is Aga.Controls.Tree.NodeControls.NodeControl nodeControl && nodeControl.ParentColumn == StatusTreeViewColumn;
				
				if (triageWrapper != null)
				{
					e.Font = new Font(e.Font, FontStyle.Bold);
					e.BackgroundBrush = new SolidBrush(Color.LightGray);

					if (triageWrapper.TriageStatus == TriageAssistTreeTriageWrapper.TriageNodeStatus.Finalised)
					{
						e.BackgroundBrush = new SolidBrush(Color.LawnGreen);
					}
					else if (isStatusColumn)
					{
						if (triageWrapper.TriageStatus == TriageAssistTreeTriageWrapper.TriageNodeStatus.AllConfirmed)
						{
							e.BackgroundBrush = new SolidBrush(Color.PaleGreen);
						}
						else if (triageWrapper.TriageStatus == TriageAssistTreeTriageWrapper.TriageNodeStatus.Excluded)
						{
							e.BackgroundBrush = new SolidBrush(Color.DarkGray);
						}
					}
				}

				if (criteriaWrapper != null)
				{
					if (isStatusColumn)
					{
						var criteria = criteriaWrapper.RelevantCriteria;
						if (!criteria.Investigate && !criteria.Confirm && !criteria.Negate) //Suggested/Null
						{
							e.BackgroundBrush = new SolidBrush(Color.LightGray);
						}
						else if (criteria.Confirm)
						{
							e.BackgroundBrush = new SolidBrush(Color.PaleGreen);
						}
						else if (criteria.Negate)
						{
							e.BackgroundBrush = new SolidBrush(Color.LightPink);
						}
					}
				}
			}
		}

		void TriageNodeTreeView_NodeMouseDoubleClick(object sender, Aga.Controls.Tree.TreeNodeAdvMouseEventArgs e)
		{
			if (e.Node.Tag is ZNode<TriageAssistTreeBizObjWrapper> node)
			{
				if (node.BizObj.BizObj is IncidentDiagnosticCriteria criteria)
				{
					ZControllerFactory.Create(ClientControllerRegistration.IncidentDiagnosticCriteria).ShowEditForm(criteria);
					e.Handled = true;
				}
				else if (node.BizObj.BizObj is IncidentTriage triage)
				{
					ZControllerFactory.Create(ClientControllerRegistration.IncidentTriage).ShowEditForm(triage);
					e.Handled = true;
				}
			}
		}

		void FinaliseSelectedButton_Click(object sender, EventArgs e)
		{
			var node = TriageNodeTreeView.SelectedNode;
			if (node != null && node.Tag is ZNode<TriageAssistTreeBizObjWrapper> tag)
			{
				if (tag.BizObj.BizObj is IncidentTriage triage)
				{
					BusinessEntity.Parent.TriagePK = triage.PK;
					return;
				}
			}
			Globals.Message.ShowError("Please select a Triage Node to finalise");
		}

		#endregion

		#region Control Refresh

		void RefreshSearchTerm() => BusinessEntity.RefreshSearchTerm(SymptomInputTextBox.Text, DiagnosticCriteriaProductDropEdit.Text);

		#endregion

		#region UI Helpers

		static void SetCollapsiblePanel(ZCollapsiblePanel panel, BusinessObjectCollection collection, Color? backColor = null)
				=> SetCollapsiblePanel(panel, collection.Count, backColor);

		static void SetCollapsiblePanel(ZCollapsiblePanel panel, int rowCount, Color? backColor = null)
		{
			panel.BackColor = backColor ?? (rowCount > 0 ? Color.LightSkyBlue : Color.LightGray);
			panel.IsCollapsed = rowCount == 0;
			panel.Text = $"{panel.Text.Split('\t').First()}\t ({rowCount})";
			panel.Refresh();
		}

		static void SetTableLayoutPanelAutoSize(KTableLayoutPanel tableLayoutPanel)
		{
			tableLayoutPanel.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100F);

			foreach (var item in tableLayoutPanel.Controls.OfType<ZCollapsiblePanel>().Select((value, index) => (value, index)))
			{
				tableLayoutPanel.RowStyles[item.index] = item.value.IsCollapsed ?
					new RowStyle(SizeType.AutoSize) : new RowStyle(SizeType.Percent, 100F);
			}

			tableLayoutPanel.Refresh();
		}

		static void CollapsePanels(KTableLayoutPanel tableLayoutPanel)
		{
			foreach (var item in tableLayoutPanel.Controls.OfType<ZCollapsiblePanel>())
			{
				item.IsCollapsed = true;
			}
		}

		void CollapseTreeViewNode(ZTreeViewAdv treeView)
		{
			foreach (var node in treeView.Root.Children)
			{
				if (node.Tag is ZNode<TriageAssistTreeBizObjWrapper> zNode
						&& zNode.BizObj is TriageAssistTreeTriageWrapper wrapper
						&& (wrapper.TriageStatus == TriageAssistTreeTriageWrapper.TriageNodeStatus.Excluded
							|| (!BusinessEntity.ShowFocusedTriageNodesOnly && !wrapper.IsFocused)))
				{
					node.Collapse();
				}
			}
		}

		void LoadUserControlLayout()
		{
			var controlLayout = EDIDataRegistry.Instance.TriageAssistFormUserControlLayout.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty);

			var searchTermOperator = controlLayout.GetDescriptionFromCode(nameof(this.BusinessEntity.SearchTermOperator));
			if (!string.IsNullOrEmpty(searchTermOperator))
			{
				this.BusinessEntity.SearchTermOperator = searchTermOperator;
			}

			var showSearchOptions = controlLayout.GetDescriptionFromCode(nameof(this.BusinessEntity.ShowSearchOptions));
			if (!string.IsNullOrEmpty(showSearchOptions) && ZBool.TryParse(showSearchOptions, out var result))
			{
				this.BusinessEntity.ShowSearchOptions = result;
			}
			else
			{
				this.BusinessEntity.ShowSearchOptions = true;
			}

			var criteriaTypeFilter = controlLayout.GetDescriptionFromCode(nameof(this.BusinessEntity.CriteriaTypeFilter));
			if (!string.IsNullOrEmpty(criteriaTypeFilter))
			{
				this.BusinessEntity.CriteriaTypeFilter = criteriaTypeFilter;
			}
		}

		void SaveUserControlLayout()
		{
			var layout = new CodeDescriptionPairList(EDIDataRegistry.Instance.TriageAssistFormUserControlLayout.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty));
			layout.AddOverwriteIfExists(new CodeDescriptionPair(nameof(this.BusinessEntity.SearchTermOperator), this.BusinessEntity.SearchTermOperator));
			layout.AddOverwriteIfExists(new CodeDescriptionPair(nameof(this.BusinessEntity.ShowSearchOptions), this.BusinessEntity.ShowSearchOptions.ToString()));
			layout.AddOverwriteIfExists(new CodeDescriptionPair(nameof(this.BusinessEntity.CriteriaTypeFilter), this.BusinessEntity.CriteriaTypeFilter));
			EDIDataRegistry.Instance.TriageAssistFormUserControlLayout.SetValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty, layout);
		}

		#endregion

		new TriageAssistBusinessObject BusinessEntity => (TriageAssistBusinessObject)base.BusinessEntity;

		protected override bool AllowNew => false;

		void InternalSupportNotePopupButton_Click(object sender, EventArgs e)
		{
			InternalSupportNoteTextBox.ShowPopupEditorNonModal();
		}

		void ClientQuestionPopupButton_Click(object sender, EventArgs e)
		{
			ClientQuestionTextBox.ShowPopupEditorNonModal();
		}

		readonly IWindowsTimer InputDebouncingTimer; //reduce db hit.
	}
}
