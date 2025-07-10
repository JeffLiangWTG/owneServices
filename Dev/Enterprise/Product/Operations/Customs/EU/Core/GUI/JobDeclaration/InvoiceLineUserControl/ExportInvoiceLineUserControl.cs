using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EUExportInvoiceLineUserControl : EUInvoiceLineUserControl
	{
		public EUExportInvoiceLineUserControl()
		{
			InitializeComponent();
			if (!this.DesignMode)
			{
				this.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);

				DangerousGoodsTabPage.RunWhenBindingOrFirstShown((s, args) =>
				{
					UNDGPanel.UpdateLayout(GetNewDangerousGoodsPanelLayout());
					var linkLabel = (ZLinkLabel)UNDGPanel.Controls[nameof(UNDGUserControl.DGLinkLabel)];
					if (linkLabel != null)
					{
						var findBox = (ZGuidFindBox)UNDGPanel.Controls[nameof(UNDGUserControl.DGGuidFindBox)];
						var flashPointCalcEdit = (ZCalcEdit)UNDGPanel.Controls[nameof(UNDGUserControl.FlashpointUserControl)].Controls[nameof(UNDGFlashpointUserControl.FlashPointCalcEdit)];
						if (findBox != null && flashPointCalcEdit != null)
						{
							// side effect: if the linklabel is not added in the layout, we do not init the multiple items manager
							new UNDGDataItemFormManager(CustomsInvoiceLinesBoundGrid, "", UNDGDataItemFormManagerConfig.IMOShowProperties()).Initialize(linkLabel, findBox, flashPointCalcEdit);
						}
					}
				}
				);
			}
			LineDetailsTabPage.BindingOrFirstShown += LineDetailsTabPage_BindingOrFirstShown;
		}

		protected virtual bool IsZG_CountryOfDestinationVisible => JobDeclaration is JobDeclaration declaration && declaration.Configuration.InvoiceLineConfiguration.CountryOfDestinationVisibleOnExportControl(declaration);

		protected override void AddColumnsToGrid()
		{
			base.AddColumnsToGrid();

			CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.ZG_CountryOfDestination,
					IsVisible = IsZG_CountryOfDestinationVisible,
					IsUnavailable = !IsZG_CountryOfDestinationVisible
				},
				new ZTextBoxColumnStyleInfo(JobComInvoiceLine.Schema.JI_PreviousEntryNumber,  ControlDpiScalingHelper.ScaleToCurrentDpiX(100))
				{
					GroupName = Res.GetData("37F187AA-E0D2-4481-9335-3FB6FC26F83E", "Prev. Entry No."),
					IsVisible = false
				},
				new ZCalcEditColumnStyleInfo(JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber,  ControlDpiScalingHelper.ScaleToCurrentDpiX(120), 0)
				{
					GroupName = Res.GetData("37F187AA-E0D2-4481-9335-3FB6FC26F83E", "Prev. Entry No."),
					IsVisible = false
				},
				new ZCalcEditColumnStyleInfo(JobComInvoiceLine.Schema.JI_BondedWhsQuantity, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), 0)
				{
					IsVisible = false
				},
				new ZDropEditColumnStyleInfo(JobComInvoiceLine.Schema.JI_BondedWhsUnitQty, ControlDpiScalingHelper.ScaleToCurrentDpiX(80))
				{
					IsVisible = false
				},
				new ZCodeFindBoxColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.ZG_CusNumber,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
					IsVisible = IsUCC6,
					IsUnavailable = !IsUCC6
				}
			});
		}

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();

		protected override void ShowHideVatGstDutyCalcuationsAndMoveEverythingElseUpIfNeeded()
		{
			JI_Calc_GSTConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_DutyConvertToLocalCurrencyControl.Visible = false;
		}

		protected override Control GetFirstCalculationControlToWorkOutJiggledPOsitionOfOthers()
		{
			return JI_Calc_DutyConvertToLocalCurrencyControl;
		}

		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.Export;

		public static void SetSpoffOnLine(ZGrid grid)
		{
			if (grid.SelectedElements.Length > 0)
			{
				foreach (BusinessObject bo in grid.SelectedElements)
				{
					JobComInvoiceLine line = bo as JobComInvoiceLine;
					if (line != null)
					{
						line.SetSupervisingOfficeFromDeclarant();
					}
				}
			}
			else
			{
				int currentRowEvenIfNotFormallySelected = grid.CurrentRowIndex;
				JobDeclaration dec = (JobDeclaration)(grid.DataSource);
				if (currentRowEvenIfNotFormallySelected < 0 || currentRowEvenIfNotFormallySelected >= dec.FilteredInvoiceLines.Count)
				{
					Globals.Message.ShowWarning(Res.GetString("319503c3-a9f3-4487-86f0-2031c61a5d75", "You need to enter an invoice line before you can assign a Supervising Office to it"));
					return;
				}
				JobComInvoiceLine line = dec.FilteredInvoiceLines[currentRowEvenIfNotFormallySelected];
				if (line != null)
				{
					line.SetSupervisingOfficeFromDeclarant();
				}
			}
		}

		protected override string[] GetDefaultColumnsForGrid()
		{
			var result = new List<string>(base.GetDefaultColumnsForGrid());
			if (IsZG_CountryOfDestinationVisible)
			{
				result.Add(JobComInvoiceLine.Schema.ZG_CountryOfDestination);
			}

			if (IsUCC6)
			{
				result.Add(JobComInvoiceLine.Schema.ZG_CusNumber);
			}
			return result.ToArray();
		}

		protected override Type GetAdditionalInfosUserControlType() => IsUCC6
			? typeof(AdditionalInfosUserControlWithGrid)
			: typeof(AdditionalInfosUserControl);

		protected virtual IPanelLayoutProvider GetNewDangerousGoodsPanelLayout() => new UNDGLayout();

		void LineDetailsTabPage_BindingOrFirstShown(object sender, EventArgs e)
		{
			DestinationCodeFindBox.Visible = IsZG_CountryOfDestinationVisible;
		}

		bool IsUCC6 => JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC6;
	}
}
