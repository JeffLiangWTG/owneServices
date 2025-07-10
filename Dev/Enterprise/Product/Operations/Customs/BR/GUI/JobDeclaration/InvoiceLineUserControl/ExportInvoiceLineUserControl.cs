using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Registry;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ExportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			InitCreatePermitMenuItem();
		}

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();

		protected override ZBool DynamicLayoutApplied => true;

		protected JobDeclaration Declaration => JobDeclaration as JobDeclaration;

		protected override void AddNewColumnForCustomsInvoiceLinesBoundGrid()
		{
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_Procedure, 80);

			var zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = JobComInvoiceLine.Schema.JI_NFeLinePrice;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);

			var zCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			zCodeFindBoxColumnStyleInfo.BindToList = "Lookups.CountryList";
			zCodeFindBoxColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport;
			zCodeFindBoxColumnStyleInfo.ModuleID = ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo);

			var zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = JobComInvoiceLine.Schema.JI_FinancedValue;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);

			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_SecondCPC, 80);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_ThirdCPC, 80);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_FourthCPC, 80);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_NFeNumber, 280);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_NFeItemNumber, 80);

			var zDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.JI_CargoPriority;
			zDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo);

			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.ComplementaryDescription, 80);

			if (JobDeclaration is JobDeclaration declaration && declaration.IsPersistent)
			{
				var zMultiLineTextBoxColumnInfo = new ZMultiLineTextBoxColumnInfo();
				zMultiLineTextBoxColumnInfo.ColumnName = JobComInvoiceLine.Schema.JI_ExportJustificationInfo;
				zMultiLineTextBoxColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo);
			}

			var zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = JobComInvoiceLine.Schema.JI_AgentCommissionPercentage;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);

			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_IntendedTermDays, 80);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (Declaration != null)
			{
				var isPersistent = Declaration.IsPersistent;

				SuspensionDrawbackTab.TabVisible = isPersistent;
				BRPreviousDocumentTabPage.TabVisible = isPersistent;
				BRReferenceInvoiceManualTabPage.TabVisible = isPersistent;
				BRElectronicLogisticInvoiceTabPage.TabVisible = isPersistent;

				if (Declaration.IsLPCO)
				{
					InvoiceLinesSummaryGroupBox.Visible = false;
				}
			}
		}

		protected override IEnumerable<string> GetDefaultColumnsInOrderCore() => new List<string>
		{
			JobComInvoiceLine.Schema.JI_LineNo,
			JobComInvoiceLine.Schema.JI_Calc_Invoice,
			JobComInvoiceLine.Schema.JI_PartNo,
			JobComInvoiceLine.Schema.JI_CC,
			JobComInvoiceLine.Schema.JI_Tariff,
			JobComInvoiceLine.Schema.JI_InvoiceQuantity,
			JobComInvoiceLine.Schema.JI_InvoiceUQ,
			JobComInvoiceLine.Schema.JI_CustomsQuantity,
			JobComInvoiceLine.Schema.JI_CustomsUnitQty,
			JobComInvoiceLine.Schema.JI_LinePrice,
			JobComInvoiceLine.Schema.FullGoodsDescription,
			JobComInvoiceLine.Schema.JI_CountryOfOrigin,
			JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
			JobComInvoiceLine.Schema.JI_Weight,
			JobComInvoiceLine.Schema.JI_WeightUQ,
			JobComInvoiceLine.Schema.JI_NetWeight,
			JobComInvoiceLine.Schema.JI_NetWeightUQ,
			JobComInvoiceLine.Schema.JI_Volume,
			JobComInvoiceLine.Schema.JI_VolumeUQ,
			JobComInvoiceLine.Schema.JI_OrderNumber,
			JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
			JobComInvoiceLine.Schema.JI_PartAttrib1,
			JobComInvoiceLine.Schema.JI_PartAttrib2,
			JobComInvoiceLine.Schema.JI_PartAttrib3,
			JobComInvoiceLine.Schema.JI_SerialNumber,
			JobComInvoiceLine.Schema.JI_CustomAttrib1,
			JobComInvoiceLine.Schema.JI_CustomAttrib2,
			JobComInvoiceLine.Schema.JI_CustomAttrib3,
			JobComInvoiceLine.Schema.JI_CustomAttrib4,
			JobComInvoiceLine.Schema.JI_CustomAttrib5,
			JobComInvoiceLine.Schema.JI_CustomAttrib6,
			JobComInvoiceLine.Schema.JI_CustomTextBlob1,
			JobComInvoiceLine.Schema.JI_CEI,
			JobComInvoiceLine.Schema.JI_NFeNumber,
			JobComInvoiceLine.Schema.JI_NFeItemNumber,
			JobComInvoiceLine.Schema.JI_NFeLinePrice,
			JobComInvoiceLine.Schema.JI_Procedure,
			JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
			JobComInvoiceLine.Schema.JI_FinancedValue,
			JobComInvoiceLine.Schema.JI_SecondCPC,
			JobComInvoiceLine.Schema.JI_AgentCommissionPercentage
		};

		protected void BR_LPCO_CodesEditButton_Click(object sender, EventArgs e)
		{
			JobComInvoiceLine invoiceLine = null;
			var listManager = CustomsInvoiceLinesBoundGrid.ListManager;
			if (listManager != null)
			{
				invoiceLine = (JobComInvoiceLine)listManager.GetCurrent();
				if (invoiceLine != null && invoiceLine.IsDeleted)
				{
					invoiceLine = null;
				}
			}
			if (invoiceLine != null)
			{
				LPCOCollectionForm.ShowDialog(invoiceLine);
				invoiceLine.LPCOConcatenatedInfo.RefreshBinding();
			}
		}

		#region Create Permit

		MenuItem createPermitMenuItem;

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			if (createPermitMenuItem != null)
			{
				createPermitMenuItem.Enabled = CurrentInvoiceLine != null && CustomsInvoiceLinesBoundGrid.SelectedElements.Length == 1;
			}
		}

		void InitCreatePermitMenuItem()
		{
			if (BRCustomsDataRegistry.Instance.EnableLPCO.Value)
			{
				createPermitMenuItem = new ZMenuItem(ResString.GetMultilingualString("73af43c0-fe98-41b4-9769-31e5d1514c9b", "Create Permit"), (s, e) => CreatePermit());
				CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add(createPermitMenuItem);
				CustomsInvoiceLinesBoundGrid.ContextMenu.Popup += ContextMenu_Popup;
			}
		}

		internal ZController LPCOController => lpcoController ?? (lpcoController = ZControllerFactory.Create(ControllerIDs.Customs.BR.LPCO));
		ZController lpcoController;

		void CreatePermit()
		{
			if (PreSaveDeclaration() && CurrentInvoiceLine is JobComInvoiceLine invoiceLine)
			{
				if (LPCOController.ShowNewForm() is ZForm form && form.BusinessEntity is CusLPCOHeader lpco)
				{
					lpco.PopulateFromInvoiceLine(invoiceLine);
				}
			}
		}

		bool PreSaveDeclaration()
		{
			var topLevelBizObj = Declaration.Shipment as BusinessObject ?? Declaration;
			return Customs.GUI.PlugIn.CustomsPlugIn.FormPreSaved(topLevelBizObj, ParentForm as ZForm);
		}

		#endregion

	}
}
