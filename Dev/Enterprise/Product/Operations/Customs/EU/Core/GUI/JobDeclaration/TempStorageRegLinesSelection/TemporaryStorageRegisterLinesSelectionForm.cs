using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.GUI
{
	public partial class TemporaryStorageRegisterLinesSelectionForm : ZChildForm
	{
		public TemporaryStorageRegisterLinesSelectionForm()
		{
			InitializeComponent();
		}

		public TemporaryStorageRegisterLinesSelectionForm(CusTempStorageRegLinesSelectionHeader header) : base(header)
		{
			InitializeComponent();
			module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.EU.TempStorageRegisterLines);
			CreateFilterStripControl();
			SetupMainPanel();
			MakeGridsLayout();
		}

		readonly ZFilterModule module;
		ZFilterStripControl filterStripControl;

		public new CusTempStorageRegLinesSelectionHeader BusinessEntity => (CusTempStorageRegLinesSelectionHeader)base.BusinessEntity;

		void CreateFilterStripControl()
		{
			var filterBusinessObject = module.FilterBusinessObject;
			((IFilterStripBusinessObjectInternals)filterBusinessObject).LayoutContext = Env.CurrentCompany.Country.Code + "TemporaryStorageRegisterSelectionForm";
			filterStripControl = (ZFilterStripControl)module.EmbeddedControl;
			filterStripControl.PerformSearch += EmbeddedControl_PerformSearch;
		}

		void SetupMainPanel()
		{
			((System.ComponentModel.ISupportInitialize)(filterStripControl.BindingSource)).BeginInit();
			filterStripControl.SuspendLayout();
			filterStripControl.FilteredGrid.Visible = false;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			Controls.Remove(MainPanel);
			filterStripControl.Dock = DockStyle.Fill;
			MainPanel.Anchor = filterStripControl.FilteredGrid.Anchor;
			MainPanel.Dock = filterStripControl.FilteredGrid.Dock;
			MainPanel.Location = filterStripControl.FilteredGrid.Location;
			MainPanel.Size = filterStripControl.FilteredGrid.Size;
			MainPanel.AllowOverlap(filterStripControl.Controls.Find("ToolStrip", true).Single());
			MainPanel.AllowOverlap(filterStripControl.Controls.Find("ToolStripHelp", true).Single());
			MainPanel.AllowOverlap(filterStripControl.Controls.Find("CoveringLabel", true).Single());
			filterStripControl.Controls.Add(MainPanel);
			filterStripControl.Controls.SetChildIndex(MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(filterStripControl.BindingSource)).EndInit();
			filterStripControl.ResumeLayout(false);
			filterStripControl.PerformLayout();
			filterStripControl.FilteredGrid.Layout += (object sender, LayoutEventArgs e) =>
			{
				if (MainPanel.Location != filterStripControl.FilteredGrid.Location)
				{
					MainPanel.Location = filterStripControl.FilteredGrid.Location;
				}
				if (MainPanel.Height != filterStripControl.FilteredGrid.Height)
				{
					ControlDpiScalingHelper.SetHeight(ref MainPanel, filterStripControl.FilteredGrid.Height, false);
				}
				if (MainPanel.Width != filterStripControl.FilteredGrid.Width)
				{
					ControlDpiScalingHelper.SetWidth(ref MainPanel, filterStripControl.FilteredGrid.Width, false);
				}
			};
			filterStripControl.TabIndex = 0;
			Controls.Add(filterStripControl);
			Controls.SetChildIndex(this.filterStripControl, 0);

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			ResumeLayout(false);
			PerformLayout();
		}

		void EmbeddedControl_PerformSearch(object sender, PerformSearchEventArgs e)
		{
			BusinessEntity.Load(filterStripControl.GridCollection.OfType<ICusTempStorageRegLine>());
		}

		void MakeGridsLayout()
		{
			LinesGrid.SaveUserLayoutSettings();
			using (LinesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				LinesGrid.SetAvailability(true, columnNames);
				LinesGrid.SetColumnVisible(true, visibleColumnNames);
				LinesGrid.SetColumnVisible(false, columnNames.Where(y => !visibleColumnNames.Contains(y)).ToArray());
				LinesGrid.ReOrderColumns(columnNames);
			}

			SelectedLinesGrid.SaveUserLayoutSettings();
			using (SelectedLinesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				SelectedLinesGrid.SetAvailability(true, columnNames);
				SelectedLinesGrid.SetColumnVisible(true, visibleColumnNames);
				SelectedLinesGrid.SetColumnVisible(false, columnNames.Where(y => !visibleColumnNames.Contains(y)).ToArray());
				SelectedLinesGrid.ReOrderColumns(columnNames);
			}
		}

		void SelectButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (filterStripControl != null)
				{
					filterStripControl.PerformSearch -= EmbeddedControl_PerformSearch;
				}

				if (module != null)
				{
					module.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void ClearDrawQtyButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.ClearDrawQuantities();
		}

		void FillAllDrawQtyButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.FillOutDrawQuantities();
		}

		readonly string[] columnNames =
		[
			CusTempStorageSelectableRegLine.Schema.PreviousReferenceType,
			CusTempStorageSelectableRegLine.Schema.PreviousReferenceNumber,
			CusTempStorageSelectableRegLine.Schema.TSDNumber,
			CusTempStorageSelectableRegLine.Schema.ArrivalDate,
			CusTempStorageSelectableRegLine.Schema.Owner,
			CusTempStorageSelectableRegLine.Schema.OriginalPackagesQty,
			CusTempStorageSelectableRegLine.Schema.PackagesQtyOnHand,
			CusTempStorageSelectableRegLine.Schema.PackageType,
			CusTempStorageSelectableRegLine.Schema.PackageMarks,
			CusTempStorageSelectableRegLine.Schema.PackagesToDraw,
			CusTempStorageSelectableRegLine.Schema.GrossWeightOnHand,
			CusTempStorageSelectableRegLine.Schema.GrossWeightToDraw,
			CusTempStorageSelectableRegLine.Schema.TSDItemNumber,
			CusTempStorageSelectableRegLine.Schema.CommodityCode,
			CusTempStorageSelectableRegLine.Schema.GoodsDescription,
		];

		readonly string[] visibleColumnNames =
		[
			CusTempStorageSelectableRegLine.Schema.TSDNumber,
			CusTempStorageSelectableRegLine.Schema.ArrivalDate,
			CusTempStorageSelectableRegLine.Schema.OriginalPackagesQty,
			CusTempStorageSelectableRegLine.Schema.PackagesQtyOnHand,
			CusTempStorageSelectableRegLine.Schema.PackageType,
			CusTempStorageSelectableRegLine.Schema.PackageMarks,
			CusTempStorageSelectableRegLine.Schema.PackagesToDraw,
			CusTempStorageSelectableRegLine.Schema.GrossWeightOnHand,
			CusTempStorageSelectableRegLine.Schema.GrossWeightToDraw,
			CusTempStorageSelectableRegLine.Schema.TSDItemNumber,
			CusTempStorageSelectableRegLine.Schema.CommodityCode,
			CusTempStorageSelectableRegLine.Schema.GoodsDescription,
		];
	}
}
