using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportInvoiceLineUserControl : EUExportInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();

			ReorderTabPages();
			if (!DesignMode)
			{
				var dgSubstanceUserControl = this.FindSingle<DgSubstanceUserControl>();
				var undgManager = new UNDGDataItemFormManager(CustomsInvoiceLinesBoundGrid, "", UNDGDataItemFormManagerConfig.IMOShowProperties());
				undgManager.Initialize();

				dgSubstanceUserControl.MoreButtonClick += (o, e) =>
				{
					undgManager.ShowMultipleItemForm();
				};
			}
		}

		public new JobComInvoiceLine CurrentInvoiceLine => (JobComInvoiceLine)base.CurrentInvoiceLine;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var jobDeclaration = CurrentDataItem as JobDeclaration;
			var configuration = jobDeclaration?.Configuration.InvoiceLineConfiguration;
			if (configuration != null)
			{
				PreviousProceduresTabPage.TabVisible = configuration.PreviousDocumentsSupport(jobDeclaration);
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			var customsQuantityGroupName = Res.GetData("300f031b-5301-423c-93a3-8c62eedbe9bf", "[38] Net Mass Measure");

			CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_CustomsQuantity, 150);
			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity).GroupName = customsQuantityGroupName;
			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsUnitQty).GroupName = customsQuantityGroupName;

			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(CreateOriginFederalStateColumn());
			}
		}

		static ZDropEditColumnStyleInfo CreateOriginFederalStateColumn()
		{
			return new ZDropEditColumnStyleInfo()
			{
				ColumnName = JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121)
			};
		}

		protected override void HookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.HookInvoiceLineEvents(invoiceLine);

			var jobComInvoiceLine = (JobComInvoiceLine)invoiceLine;
			if (jobComInvoiceLine != null)
			{
				jobComInvoiceLine.PreviousProcedureMaster.CSI_Procedure_ValueChanged += MasterCSI_Procedure_ValueChanged;
			}
			MasterCSI_Procedure_ValueChanged(null, null);
		}

		protected override void UnHookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.UnHookInvoiceLineEvents(invoiceLine);

			var jobComInvoiceLine = (JobComInvoiceLine)invoiceLine;
			if (jobComInvoiceLine != null)
			{
				jobComInvoiceLine.PreviousProcedureMaster.CSI_Procedure_ValueChanged -= MasterCSI_Procedure_ValueChanged;
			}
		}

		protected override ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => Res.GetData("F2745DB2-D6A3-46CB-82EB-9EFBB1AF4C42", "[44] Additional Documents");

		void MasterCSI_Procedure_ValueChanged(object sender, EventArgs e)
		{
			var invoiceLine = CurrentInvoiceLine;
			previousProceduresUserControl.SetPreviousDocumentsGridColumnsVisible(invoiceLine?.PreviousProcedureMaster.CSI_Procedure ?? ZString.Empty);
		}

		protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControlWithGrid);

		protected override Type GetSupportingDocumentsUserControlType() => typeof(ExportInvoiceLineSupportingDocumentsUserControl);

		protected override Type GetPreviousDocumentsUserControlType() => typeof(ExportInvoiceLinePreviousDocumentsUserControl);

		protected override void SetGridLineColumnCharacterCasing()
		{
			var descColumn = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description);
			if (descColumn != null)
			{
				descColumn.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			}
		}

		protected override string[] GetDefaultColumnsForGrid()
		{
			var defaultColumnsForGrid = base.GetDefaultColumnsForGrid();

			var cpcColumnInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedProcedure);
			if (cpcColumnInfo != null)
			{
				cpcColumnInfo.ColumnName = JobComInvoiceLine.Schema.JI_Procedure;
				var cpcColumn = Array.IndexOf(defaultColumnsForGrid, JobComInvoiceLine.Schema.JI_FormattedProcedure);
				if (cpcColumn != -1)
				{
					defaultColumnsForGrid[cpcColumn] = JobComInvoiceLine.Schema.JI_Procedure;
				}
			}

			return defaultColumnsForGrid;
		}

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();

		protected override ZBool DynamicLayoutApplied => ZBool.True;

		protected override ZBool HasDifferentPanelLayout => ZBool.True;

		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl() => new ExportInvoiceLineChargesUserControl();

		void ReorderTabPages()
		{
			LineDetailTabControl.TabPages.Remove(DangerousGoodsTabPage);

			LineDetailTabControl.TabPages.Remove(NewLineDetailsTabPage);
			LineDetailTabControl.TabPages.Insert(NewLineDetailsTabPage, 0);

			LineDetailTabControl.TabPages.Remove(OrganizationsTabPage);
			LineDetailTabControl.TabPages.Insert(OrganizationsTabPage, 1);

			LineDetailTabControl.TabPages.Remove(LineChargesTabPage);
			LineDetailTabControl.TabPages.Insert(LineChargesTabPage, 2);

			LineDetailTabControl.TabPages.Remove(SupportingDocumentsTabPage);
			LineDetailTabControl.TabPages.Insert(SupportingDocumentsTabPage, 3);

			LineDetailTabControl.TabPages.Remove(AdditionalInfosTabPage);
			LineDetailTabControl.TabPages.Insert(AdditionalInfosTabPage, 4);

			LineDetailTabControl.TabPages.Remove(PreviousDocumentsTabPage);
			LineDetailTabControl.TabPages.Insert(PreviousDocumentsTabPage, 5);

			LineDetailTabControl.TabPages.Remove(PackagesPivotTabPage);
			LineDetailTabControl.TabPages.Insert(PackagesPivotTabPage, 6);

			LineDetailTabControl.TabPages.Remove(PreviousProceduresTabPage);
			LineDetailTabControl.TabPages.Insert(PreviousProceduresTabPage, 7);
		}
	}
}
