using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVSLinesUserControl : ZUserControl
	{
		public LVSLinesUserControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				this.LVSLinesGrid.AfterBind += LVSLinesGrid_AfterBind;
			}

			ClassificationTariffUserControlHelper.UpdateTariffColumnStyleInfoToGetTariffFromSRDb(
				LVSLinesGrid,
				JobComInvoiceLine.Schema.JI_FormattedTariff,
				() => { return currentInvoiceLine?.EffectiveDateForDutyRate ?? ZDateTime.Today; }
				);
		}

		void LVSLinesGrid_AfterBind(object sender, EventArgs e)
		{
			LVSLinesGrid.ListManager.CurrentChanged += LVSLinesGridListManager_CurrentChanged;
			LVSLinesGridListManager_CurrentChanged(sender, e);
			if (fInvoiceLineGridLayoutPersister == null)
			{
				CreateInvoiceLineGridLayoutPersister();
			}
			fInvoiceLineGridLayoutPersister?.ConfigOrgChanged();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (fInvoiceLineGridLayoutPersister != null)
			{
				fInvoiceLineGridLayoutPersister.Dispose();
				fInvoiceLineGridLayoutPersister = null;
			}
			CreateInvoiceLineGridLayoutPersister();
			fInvoiceLineGridLayoutPersister?.ConfigOrgChanged();
		}

		void CreateInvoiceLineGridLayoutPersister()
		{
			var currentDataItem = CurrentDataItem;
			if (currentDataItem != null)
			{
				fInvoiceLineGridLayoutPersister = new CustomLabelsGridLayoutPersister(
					LVSLinesGrid, new BaseJobComInvoiceLine.CustomLabelsProvider((ICustomsCustomLabelsConfigOrgProvider)currentDataItem), "", "");
				((JobComInvoiceHeader)currentDataItem).JZ_OH_BuyerInfo.ValueChanged += InvoiceJZ_OH_BuyerValueChanged;
			}
		}

		void InvoiceJZ_OH_BuyerValueChanged(object sender, EventArgs e)
		{
			fInvoiceLineGridLayoutPersister.ConfigOrgChanged();
		}

		protected internal CustomLabelsGridLayoutPersister fInvoiceLineGridLayoutPersister;

		void LVSLinesGridListManager_CurrentChanged(object sender, EventArgs e)
		{
			JobComInvoiceLine invoiceLine = null;
			var listManager = LVSLinesGrid.ListManager;
			if (listManager != null)
			{
				invoiceLine = (JobComInvoiceLine)listManager.GetCurrent();
				if (invoiceLine != null && invoiceLine.IsDeleted)
				{
					invoiceLine = null;
				}

				if (currentInvoiceLine != invoiceLine)
				{
					UnHookInvoiceLineEvent(currentInvoiceLine);
					currentInvoiceLine = invoiceLine;
					HookInvoiceLineEvent(invoiceLine);
				}
			}
		}
		JobComInvoiceLine currentInvoiceLine;

		void UnHookInvoiceLineEvent(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null)
			{
				invoiceLine.OnRefreshSIMAMeasureEvent = null;
			}
		}

		void HookInvoiceLineEvent(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null)
			{
				invoiceLine.OnRefreshSIMAMeasureEvent += delegate
				{
					var simaMeasuresForm = new SIMADumpingNumberForm(invoiceLine.SIMAMeasures, invoiceLine.JI_Tariff);
					var parentForm = FindForm();
					simaMeasuresForm.Icon = parentForm.Icon;
					if (simaMeasuresForm.ShowDialog(parentForm) == DialogResult.OK)
					{
						return simaMeasuresForm.SelectedDumpingNumber;
					}

					return null;
				};
			}
		}

		internal void SetSimplifiedLVSMode()
		{
			IsSimplifiedLVSMode = true;
			this.LVSLineChargesUserControl.IsSimplifiedLVSMode = IsSimplifiedLVSMode;
			InitializeSimplififedLVSModeGridLayout();
		}

		internal void SetLVXMode()
		{
			InitializeLVXModeGridLayout();
		}
#if DEBUG
		internal
#endif
		bool IsSimplifiedLVSMode;

		internal void SetGroupBoxText(string text)
		{
			this.LVSLinesGroupBox.Text = text;
			this.LineDetailsGroupBox.Text = text;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (ListManager != null)
			{
				ListManager.CurrentChanged += ListManager_CurrentChanged;
				ListManager.CurrentItemChanged += ListManager_CurrentChanged;
			}
			SetRemissionDropEditAndRemissionTypeDropEditVisible();
		}

		void SetRemissionDropEditAndRemissionTypeDropEditVisible()
		{
			var remissionDropEditVisible = false;
			var remissionTypeDropEditVisible = false;
			if (InvoiceLine is JobComInvoiceLine line)
			{
				if (line.CA_CalculationMethod != CalculationMethods.Codes.DeliveredDutyPaid)
				{
					remissionDropEditVisible = true;
				}

				if (line.Declaration is JobDeclaration declaration && declaration.IsCADEnabled)
				{
					remissionTypeDropEditVisible = true;
				}
			}
			LVSLineAllDetailsUserControl.LVSLineDetailsUserControl.RemissionDropEdit.Visible = remissionDropEditVisible;
			LVSLineAllDetailsUserControl.LVSLineDetailsUserControl.RemissionTypeDropEdit.Visible = remissionTypeDropEditVisible;
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (InvoiceLine != null)
			{
				InvoiceLine.SetReadOnlyIncludingChildren(InvoiceLine.CA_IsAutoDummyHSCodeCasualImportLine);
				SetRemissionDropEditAndRemissionTypeDropEditVisible();
			}
		}

		CurrencyManager ListManager
		{
			get { return LVSLinesGrid?.ListManager; }
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)ListManager?.GetCurrent(); }
		}

		internal OrgHeader GetEffectiveImporterForLVSLinesGrid()
		{
			OrgHeader effectiveImporter = null;
			var invoice = CurrentDataItem as JobComInvoiceHeader;
			if (invoice != null)
			{
				if (invoice.IsAttachedToPersistentLVXDeclaration)
				{
					effectiveImporter = invoice.Buyer;
				}
				else if (IsSimplifiedLVSMode && invoice.JobDeclaration != null)
				{
					effectiveImporter = invoice.JobDeclaration.Importer;
				}
				else
				{
					effectiveImporter = invoice.Importer_Effective;
				}
			}
			return effectiveImporter;
		}

		void InitializeLVXModeGridLayout()
		{
			var b3LineNumberColumnStyleInfo = LVSLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_B3LineNumber);
			b3LineNumberColumnStyleInfo.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("40714A3A-7EFF-42CA-BC96-F19C8A3588FB", "Entry Line No");
			b3LineNumberColumnStyleInfo.ColumnName = "LVXB3LineNumber";
		}

		void InitializeSimplififedLVSModeGridLayout()
		{
			var currencyColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			currencyColumnStyleInfo.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bad17f87-867a-48ad-a563-6d38d4d8ece9", "Currency");
			currencyColumnStyleInfo.ColumnName = "InvoiceHeader+JZ_RX_NKInvoice_Currency";
			currencyColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			LVSLinesGrid.ColumnStyles.Add(currencyColumnStyleInfo);

			LVSLinesGrid.SetAllColumnsVisible(false);
			LVSLinesGrid.SetColumnVisible(true, SimplififedLVSModeColumnsSequence);
			LVSLinesGrid.ReOrderColumns(SimplififedLVSModeColumnsSequence);
		}

		string[] SimplififedLVSModeColumnsSequence
		{
			get
			{
				if (simplififedLVSModeColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						JobComInvoiceLine.Schema.JI_PartNo,
						JobComInvoiceLine.Schema.JI_Description,
						JobComInvoiceLine.Schema.JI_CountryOfOrigin,
						JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin,
						JobComInvoiceLine.Schema.CA_TreatmentCode,
						JobComInvoiceLine.Schema.JI_InvoiceQuantity,
						JobComInvoiceLine.Schema.JI_InvoiceUQ,
						JobComInvoiceLine.Schema.JI_CustomsQuantity,
						JobComInvoiceLine.Schema.JI_CustomsUnitQty,
						JobComInvoiceLine.Schema.UnitPrice,
						"InvoiceHeader+JZ_RX_NKInvoice_Currency",
						JobComInvoiceLine.Schema.JI_LinePrice,
						JobComInvoiceLine.Schema.CA_RN_NKExport,
						JobComInvoiceLine.Schema.CA_USStateOfExport,
						JobComInvoiceLine.Schema.JI_CC,
						JobComInvoiceLine.Schema.JI_FormattedTariff,
						JobComInvoiceLine.Schema.CA_99TariffCode,
						JobComInvoiceLine.Schema.JI_LineNo,
						JobComInvoiceLine.Schema.JI_B3LineNumber
					};
					simplififedLVSModeColumnsSequence = columnList.ToArray();
				}
				return simplififedLVSModeColumnsSequence;
			}
		}
		string[] simplififedLVSModeColumnsSequence;
	}
}
