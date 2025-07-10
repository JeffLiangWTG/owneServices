using System;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class ExportEntryInstructionUserControl : BaseCustomsEntryUserControl
	{
		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		const string CustomsEntryInstructions = "CustomsEntryInstructions";

		public ExportEntryInstructionUserControl()
		{
			InitializeComponent();
			ResetColumnsInEntryInstructionsGrid();
			SetECRGrid();
			ApprovalCertificateInfoGrid.MaximumRows = ApprovalCertificateInfoCollection.MaxCountForExport;
			InventoryNumbersGroupBox.Visible = false;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			DetailsLayoutPanel.UpdateLayout(new ExportEntryInstructionLayouts());
			BindingSource.SetBindingMember(DetailsLayoutPanel, CustomsEntryInstructions);

			ECRDynamicLayoutPanel.UpdateLayout(new ECREntryInstructionLayouts());
			BindingSource.SetBindingMember(ECRDynamicLayoutPanel, CustomsEntryInstructions);

			RCRDetailsLayoutPanel.UpdateLayout(new RCREntryInstructionLayouts());
			BindingSource.SetBindingMember(RCRDetailsLayoutPanel, CustomsEntryInstructions);

			CDB01DynamicLayoutPanel.UpdateLayout(new CDB01EntryInstructionLayouts());
			BindingSource.SetBindingMember(CDB01DynamicLayoutPanel, CustomsEntryInstructions);
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			var isExport = JobDeclaration.IsExport;

			ApprovalCertificateCategoryDropEdit.Visible = isExport;
			MarksAndNumbersTextBox.Visible = !JobDeclaration.IsAir;
			ECRTabPage.TabVisible = JobDeclaration.IsSea;
			CDB01TabPage.TabVisible = JobDeclaration.IsAir;
			EntryInstructionsGrid.SetAvailability(JobDeclaration.IsExportAndAir, nameof(CusEntryInstruction.CEI_BillNumber));
			EntryInstructionsGrid.SetAvailability(JobDeclaration.IsExportAndSea, nameof(CusEntryInstruction.ExportControlNumber));
			EntryInstructionsGrid.SetColumnVisible(JobDeclaration.IsExportAndSea, nameof(CusEntryInstruction.CEI_GoodsDescription));
		}

		void ResetColumnsInEntryInstructionsGrid()
		{
			var exportControlNumberColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			exportControlNumberColumnStyleInfo.ColumnName = nameof(CusEntryInstruction.ExportControlNumber);
			exportControlNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			EntryInstructionsGrid.ColumnStyles.Insert(1, exportControlNumberColumnStyleInfo);

			var awbOrbillNumberColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			awbOrbillNumberColumnStyleInfo.ColumnName = nameof(CusEntryInstruction.CEI_BillNumber);
			awbOrbillNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			EntryInstructionsGrid.ColumnStyles.Insert(1, awbOrbillNumberColumnStyleInfo);

			var ceiDescriptionColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ceiDescriptionColumnStyleInfo.ColumnName = nameof(CusEntryInstruction.CEI_Description);
			ceiDescriptionColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			ceiDescriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			EntryInstructionsGrid.ColumnStyles.Insert(3, ceiDescriptionColumnStyleInfo);

			var goodsDescriptionColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			goodsDescriptionColumnStyleInfo.ColumnName = nameof(CusEntryInstruction.CEI_GoodsDescription);
			goodsDescriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			EntryInstructionsGrid.ColumnStyles.Insert(4, goodsDescriptionColumnStyleInfo);

			var specialCargoCodeColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			specialCargoCodeColumnStyleInfo.ColumnName = nameof(CusEntryInstruction.CEI_SpecialCargoCode);
			specialCargoCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			EntryInstructionsGrid.ColumnStyles.Insert(5, specialCargoCodeColumnStyleInfo);
		}

		void SetECRGrid()
		{
			ECRGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = nameof(MoveInDestination.CSI_Code),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(MoveInDestination.CSI_DateOfIssue),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short
				},

				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = nameof(MoveInDestination.CSI_ReferenceNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
				},

				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(MoveInDestination.CSI_Quantity),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					Decimals = 0,
					MaxValue = 99999999,
				},

				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(MoveInDestination.CSI_Quantity2),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					Decimals = 3,
				},

				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(MoveInDestination.CSI_Quantity3),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					Decimals = 3,
				},

				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = nameof(MoveInDestination.CSI_Description),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
				},
			});

			ECRGrid.CurrentCellChanged += ECRGrid_CurrentCellChanged;
		}

		void ECRGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			InventoryNumbersGroupBox.Visible = ECRGrid.CurrentRowIndex >= 0;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}

			if (ECRGrid != null)
			{
				ECRGrid.CurrentCellChanged -= ECRGrid_CurrentCellChanged;
			}

			base.Dispose(disposing);
		}
	}
}
