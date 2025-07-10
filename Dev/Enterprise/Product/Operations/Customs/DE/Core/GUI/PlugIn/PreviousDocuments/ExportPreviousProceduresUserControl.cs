using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportPreviousProceduresUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl
	{
		public ExportPreviousProceduresUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		public void SetPreviousDocumentsGridColumnsVisible(ZString procedure)
		{
			var previousDocumentConfig = new PreviousDocumentConfiguration();
			var availableColumns = previousDocumentConfig.GetAvailableColumnsFromProcedureCode(false, procedure, ZString.Empty);
			var availableColumnNames = availableColumns.Select(x => x.ColumnName).ToArray();

			PreviousDocumentsGrid.Visible = previousDocumentConfig.ProcedureCodeSupportsMultiplePreviousDocuments(false, procedure);
			PreviousDocumentsGrid.RemoveFromAvailableColumns(previousDocumentConfig.GetUnavailableColumns(availableColumnNames));
			PreviousDocumentsGrid.AddToAvailableColumns(availableColumnNames);
			PreviousDocumentsGrid.ReOrderColumns(availableColumnNames);

			SetPreviousDocumentsGridCaption(availableColumns);
			SetAvailableControls(procedure);
		}

		#region Temporary Controls

		void SetAvailableControls(string procedure)
		{
			ResetAvailableControls();

			if (procedure == PreviousProcedureList.Codes._ATZL)
			{
				SetAvailableFields_ATZL();
			}
			else if (procedure == PreviousProcedureList.Codes._ATAV)
			{
				SetAvailableFields_ATAV();
			}
		}

		void ResetAvailableControls()
		{
			foreach (var control in TemporaryControls)
			{
				ProcedureGroupBox.Controls.Remove(control);
			}
			temporaryControls = null;
		}

		void AddTemporaryControl(Control control)
		{
			ProcedureGroupBox.Controls.Add(control);
			TemporaryControls.Add(control);
		}

		void SetAvailableFields_ATZL() => AddTemporaryControl(new PreviousProcedureExportATZLPanel(BindingPrefix));
		void SetAvailableFields_ATAV() => AddTemporaryControl(new PreviousProcedureExportATAVPanel(BindingPrefix));

		string BindingPrefix
		{
			get
			{
				if (!bindingPrefix.HasValue)
				{
					var gridBinding = ((EU.GUI.PlugIn.ISupportingInfoUserControls)this).Grid.BindTo;
					bindingPrefix = gridBinding.Substring(0, gridBinding.LastIndexOf(".", StringComparison.Ordinal));
				}
				return bindingPrefix.Value;
			}
		}
		ZString? bindingPrefix;

		List<Control> TemporaryControls => temporaryControls ?? (temporaryControls = new List<Control>());
		List<Control> temporaryControls;

		#endregion

		void SetPreviousDocumentsGridCaption(IEnumerable<(string ColumnName, string Caption)> availableColumns)
		{
			foreach (var (columnName, caption) in availableColumns)
			{
				PreviousDocumentsGrid.SetColumnCaption(columnName, caption);
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			PreviousDocumentsGrid.ColumnStyles.Clear();
			PreviousDocumentsGrid.ColumnStyles.AddRange(
				new IZColumnStyleInfo[]
				{
					new ZTextBoxColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_Procedure,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72),
						CharacterCasing = CharacterCasing.Upper
					},
					new ZTextBoxColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_ReferenceNumber,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
						CharacterCasing = CharacterCasing.Normal
					},
					new ZDropEditColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_SubType,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(167),
						CharacterCasing = CharacterCasing.Upper
					},
					new ZDateEditColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_DateOfIssue,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
					},
					new ZCalcEditColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_LineNo,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66)
					},
					new ZCheckBoxColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.Status,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105),
						CharacterCasing = CharacterCasing.Upper
					},
					new ZTextBoxColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_Description,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
						CharacterCasing = CharacterCasing.Normal
					},
					new Universal.GUI.TariffColumnStyleInfo()
					{
						ColumnName = nameof(PreviousDocument.FormattedTariff),
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106),
						GetCountryCode = () => Core.Constants.CountryCodes.Germany,
						GetDataGrouping = () => Core.Constants.CountryCodes.Germany,
						TariffType = Universal.Constants.TariffTypes.Import,
						GetEffectiveDate = () => ZDateTime.Now,
					},
					new ZCalcEditColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_Quantity,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107)
					},
					new ZDropEditColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_UnitOfQuantity,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(38),
						CharacterCasing = CharacterCasing.Upper
					},
					new ZCalcEditColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_Quantity2,
						Decimals = 3,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93)
					},
					new ZDropEditColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_UnitOfQuantity2,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(38),
						CharacterCasing = CharacterCasing.Upper
					},
					new ZCodeFindBoxColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.CSI_CustomsOffice,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(151),
						CharacterCasing = CharacterCasing.Upper
					},
					new ZCheckBoxColumnStyleInfo()
					{
						ColumnName = PreviousDocument.Schema.UsualProcessingFlag,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128)
					}
				});
		}
	}
}
