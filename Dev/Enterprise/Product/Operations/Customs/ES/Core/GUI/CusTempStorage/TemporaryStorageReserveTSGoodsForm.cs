using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class TemporaryStorageReserveTSGoodsForm : ZChildForm
	{
		public TemporaryStorageReserveTSGoodsForm()
		{
			InitializeComponent();
		}

		public TemporaryStorageReserveTSGoodsForm(CusTempStorageReserveTSGoodsRegHeader header) : base(header)
		{
			InitializeComponent();
			SortCollection();
			MakeGridLayout();
			SetGridColumnsReadOnly();
			GetTotalQuantities();
			BottomPanel.AllowOverlap(DataGroupBox);
		}

		void SortCollection() => BusinessEntity.ReserveTSGoodsCollection.Sort(BusinessEntity.IsPackageTypeBulk ? nameof(CusTempStorageReserveTSGoodsRegLine.GrossWeightToUse) : nameof(CusTempStorageReserveTSGoodsRegLine.PackagesToUse), System.ComponentModel.ListSortDirection.Descending);

		void MakeGridLayout()
		{
			LinesGrid.SaveUserLayoutSettings();
			using (LinesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				LinesGrid.SetAvailability(available: true, columnNames);
				LinesGrid.SetColumnVisible(isVisible: true, visibleColumnNames);
				LinesGrid.SetColumnVisible(isVisible: false, columnNames.Where(y => !visibleColumnNames.Contains(y)).ToArray());
				LinesGrid.ReOrderColumns(columnNames);
			}
		}

		void SetGridColumnsReadOnly()
		{
			LinesGrid.GetColumnStyle(CusTempStorageReserveTSGoodsRegLine.Schema.PackagesToUse).IsReadOnly = BusinessEntity.IsPackageTypeBulk;
			LinesGrid.GetColumnStyle(CusTempStorageReserveTSGoodsRegLine.Schema.GrossWeightToUse).IsReadOnly = !BusinessEntity.IsPackageTypeBulk;
		}

		void GetTotalQuantities()
		{
			TotalPackagesLabel.Text = Res.GetString("9B2BD7AF-9088-41C3-8144-2F21A8BC904C", $"Total Packages: {BusinessEntity.TotalPackages}");
			TotalGrossWeightLabel.Text = Res.GetString("FF638CED-A56A-4BAD-92AF-F8B37F0406E4", $"Total Gross Weight: {BusinessEntity.TotalGrossWeight}");
		}

		void OnSaveButtonClick(object sender, EventArgs e)
		{
			if (BusinessEntity.ReserveTSGoodsCollection.Any(l => l.HasErrors))
			{
				Globals.Message.Show(Res.GetString("388DA0B1-A691-49CE-AA44-8803A756B48B", "Please rectify package quantities and/or gross weights."));
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void OnCancelButtonClick(object sender, EventArgs e)
		{
			BusinessEntity.RemoveAllReserveTSGoodsCollection();
			DialogResult = DialogResult.Cancel;
			Close();
		}

		readonly string[] columnNames =
		[
			CusTempStorageReserveTSGoodsRegLine.Schema.TSDNumber,
			CusTempStorageReserveTSGoodsRegLine.Schema.TSDItemNumber,
			CusTempStorageReserveTSGoodsRegLine.Schema.PackageType,
			CusTempStorageReserveTSGoodsRegLine.Schema.Location,
			CusTempStorageReserveTSGoodsRegLine.Schema.Reference,
			CusTempStorageReserveTSGoodsRegLine.Schema.RemainingPackageQty,
			CusTempStorageReserveTSGoodsRegLine.Schema.RemainingGrossWeight,
			CusTempStorageReserveTSGoodsRegLine.Schema.PackagesToUse,
			CusTempStorageReserveTSGoodsRegLine.Schema.GrossWeightToUse
		];

		readonly string[] visibleColumnNames =
		[
			CusTempStorageReserveTSGoodsRegLine.Schema.TSDNumber,
			CusTempStorageReserveTSGoodsRegLine.Schema.TSDItemNumber,
			CusTempStorageReserveTSGoodsRegLine.Schema.PackageType,
			CusTempStorageReserveTSGoodsRegLine.Schema.Location,
			CusTempStorageReserveTSGoodsRegLine.Schema.Reference,
			CusTempStorageReserveTSGoodsRegLine.Schema.RemainingPackageQty,
			CusTempStorageReserveTSGoodsRegLine.Schema.RemainingGrossWeight,
			CusTempStorageReserveTSGoodsRegLine.Schema.PackagesToUse,
			CusTempStorageReserveTSGoodsRegLine.Schema.GrossWeightToUse
		];

		public new CusTempStorageReserveTSGoodsRegHeader BusinessEntity => (CusTempStorageReserveTSGoodsRegHeader)base.BusinessEntity;
		public override string FormVerb => string.Empty;
		public override string FormCaption => Res.GetString("42995E34-4735-4FA4-B4EF-C4B7F017CFD4", "Temporary Storage Reserve Goods");
	}
}
