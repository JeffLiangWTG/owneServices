using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUCMRImportInvoiceLineUserControl : AUImportInvoiceLineUserControl
	{
		public AUCMRImportInvoiceLineUserControl()
		{
			InitializeComponent();
			UpdateComponentProperty();
		}

		void UpdateComponentProperty()
		{
			var premisesIdColumn = this.AQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
			premisesIdColumn.ModuleID = CMRReferenceDataHelper.UseReferenceData ? Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList : Enterprise.ZArchitecture.Modules.ModuleIDs.Premises;

			this.LineDetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			this.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.JI_CC.Name).CaptionResourceString = Res.GetData("AUCMRImportInvoiceLineUserControl|35db7c43-4cb2-48ba-8a63-7d20ed53cf28", "Look Up Code Class.", "Classification Lookup Code", "Enter the Classification Lookup code used to default the Line Tariff details. Classification Lookups are created and maintained in the Customs Files menu.");
			this.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.JI_CustomsUnitQty.Name).CaptionResourceString = Res.GetData("AUCMRImportInvoiceLineUserControl|ffd0d82f-c967-4a3d-bb45-133bc1389490", "Cust. UQ", "Customs UQ", "Customs Unit Quantity (Statistical) as indicated by the tariff item entered.Some Tariffs require more than one Customs UQ. Additional UQ's can be added in the Add Info form.");

			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_CustomsValue";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.ICSPermitTabPage.TabVisible = true;
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			bool isExWarehouse = JobDeclaration.IsExWarehouse;
			ContainersTabPage.Text = "Containers";
			ContainersGroupBox.Text = "Containers";
			CusContainerInvoiceLineGrid.Visible = !isExWarehouse;
			ContainerTabHiddenLabel.Visible = isExWarehouse;
			AQISTabHiddenLabel.Visible = isExWarehouse;
			AQISDetailsPanel.Visible = !isExWarehouse;
			ValuationBasisBoundDropEdit.Visible = !isExWarehouse;
			HeaderValuationBasisTextBox.Visible = !isExWarehouse;
			TransactionBasisZDropEdit.Visible = !isExWarehouse;
			JI_LinePriceBoundCurrencyControl.GetExtension<ILabelCaptionRenderer>().Options = StringRenderingOptions.Wrap;
			JI_LinePriceBoundCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption = isExWarehouse ? "Customs Value" : "Price";
			CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLineSchema.Constants.JI_LinePrice, JobDeclaration.IsExWarehouse ? "Customs Value" : "Price");
		}

		protected override void SetDynamicControlStates()
		{
			currencyVisible = AdjustmentDollarPercentageBoundDropEdit.Text != "%";
			AdjustmentCurrencyBoundFindBox.Visible = currencyVisible;
			SaveCurrentCaptions();
		}

		protected override void CustomsInvoiceLinesBoundGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			base.CustomsInvoiceLinesBoundGrid_ListManager_CurrentChanged(sender, e);

			RefreshCaptions();
		}

		void SaveCurrentCaptions()
		{
			if (string.IsNullOrEmpty(dutyCaption))
			{
				dutyCaption = DutyConvertToLocalCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption;
			}

			if (string.IsNullOrEmpty(taxCaption))
			{
				taxCaption = GSTConvertToLocalCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption;
			}
		}
		string dutyCaption;
		string taxCaption;

		void RefreshCaptions()
		{
			var listManager = CustomsInvoiceLinesBoundGrid.ListManager;
			var currentInvoiceLine = listManager != null ? (JobComInvoiceLine)listManager.GetCurrent() : null;
			if (currentInvoiceLine != null)
			{
				DutyConvertToLocalCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption = currentInvoiceLine.DutyControlCaptionPrefix + dutyCaption;
				GSTConvertToLocalCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption = currentInvoiceLine.GSTControlCaptionPrefix + taxCaption;
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			ClassificationFindBox.ModuleID = ClassificationModuleID;
			var declaration = AUJobDeclaration;
			if (declaration != null && declaration.IsWHSUniversalXMLActive)
			{
				using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(AddInfoPrefix + AUAddInfo.Schema.UseBondedWarehouseAutomation);
					if (!declaration.IsExWarehouse)
					{
						CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(AddInfoPrefix + AUAddInfo.Schema.WarehouseOrgFK, AddInfoPrefix + AutoAUAddInfo.Schema.ZA_OA_WarehouseAddress_Hidden);
					}
				}
			}
		}

		protected override string[] GetInvoiceLinesGridColumnOrder()
		{
			List<string> result = new List<string>();
			result.AddRange(base.GetInvoiceLinesGridColumnOrder());
			result.Add(AddInfoPrefix + AutoAUAddInfo.Schema.ZA_POC);
			result.Add(AddInfoPrefix + AutoAUAddInfo.Schema.ZA_PST);
			result.Add(AddInfoPrefix + AutoAUAddInfo.Schema.ZA_PRT);
			result.Add(AddInfoPrefix + AutoAUAddInfo.Schema.ZA_RNO);
			result.Add(AddInfoPrefix + AUAddInfo.Schema.WarehouseOrgFK);
			result.Add(AddInfoPrefix + AutoAUAddInfo.Schema.ZA_OA_WarehouseAddress_Hidden);
			AddToListIfNotExists(result, AddInfoPrefix + AutoAUAddInfo.Schema.ZA_WRQ);
			AddToListIfNotExists(result, AddInfoPrefix + AutoAUAddInfo.Schema.ZA_WRU);
			result.Add(AddInfoPrefix + AutoAUAddInfo.Schema.ZA_TILV);
			AddToListIfNotExists(result, JobComInvoiceLineSchema.Constants.JI_OH_Supplier);
			return result.ToArray();
		}

		protected override string[] GetDefaultColumnsForGrid()
		{
			List<string> result = new List<string>();
			result.AddRange(base.GetDefaultColumnsForGrid());
			result.Add(AddInfoPrefix + AUAddInfo.Schema.UseBondedWarehouseAutomation);
			var declaration = AUJobDeclaration;
			if (declaration.IsExWarehouse)
			{
				if (declaration.IsWHSUniversalXMLActive)
				{
					result.Add(AddInfoPrefix + AutoAUAddInfo.Schema.ZA_WRN);
					result.Add(AddInfoPrefix + AutoAUAddInfo.Schema.ZA_WRL);
					result.Add(AddInfoPrefix + AutoAUAddInfo.Schema.ZA_WRQ);
					result.Add(AddInfoPrefix + AutoAUAddInfo.Schema.ZA_WRU);
				}

				result.Add(JobComInvoiceLineSchema.Constants.JI_OH_Supplier);
			}
			return result.ToArray();
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			var headerData = JobDeclaration;
			var declaration = headerData as JobDeclaration;
			CustomsInvoiceLinesBoundGrid.SetAvailability(!declaration.IsWHSUniversalXMLActive && declaration.IsExWarehouse && declaration.SupportsBondedWarehousing, AddInfoPrefix + AUAddInfo.Schema.UseBondedWarehouseAutomation);
			CustomsInvoiceLinesBoundGrid.SetAvailability(headerData.IsExWarehouse, JobComInvoiceLineSchema.Constants.JI_OH_Supplier);
		}

		#region JobDeclaration TypeCast

		JobDeclaration AUJobDeclaration
		{
			get { return (JobDeclaration)JobDeclaration; }
		}

		#endregion

		#region CantCreateInvoiceLinesText

		protected override ZString CantCreateInvoiceLinesText
		{
			get
			{
				if (AUJobDeclaration.IsSACWithoutLines)
				{
					return Res.GetString("9c0ad6b6-e231-4a84-b260-36ea0e4ed16b", @"Invoice Lines are not applicable to a SAC without lines.
If Invoice Lines are required you should select SWL in the Style field on the Declaration Tab.");
				}
				else
				{
					return base.CantCreateInvoiceLinesText;
				}
			}
		}

		#endregion
	}
}
