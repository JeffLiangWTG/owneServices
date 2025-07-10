using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI
{
	public partial class FSimplifiedDeclarationFilterStripControl : ZFilterStripControl
	{
		public FSimplifiedDeclarationFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			SetLinesGridDataBinding();

			ToolStripFindDropButton.Click += OnToolStripFindDropButtonOnClick;

			HeaderAndLinesGridSplitContainer.AllowOverlap(ToolStrip);
			HeaderAndLinesGridSplitContainer.AllowOverlap(ToolStripHelp);

			grid.RowsDeleting += SimplifiedDeclarationsGrid_RowDeleting;
			grid.Deleted += SimplifiedDeclarationsGrid_Deleted;
			grid.MouseDown += SimplifiedDeclarationsGrid_MouseDown;

			LinesGrid.RowsDeleting += LinesGrid_RowDeleting;
			LinesGrid.RowsDeleted += LinesGrid_RowsDeleted;

			FilteredGrid.ForceShowExportToExcelMenuItem = true;
		}

		public FSimplifiedDeclarationFilterStripControl(CusReconDeclaration declaration)
			: this(declaration.CusReconEntries, new FSimplifiedDeclarationFilterBusinessObject(declaration))
		{
			this.declaration = declaration;
		}

		readonly CusReconDeclaration declaration;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			FirePerformSearch();
		}

		protected override void OnSearchPerformed(bool showError, bool didSearch, bool isManualSearch, Form form)
		{
			base.OnSearchPerformed(showError, didSearch, isManualSearch, form);

			if (declaration != null && FilterBusinessObject != null)
			{
				var filter = FilterBusinessObject as FSimplifiedDeclarationFilterBusinessObject;
				declaration.CusReconEntries.AdditionalFilter = filter?.Filter;
			}

			ClearCurrentLinesQuery();
			OnListManagerOnCurrentChanged(grid.ListManager, EventArgs.Empty);
		}

		protected override void HandleGridSizing()
		{
			if (HeaderAndLinesGridSplitContainer != null && (HeaderAndLinesGridSplitContainer.Left != 0 || HeaderAndLinesGridSplitContainer.Right != ClientRectangle.Right || HeaderAndLinesGridSplitContainer.Bottom != ClientRectangle.Bottom))
			{
				HeaderAndLinesGridSplitContainer.Location = ControlDpiScalingHelper.NewScaledPoint(0, HeaderAndLinesGridSplitContainer.Top, false);
				ControlDpiScalingHelper.SetHeight(HeaderAndLinesGridSplitContainer, ClientSize.Height - HeaderAndLinesGridSplitContainer.Top, false);
				ControlDpiScalingHelper.SetWidth(HeaderAndLinesGridSplitContainer, ClientSize.Width, false);
			}
		}

		protected override void UpdateFilteredGridRefreshWarning()
		{
			base.UpdateFilteredGridRefreshWarning();

			UpdateSplitContainerOnFilterStripsChanged();
		}

		void ClearCurrentLinesQuery()
		{
			var filter = FilterBusinessObject as FSimplifiedDeclarationFilterBusinessObject;
			var currentItem = grid.ListManager.GetCurrent() as CusReconEntry;
			if (currentItem != null && filter != null && filter.LineOnlyQuery.IsEmpty)
			{
				currentItem.CusReconEntryLines.AdditionalFilter = new ZDBOnlyQuery(typeof(CusReconEntryLine));
			}
		}

		void OnListManagerOnCurrentChanged(object sender, EventArgs args)
		{
			var filter = FilterBusinessObject as FSimplifiedDeclarationFilterBusinessObject;

			var listManager = sender as CurrencyManager;
			var currentItem = listManager.GetCurrent() as CusReconEntry;

			if (currentItem != null && filter != null)
			{
				if (!filter.LineOnlyQuery.IsEmpty)
				{
					currentItem.CusReconEntryLines.AdditionalFilter = filter.LineOnlyQuery;
				}

				LinesGrid.SetDataBinding(GridCollection, nameof(CusReconEntry.CusReconEntryLines));
			}
		}

		void OnToolStripFindDropButtonOnClick(object sender, EventArgs args)
		{
			var filterBizO = (FSimplifiedDeclarationFilterBusinessObject)FilterBusinessObject;
			filterBizO.ResetLineOnlyQuery();
		}

		void SetLinesGridDataBinding()
		{
			LinesGrid.SetDataBinding(GridCollection, nameof(CusReconEntry.CusReconEntryLines));
		}

		void UpdateSplitContainerOnFilterStripsChanged()
		{
			if (HeaderAndLinesGridSplitContainer != null)
			{
				ControlDpiScalingHelper.SetTop(HeaderAndLinesGridSplitContainer, AddStripButton.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(4), false);

				if (AutoRefreshWarningLabel.Visible)
				{
					if (IsFilterVisible)
					{
						ControlDpiScalingHelper.SetTop(HeaderAndLinesGridSplitContainer, ControlForLayout.Top + AutoRefreshWarningLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
					}
					else
					{
						ControlDpiScalingHelper.SetTop(HeaderAndLinesGridSplitContainer, 0, false);
						ControlDpiScalingHelper.SetTop(HeaderAndLinesGridSplitContainer, AutoRefreshWarningLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
					}
				}
			}
		}

		void SimplifiedDeclarationsGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			if (Globals.Message.ShowConfirmation(
					Res.GetString("D513CBB0-FC7E-4BF2-86DD-D3F160A192BF", "This action will remove the Simplified Declaration(s) from the Monthly Closing Declaration.\r\nThe Simplified Declaration(s) must be linked into a new Monthly Closing Declaration."),
					Res.GetString("A3D9BF99-3884-41B4-9A4F-4F39FAACAA85", "Remove Simplified Declarations"),
					Res.GetString("D82840DF-0C9D-4E09-9FFE-F7EF545B1BEF", "Yes"),
					MessageBoxIcon.Warning
					) != DialogResult.OK)
			{
				e.Cancel = true;
			}
		}

		void SimplifiedDeclarationsGrid_Deleted(object sender, GridRowOnDeletedEventArgs e)
		{
			foreach (var line in ((CusReconEntry)e.DeletedBusinessObjects[0]).CusReconEntryLines)
			{
				line.CRL_LineNumber = ZShort.Zero;
			}
		}

		void SimplifiedDeclarationsGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks > 1)
			{
				var hitInfoRow = grid.HitTest(e.X, e.Y).Row;
				var listManagerList = grid.ListManager.List;
				if (hitInfoRow >= 0 && hitInfoRow < listManagerList.Count)
				{
					var reconEntry = (CusReconEntry)listManagerList[hitInfoRow];
					using (var module = ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration) as ZFilterModule)
					{
						var controller = module.GetNewController();
						controller.ShowEditForm(reconEntry.EntryHeader.Declaration);
					}
				}
			}
		}

		void LinesGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			if (Globals.Message.ShowConfirmation(
					Res.GetString("7B719C10-56E3-4FF0-82CF-686EA08DA9B1", "This action will remove the Simplified Declaration Line(s) from the Monthly Closing Declaration.\r\nThe Simplified Declaration Line(s) must be linked into a new Monthly Closing Declaration."),
					Res.GetString("587A4224-752B-4981-A63B-F6594E099902", "Remove Simplified Declaration Line(s)"),
					Res.GetString("FC87FDB4-69A7-49E4-ABEB-0456E65BCE50", "Yes"),
					MessageBoxIcon.Warning
					) != DialogResult.OK)
			{
				e.Cancel = true;
			}
		}

		void LinesGrid_RowsDeleted(object sender, RowsDeletingEventArgs e)
		{
			if (!e.Cancel)
			{
				var originalCusReconEntry = (CusReconEntry)grid.GetCurrent();
				var unlinkedCusReconEntry = originalCusReconEntry.GetUnlinkedCusReconEntryWithTheSameOriginalEntryNumber() ?? originalCusReconEntry.DuplicateCusReconEntryAndUnlinkFromDeclaration();

				var removedCusReconEntryLines = e.Objects.Cast<CusReconEntryLine>();
				removedCusReconEntryLines.ForEach(x => x.CRL_LineNumber = ZShort.Zero);
				unlinkedCusReconEntry.CusReconEntryLines.AddRange(removedCusReconEntryLines);

				if (!originalCusReconEntry.CusReconEntryLines.Any())
				{
					originalCusReconEntry.Delete();
				}
			}
		}
	}
}
