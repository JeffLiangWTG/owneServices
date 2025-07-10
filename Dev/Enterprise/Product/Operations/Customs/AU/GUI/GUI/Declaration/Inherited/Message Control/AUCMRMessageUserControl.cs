using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUCMRMessageUserControl : ImportMessageUserControl
	{
		public AUCMRMessageUserControl()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();

#if DEBUG
			ZArchitecture.GUI.Testing.MissingResourceStringChecker.ExcludeFromTest(SubjectToRedLineLabel);
#endif
		}

		new public JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (JobDeclaration != null)
			{
				JobDeclaration.JE_MessageTypeInfo.ValueChanged -= OnMessageTypeChanged;
				JobDeclaration.JE_MessageTypeInfo.ValueChanged += OnMessageTypeChanged;
			}
			UpdateEntryHeaderInfoControls();
		}

		void OnMessageTypeChanged(object sender, EventArgs e)
		{
			UpdateEntryHeaderInfoControls();
		}

		void UpdateEntryHeaderInfoControls()
		{
			if (JobDeclaration?.IsEXPDeclaration ?? false)
			{
				MessageStatusLabel.Text = "Entry Status :";
				EntryAdviceLabel.Visible = false;
				EntryAdviceTextBox.Visible = false;
				ATDCodeLabel.Visible = false;
				ATDCodeTextBox.Visible = false;
				CustomsPaymentLabel.Visible = false;
				CustomsPaymentTextBox.Visible = false;
			}
		}

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("42D79A70-5A3F-4280-8726-F38EDCB6BA78", "Warehouse No. of Packs"),
				ColumnName = CusEntryHeader.Schema.WarehouseNumberOfPacks,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("27AF79A1-8094-4C3F-BA89-D574AA083306", "Total Quarantine Service Amount"),
				ColumnName = CusEntryHeader.Schema.AQISServicePaymentAmount,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(233)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.GUI.ZDropEditColumnStyleInfo
			{
				BindToList = "AddInfo.Lookups.ZA_RRC_List",
				CaptionResourceString = Res.GetData("91CCB73F-E62B-4B83-996C-198188B1C958", "Refund Reason Code"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = CusEntryHeader.Schema.RefundReasonCode,
				GroupName = Res.GetData("F3D765EA-8D5E-47EC-8F20-0CB0831BD71B", "Refund"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(154)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("00F06058-010F-4808-9527-2802248C7EDB", "Total Payable"),
				ColumnName = Customs.Business.AutoCusEntryHeader.Schema.CH_TotalPaid,
				GroupName = Res.GetData("F3D765EA-8D5E-47EC-8F20-0CB0831BD71B", "Refund"),
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("74561D8F-58A6-4213-B610-68AED41E4A8E", "Wood Levy"),
				ColumnName = CusEntryHeader.Schema.WoodLevyIncludingWHEstimate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("13C0713D-11CF-4CBB-AF2D-63EF1E25C84F", "T&I Amount"),
				ColumnName = CusEntryHeader.Schema.TAndI,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("9F171684-9990-48F4-A3D3-8E1DB4138AB5", "Duty Amount"),
				ColumnName = CusEntryHeader.Schema.DutyAmountIncludingWHEstimate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("B6277D94-4645-447F-B04A-97247A77420C", "GST Amount"),
				ColumnName = CusEntryHeader.Schema.GSTAmountIncludingWHEstimate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("4FAE228F-F463-48CE-9539-3C79D04EA810", "LCT Amount"),
				ColumnName = CusEntryHeader.Schema.LCTAmount,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("DF35D289-86E7-48CB-826E-762E5859F816", "WET Amount"),
				ColumnName = CusEntryHeader.Schema.WETAmountIncludingWHEstimate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("11F4D23F-E3D4-4C4B-92A4-F03B4DEA272F", "Customs Factor"),
				ColumnName = CusEntryHeader.Schema.CustomsFactor,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("CCCEED3B-D931-4656-8AD8-4FEE242A8E00", "Import Entry Advice"),
				ColumnName = CusEntryHeader.Schema.ImportEntryAdvice,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(149)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("652E9CC8-B60E-4CF4-BC85-68ADDB9963AF", "Quarantine Container Charges"),
				ColumnName = CusEntryHeader.Schema.AQISContainerCharges,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(215)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("AF158A00-BB36-4EEA-8CBA-17B13833C54A", "Quarantine Processing Charge"),
				ColumnName = CusEntryHeader.Schema.AQISProcessingCharge,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(215)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("885C6F96-6740-4992-BD9B-D5C9F2131E2B", "Declaration Processing Charge"),
				ColumnName = CusEntryHeader.Schema.DeclarationProcessingCharge,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(216)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("AF880668-0819-4EDD-B9DA-E1093FCB14B8", "Total Payable Admin"),
				ColumnName = CusEntryHeader.Schema.TotalPayableAdmin,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("92707410-E211-493A-B7C1-AD9466F9CC55", "Other Entry Charges"),
				ColumnName = CusEntryHeader.Schema.OtherEntryCharge,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("DC5E30D2-743D-4A9A-BA0F-FF43039F8997", "Security Amount"),
				ColumnName = CusEntryHeader.Schema.TotalSecurityConcession,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("91863D6B-43F6-4077-B57C-EA307068B886", "Security Uncollected"),
				ColumnName = CusEntryHeader.Schema.TotalSecurityLiability,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155)
			});

			var entryNumber = EntriesBoundGrid.GetColumnStyle(Customs.Business.CusEntryHeader.Schema.EntryNumber);
			entryNumber.IsReadOnly = false;
			entryNumber.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			var isExport = JobDeclaration.IsEXPDeclaration;
			foreach (ZGridColumnInfo columnStyle in EntriesBoundGrid.ColumnStyles)
			{
				var columnName = columnStyle.ColumnName;
				var visible = !isExport || entriesBoundGridColumnsVisibleForExport.Contains(columnName);
				EntriesBoundGrid.SetAvailability(visible, columnName);
				EntriesBoundGrid.SetColumnVisible(visible, columnName);
			}

			EntriesBoundGrid.SetAvailability(JobDeclaration.IsExWarehouse, CusEntryHeader.Schema.WarehouseNumberOfPacks);

			EntryLineGrid.SetAvailability(JobDeclaration.IsImport, entryLineGridColumnsVisibleForImport);
			EntryLineGrid.SetColumnVisible(JobDeclaration.IsImport, entryLineGridColumnsVisibleForImport);
			EntryLineGrid.SetAvailability(JobDeclaration.IsImport && ConsolidatedDeclaration.IsConsolidated(JobDeclaration), nameof(CusEntryLine.ZA_AggregateEntryLineNumber));
			EntryLineGrid.SetColumnVisible(JobDeclaration.IsImport && ConsolidatedDeclaration.IsConsolidated(JobDeclaration), nameof(CusEntryLine.ZA_AggregateEntryLineNumber));
		}

		readonly string[] entryLineGridColumnsVisibleForImport =
		{
			nameof(CusEntryLine.CL_DutyPercent),
			nameof(CusEntryLine.CurrentTotalDutyTax),
			nameof(CusEntryLine.FlatRateDescription),
			nameof(CusEntryLine.IsNonAQISAEPLine),
			nameof(CusEntryLine.RefundReasonCode),
			nameof(CusEntryLine.SecurityConcessionAmount),
			nameof(CusEntryLine.SecurityLiabilityAmount),
			nameof(CusEntryLine.TotalDutyTaxAdvisedInLastClearanceMessage),
		};

		readonly string[] entriesBoundGridColumnsVisibleForExport =
		{
			nameof(CusEntryHeader.CH_BGMReference),
			nameof(CusEntryHeader.CH_MessageType),
			nameof(CusEntryHeader.CH_MessageTypeDescription),
			nameof(CusEntryHeader.EntryNumber),
		};
	}
}
