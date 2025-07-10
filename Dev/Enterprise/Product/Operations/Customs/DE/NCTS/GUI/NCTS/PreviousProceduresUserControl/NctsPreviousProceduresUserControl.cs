using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.DE.GUI;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public partial class NctsPreviousProceduresUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl
	{
		public NctsPreviousProceduresUserControl()
		{
			InitializeComponent();
			InitializeGridLayoutCore();

			PreviousDocumentsGrid.AfterBind += PreviousDocumentsGrid_AfterBind;
		}

		void PreviousDocumentsGrid_AfterBind(object sender, EventArgs e)
		{
			MasterCSI_Procedure_ValueChanged(sender, e);
		}

		NctsPreviousDocument PreviousDocument => (NctsPreviousDocument)PreviousDocumentsGrid.ListManager?.GetCurrent();

		protected override void InitializeGridLayoutCore()
		{
			PreviousDocumentsGrid.ColumnStyles.Remove(PreviousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_ReferenceNumber2));
			PreviousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_ReferenceNumber).CharacterCasing = CharacterCasing.Normal;

			var columnStylesToAdd = new[]
			{
				new ZTextBoxColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_ReferenceNumber2,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					CharacterCasing = CharacterCasing.Normal
				},
				new ZTextBoxColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_Description,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
					CharacterCasing = CharacterCasing.Normal
				},
				new Universal.GUI.TariffColumnStyleInfo
				{
					ColumnName = NctsPreviousDocument.Schema.FormattedTariff,
					GetCountryCode = () => Core.Constants.CountryCodes.Germany,
					GetDataGrouping = () => Core.Constants.CountryCodes.Germany,
					TariffType = Universal.Constants.TariffTypes.Import,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106),
					GetEffectiveDate = () => ZDateTime.Now,
				},
				new ZCalcEditColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_Quantity,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(124),
					GroupName = Res.GetData("733C5E71-EF0F-45E6-988B-4F0C74E8C561", "Quantity Group")
				},
				new ZDropEditColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_UnitOfQuantity,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112),
					GroupName = Res.GetData("733C5E71-EF0F-45E6-988B-4F0C74E8C561", "Quantity Group"),
					CharacterCasing = CharacterCasing.Upper
				},
				new ZCalcEditColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_Quantity2,
					Decimals = 3,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93),
					GroupName = Res.GetData("3F0FC03F-15B6-408B-98E8-F83294EFA031", "Quantity 2 Group")
				},
				new ZDropEditColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_UnitOfQuantity2,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112),
					GroupName = Res.GetData("3F0FC03F-15B6-408B-98E8-F83294EFA031", "Quantity 2 Group"),
					CharacterCasing = CharacterCasing.Upper
				},
				new ZCodeFindBoxColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_CustomsOffice,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					CharacterCasing = CharacterCasing.Upper
				},
				new ZTextBoxColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_Procedure,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72),
					CharacterCasing = CharacterCasing.Upper,
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_AdditionalDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84),
					CharacterCasing = CharacterCasing.Normal
				},
				new ZCheckBoxColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.Status,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105)
				},
				new ZCheckBoxColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.UsualProcessingFlag,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128)
				}
			};

			PreviousDocumentsGrid.ColumnStyles.AddRange(columnStylesToAdd);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (nctsHeader != null)
			{
				nctsHeader.OnPreviousProcedureMasterCSI_ProcedureAboutToChange -= PreviousProcedureCodeAboutToChange;
			}
			nctsHeader = dataSource as NctsHeader;
			base.SetDataBinding(dataSource, dataMember);
			if (nctsHeader != null)
			{
				nctsHeader.OnPreviousProcedureMasterCSI_ProcedureAboutToChange += PreviousProcedureCodeAboutToChange;
			}
		}
		NctsHeader nctsHeader;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem is NctsDepartureCargoDesc goodsItem)
			{
				goodsItem.PreviousProcedureMaster.CSI_Procedure_ValueChanged -= MasterCSI_Procedure_ValueChanged;
				goodsItem.PreviousProcedureMaster.CSI_Procedure_ValueChanged += MasterCSI_Procedure_ValueChanged;
				MasterCSI_Procedure_ValueChanged(null, null);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (CurrentDataItem is NctsDepartureCargoDesc goodsItem)
				{
					goodsItem.PreviousProcedureMaster.CSI_Procedure_ValueChanged -= MasterCSI_Procedure_ValueChanged;
				}
				if (nctsHeader != null)
				{
					nctsHeader.OnPreviousProcedureMasterCSI_ProcedureAboutToChange -= PreviousProcedureCodeAboutToChange;
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void MasterCSI_Procedure_ValueChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem is NctsDepartureCargoDesc goodsItem)
			{
				SetPreviousDocumentsGridColumnsVisible(goodsItem.PreviousProcedureMaster.CSI_Procedure);
			}
		}

		void ImportSumARegisterButtonClick(object sender, EventArgs e)
		{
			using (var module = ZModuleFactory.Instance.Create(ModuleIDs.Customs.EU.DE.ImportFromSumARegister) as ZFilterGridModule)
			{
				var parentForm = ParentForm;
				module.SetFormsModalTo(parentForm);
				if (CurrentDataItem is NctsDepartureCargoDesc goodsItem)
				{
					using (var sumARegisterPopup = module.ShowPopup() as ImportFromSumARegisterModuleForm)
					{
						sumARegisterPopup.PreviousDocuments = goodsItem.PreviousProcedures;
						ZFormModaliser.ShowDialogWithoutDispose(sumARegisterPopup, parentForm);
					}
				}
			}
		}

		void PreviousProcedureCodeAboutToChange(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (sender is NctsPreviousProcedureMaster nctsPreviousProcedureMaster)
			{
				var procedureCode = nctsPreviousProcedureMaster.CSI_Procedure;
				if (!procedureCode.IsEmpty && NctsPreviousDocumentHelper.NctsProcedureCodeSupportsMultiplePreviousDocuments(procedureCode))
				{
					e.Cancel = Globals.Message.Show(
						Res.GetString("C0CF47E3-E0D5-4FCF-BEB6-26E9E22B622F", "The System is about to delete all existing Previous Procedure lines.\r\nDo you want to continue?"),
						Res.GetString("8EA44D9D-B868-499A-8D9E-1B4F6CE41AE5", "Previous Procedure Deletion"),
						MessageBoxButtons.OKCancel,
						DialogResult.Cancel) == DialogResult.Cancel;
				}
			}
		}

		public void SetPreviousDocumentsGridColumnsVisible(string procedure)
		{
			var availableColumns = NctsPreviousDocumentHelper.GetAvailableNctsColumns(procedure);
			if (!availableColumns.Any())
			{
				PreviousDocumentsGrid.Visible = false;
				ResetAvailableControls();
			}
			else
			{
				PreviousDocumentsGrid.Visible = true;
				var availableColumnNames = availableColumns.Select(x => x.ColumnName).ToArray();
				var unAvailableColumns = NctsPreviousDocumentHelper.GetUnavailableColumns(availableColumnNames);
				PreviousDocumentsGrid.RemoveFromAvailableColumns(unAvailableColumns);
				PreviousDocumentsGrid.AddToAvailableColumns(availableColumnNames);
				PreviousDocumentsGrid.ReOrderColumns(availableColumnNames);

				SetPreviousDocumentsGridCaption(availableColumns);
				ResetAvailableControls();
				SetAvailableControls(procedure);
			}
			ImportSumARegisterButton.Visible = procedure == NctsPreviousProcedureList.Codes._N337 && GoodsItem != null;
		}

		void SetPreviousDocumentsGridCaption(IEnumerable<(string ColumnName, string Caption)> availableColumns)
		{
			foreach (var (columnName, caption) in availableColumns)
			{
				PreviousDocumentsGrid.SetColumnCaption(columnName, caption);
			}
		}

		void SetAvailableControls(string procedure)
		{
			if (procedure == NctsPreviousProcedureList.Codes._9DEZ)
			{
				SetAvailableFields_9DEZ();
			}
			else if (procedure == NctsPreviousProcedureList.Codes._9DEY)
			{
				SetAvailableFields_9DEY();
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

		void SetAvailableFields_9DEZ() => AddTemporaryControl(new NctsPreviousProceduresATZLPanel());

		void SetAvailableFields_9DEY() => AddTemporaryControl(new NctsPreviousProceduresATAVPanel());

		List<Control> TemporaryControls => temporaryControls ?? (temporaryControls = new List<Control> { });
		List<Control> temporaryControls;

		NctsDepartureCargoDesc GoodsItem => PreviousDocument?.Parent;
	}
}
