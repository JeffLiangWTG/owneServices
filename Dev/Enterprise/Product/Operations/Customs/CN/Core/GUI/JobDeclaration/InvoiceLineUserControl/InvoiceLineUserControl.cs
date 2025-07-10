using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.GUI
{
	public partial class InvoiceLineUserControl : DeclarationInvoiceLineUserControl
	{
		public InvoiceLineUserControl()
		{
			LineDetailsTabPage.Controls.Remove(InvoiceDetailsGroupBox);

			InitializeComponent();
			InvoiceLineUserControlHelper.SetTariffRelated(CustomsInvoiceLinesBoundGrid, Name, JI_TariffFindBox, GetCustomsCountryCode, GetDataGroupingForUniversalTariff, GetUniversalTariffType());
			JI_TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = true;
			SetAdditionalTabOrder();
			JI_TariffFindBox.PartialDescriptionMinLengthForSearch = 2;
		}

		void SetAdditionalTabOrder()
		{
			LineDetailTabControl.TabPages.Remove(CusSupportingDocumentsTabPage);
			LineDetailTabControl.TabPages.Insert(CusSupportingDocumentsTabPage, 1);
			LineDetailTabControl.TabPages.Remove(CIQTabPage);
			LineDetailTabControl.TabPages.Insert(CIQTabPage, 2);
			LineDetailTabControl.TabPages.Remove(AttachmentsTabPage);
			LineDetailTabControl.TabPages.Insert(AttachmentsTabPage, 3);
			LineDetailTabControl.TabPages.Remove(CIQProductQualificationsTabPage);
			LineDetailTabControl.TabPages.Insert(CIQProductQualificationsTabPage, 4);
		}

		void ResetCustomsInvoiceLinesBoundGridColumnsOrder()
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(UnavailableColumns);
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(ColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(false, ColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, DefaultColumnsForGrid);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible((JobDeclaration as JobDeclaration).IsImport, ImpOnlyColumns);
				if (!(JobDeclaration is JobDeclaration dec && dec.IsPersistent))
				{
					CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(UnavailableColumnsOnCommercialInvoice);
				}
			}
		}

		void AddNewColumnForCustomsInvoiceLinesBoundGrid()
		{
			var zGuidDropEditColumnStyleInfo = new ZGuidDropEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var entryLineNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			var zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			var zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			var zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo7 = new ZDropEditColumnStyleInfo();
			var zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();

			zGuidDropEditColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.JI_CEI;
			zGuidDropEditColumnStyleInfo.CaptionResourceString = Res.GetData("BaseInvoiceLineUserControl|ad495f48-8a13-4e89-a329-04a7896e7d8f", "Ent.Inst.", "Entry Ins.", "Entry Instruction", "Entry Instruction Code");
			zGuidDropEditColumnStyleInfo.GroupName = Res.GetData("BaseInvoiceLineUserControl|dba4f410-68c6-431a-a23c-ec8c3fadfc68", "Entry Ins", "Entry Instruction", "Entry Instruction", "Customs Entry Instruction");
			zGuidDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.JI_CEI_Description;
			zTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("BaseInvoiceLineUserControl|9f97aa1c-ea9b-4aa0-afee-accdd2bedaa9", "Ent.Ins.Desc.", "Entry Ins. Desc.", "Entry Instruction Description", "Customs Entry Instruction Description");
			zTextBoxColumnStyleInfo.GroupName = zGuidDropEditColumnStyleInfo.GroupName;
			zTextBoxColumnStyleInfo.IsReadOnly = true;
			zTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			entryLineNumberTextBoxColumnStyleInfo.ColumnName = "CusEntryLine+EntryLRNAndEntryLineNo";
			entryLineNumberTextBoxColumnStyleInfo.IsReadOnly = true;
			entryLineNumberTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("8BF164EE-ED24-46E1-A5F5-9DFE0844BB95", "Entry Line #", "Entry Line No.", "Entry Line Number");
			entryLineNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo6.ColumnName = JobComInvoiceLine.Schema.JI_CEI_StyleDescription;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Res.GetData("A5E54D3F-4463-4A3A-AAA1-44C6BC71EC66", "Procedure Description ");
			zTextBoxColumnStyleInfo6.GroupName = zGuidDropEditColumnStyleInfo.GroupName;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			zCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo.CaptionResourceString = Res.GetData("293D6ECF-609F-4442-807D-2586B19A5803", "Manual Item No.");
			zCalcEditColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.JI_ProductManualNo;
			zCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);

			zTextBoxColumnStyleInfo1.ColumnName = JobComInvoiceLine.Schema.JI_NameOfGoods;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);

			zTextBoxColumnStyleInfo2.ColumnName = JobComInvoiceLine.Schema.XC_GoodsSpecModel;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);

			zTextBoxColumnStyleInfo5.ColumnName = JobComInvoiceLine.Schema.JI_ProductVersion;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);

			zDropEditColumnStyleInfo.MaxLengthOverride = 1;
			zDropEditColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.JI_DutyMode;

			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondQuantity;
			zCalcEditColumnStyleInfo1.GroupName = Res.GetData("DA569AB4-8678-4602-890C-BDCFA8023F97", "Customs Second Quantity");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zDropEditColumnStyleInfo1.CaptionResourceString = Res.GetData("02039D8D-9B58-4C38-A5D0-F7D6D3CD12E1", "UQ");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty;
			zDropEditColumnStyleInfo1.GroupName = Res.GetData("DA569AB4-8678-4602-890C-BDCFA8023F97", "Customs Second Quantity");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);

			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.CountryList";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("B99410E0-A288-4BB0-B836-9C3D117C7C24", "Final Destination");
			zCodeFindBoxColumnStyleInfo1.ColumnName = JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport;
			zCodeFindBoxColumnStyleInfo1.ModuleID = ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			zDropEditColumnStyleInfo2.CaptionResourceString = Res.GetData("5A3F7566-9498-4248-95E7-E7A1E5F579CC", "Preference");
			zDropEditColumnStyleInfo2.ColumnName = JobComInvoiceLine.Schema.JI_PrimaryPreference;

			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = JobComInvoiceLine.Schema.JI_TradeQuantity;
			zCalcEditColumnStyleInfo2.GroupName = Res.GetData("BEA0CB1A-0CAA-4256-8DBC-39D1BA444DCA", "Trade Quantity");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zDropEditColumnStyleInfo3.BindToList = "Lookups.TradeUnitQtyList";
			zDropEditColumnStyleInfo3.CaptionResourceString = Res.GetData("D15F2E5A-89A3-4D3C-B541-77DC09D1B5A8", "UQ");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = JobComInvoiceLine.Schema.JI_TradeUnitQty;
			zDropEditColumnStyleInfo3.GroupName = Res.GetData("BEA0CB1A-0CAA-4256-8DBC-39D1BA444DCA", "Trade Quantity");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);

			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = JobComInvoiceLine.Schema.TradeUnitPrice;
			zCalcEditColumnStyleInfo4.GroupName = Res.GetData("908c16eb-9f7a-4d6c-a4b5-93f443a8003c", "Trade Unit Price");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);

			zDropEditColumnStyleInfo7.ColumnName = JobComInvoiceLine.Schema.TradeAgreementCode;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(entryLineNumberTextBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);

			AddCertificateOfOriginColumns();
		}

		void AddCertificateOfOriginColumns()
		{
			var certOfOriginGroupName = Res.GetData("C3D278AA-BA12-4E66-BF66-751BE6D2803C", "Certificate of Origin");

			var cooCountryFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			cooCountryFindBoxColumnStyleInfo.BindToList = "Lookups.CountryList";
			cooCountryFindBoxColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.CertificateOfOriginCountry;
			cooCountryFindBoxColumnStyleInfo.GroupName = certOfOriginGroupName;
			cooCountryFindBoxColumnStyleInfo.ModuleID = ZArchitecture.Modules.ModuleIDs.RefCountry;
			cooCountryFindBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			var cooTypeDropEditColumn = new ZDropEditColumnStyleInfo();
			cooTypeDropEditColumn.ColumnName = JobComInvoiceLine.Schema.CertificateOfOriginType;
			cooTypeDropEditColumn.GroupName = certOfOriginGroupName;
			cooTypeDropEditColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(85);

			var cooDropEditColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			cooDropEditColumn.ColumnName = JobComInvoiceLine.Schema.CertificateOfOrigin;
			cooDropEditColumn.GroupName = certOfOriginGroupName;
			cooDropEditColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(85);

			var cooItemNumberCalcEditColumn = new ZArchitecture.ZCalcEditColumnStyleInfo();
			cooItemNumberCalcEditColumn.BindToDecimalPlaces = null;
			cooItemNumberCalcEditColumn.ColumnName = JobComInvoiceLine.Schema.ItemNoOnCertOfOrigin;
			cooItemNumberCalcEditColumn.GroupName = certOfOriginGroupName;
			cooItemNumberCalcEditColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(cooCountryFindBoxColumnStyleInfo);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(cooTypeDropEditColumn);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(cooDropEditColumn);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(cooItemNumberCalcEditColumn);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			AddNewColumnForCustomsInvoiceLinesBoundGrid();
			ResetCustomsInvoiceLinesBoundGridColumnsOrder();

			if (CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_Tariff) is Universal.GUI.TariffColumnStyleInfo tariff)
			{
				tariff.PartialDescriptionMinLengthForSearch = 2;
				tariff.ShowDescriptionFilterOnNonNomenclatureTariffModule = true;
			}
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			var declaration = JobDeclaration as JobDeclaration;
			if (declaration != null)
			{
				var isEntering = declaration.WillGenerateEnteringEntry;
				var isExiting = declaration.WillGenerateExitingEntry;
				var isImport = declaration.IsImport;

				DestDistrictCodeFindBox.Visible = isEntering;
				DestRegionCodeFindBox.Visible = isEntering;
				OriginDestrictCodeFindBox.Visible = isExiting;
				OriginRegionCodeFindBox.Visible = isExiting;
				JI_StateOrRegionOfOriginDropEdit.Visible = isEntering;
				JI_CIQOriginStateCodeFindBox.Visible = isEntering;
				PrimaryPreferenceDropEdit.Visible = isImport;

				if (!declaration.IsPersistent)
				{
					EntryInstructionDropEdit.Visible = false;
					EntryInstructionManualNoTextBox.Visible = false;
					ManualItemNoCalcEdit.Visible = false;
					ChildInstructionDropEdit.Visible = false;
					ChildInstructionManualNoTextBox.Visible = false;
					ManualItemNo2CalcEdit.Visible = false;
				}

				CIQProductQuantificationsUserControl.ChangeControlsVisibility(isImport);
				ResetInstructionGroupBoxesCaption();

				using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					CustomsInvoiceLinesBoundGrid.SetAvailability(isImport, ImpOnlyColumns);
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			BindCertificateOfOriginControlsVisible(dataSource);
		}

		void BindCertificateOfOriginControlsVisible(object dataSource)
		{
			var certificateOfOriginControls = new List<Control>() { CertificateOfOriginCountryCodeFindBox, CertificateOfOriginTextBox, CertificateOfOriginTypeDropEdit, TradeAgreementCodeDropEdit, ItemNoOnCertOfOriginCalcEdit };
			foreach (var control in certificateOfOriginControls)
			{
				control.DataBindings.RemoveBinding(IsVisibleForBindingConst);
				if (dataSource != null)
				{
					control.DataBindings.Add(new KBinding(IsVisibleForBindingConst, BindingSource.DataSource, nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.IsCertificateOfOriginApplicable), false, DataSourceUpdateMode.Never));
				}
			}
		}
		const string IsVisibleForBindingConst = "IsVisibleForBinding";

		#region Column Names: Order, Defaults and Unavailables

		string[] UnavailableColumnsOnCommercialInvoice => fUnavailableColumnsForCommercialInvoice ?? (fUnavailableColumnsForCommercialInvoice = new string[]
		{
			"CusEntryLine+EntryLRNAndEntryLineNo",
			BaseJobComInvoiceLine.Schema.JI_CEI,
			JobComInvoiceLine.Schema.JI_CEI_StyleDescription,
			JobComInvoiceLine.Schema.JI_CEI_Description
		});
		string[] fUnavailableColumnsForCommercialInvoice;

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (fcolumnNamesInSortOrder == null)
				{
					var columns = new List<string>();
					columns.AddRange(DefaultColumnsForGrid);

					columns.Add(BaseJobComInvoiceLine.Schema.JI_CustomAttrib1);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_CustomAttrib2);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_CustomAttrib3);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_CustomAttrib4);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_CustomAttrib5);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_CustomAttrib6);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_CustomTextBlob1);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_Description);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_PartAttrib1);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_PartAttrib2);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_PartAttrib3);
					columns.Add(BaseJobComInvoiceLine.Schema.JI_SerialNumber);
					fcolumnNamesInSortOrder = columns.ToArray();
				}
				return fcolumnNamesInSortOrder.ToArray();
			}
		}
		string[] fcolumnNamesInSortOrder;

		string[] DefaultColumnsForGrid => fdefaultColumnsForGrid ?? (fdefaultColumnsForGrid = new[]
		{
			"CusEntryLine+EntryLRNAndEntryLineNo",
			BaseJobComInvoiceLine.Schema.JI_LineNo,
			BaseJobComInvoiceLine.Schema.JI_Calc_Invoice,
			BaseJobComInvoiceLine.Schema.JI_CEI,
			JobComInvoiceLine.Schema.JI_CEI_StyleDescription,
			JobComInvoiceLine.Schema.JI_CEI_Description,
			BaseJobComInvoiceLine.Schema.JI_PartNo,
			JobComInvoiceLine.Schema.JI_ProductManualNo,
			BaseJobComInvoiceLine.Schema.JI_Tariff,
			JobComInvoiceLine.Schema.JI_NameOfGoods,
			BaseJobComInvoiceLine.Schema.JI_InvoiceQuantity,
			BaseJobComInvoiceLine.Schema.JI_InvoiceUQ,
			BaseJobComInvoiceLine.Schema.UnitPrice,
			JobComInvoiceLine.Schema.JI_TradeQuantity,
			JobComInvoiceLine.Schema.JI_TradeUnitQty,
			JobComInvoiceLine.Schema.TradeUnitPrice,
			BaseJobComInvoiceLine.Schema.JI_LinePrice,
			BaseJobComInvoiceLine.Schema.JI_RX_NKLinePriceCurr,
			BaseJobComInvoiceLine.Schema.JI_CustomsQuantity,
			BaseJobComInvoiceLine.Schema.JI_CustomsUnitQty,
			JobComInvoiceLine.Schema.JI_ProductVersion,
			BaseJobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
			JobComInvoiceLine.Schema.JI_DutyMode,
			JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
			JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
			BaseJobComInvoiceLine.Schema.JI_CountryOfOrigin,
			JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
			JobComInvoiceLine.Schema.JI_PrimaryPreference,
			JobComInvoiceLine.Schema.CertificateOfOriginType,
			JobComInvoiceLine.Schema.CertificateOfOrigin,
			JobComInvoiceLine.Schema.ItemNoOnCertOfOrigin,
			JobComInvoiceLine.Schema.CertificateOfOriginCountry,
			JobComInvoiceLine.Schema.TradeAgreementCode,
			BaseJobComInvoiceLine.Schema.JI_Weight,
			BaseJobComInvoiceLine.Schema.JI_WeightUQ,
			BaseJobComInvoiceLine.Schema.JI_NetWeight,
			BaseJobComInvoiceLine.Schema.JI_NetWeightUQ,
		});
		string[] fdefaultColumnsForGrid;

		string[] UnavailableColumns => fUnavailableColumns ?? (fUnavailableColumns = new[]
		{
			BaseJobComInvoiceLine.Schema.JI_Volume,
			BaseJobComInvoiceLine.Schema.JI_VolumeUQ,
			BaseJobComInvoiceLine.Schema.JI_OrderNumber,
			BaseJobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
			BaseJobComInvoiceLine.Schema.JI_CC
		});
		string[] fUnavailableColumns;

		string[] ImpOnlyColumns => fImpOnlyColumns ?? (fImpOnlyColumns = new[]
		{
			JobComInvoiceLine.Schema.JI_PrimaryPreference,
		});
		string[] fImpOnlyColumns;

		#endregion

		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.China;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.China;

		#region Extract From GeneralCountryInvoiceLineUserControl

		protected override void HookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.HookInvoiceLineEvents(invoiceLine);

			if (invoiceLine != null)
			{
				invoiceLine.JI_CEIInfo.ValueChanged -= JI_CEI_ValueChanged;
				invoiceLine.JI_CEIInfo.ValueChanged += JI_CEI_ValueChanged;
				JI_CEI_ValueChanged(null, null);
			}
		}

		protected override void UnHookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.UnHookInvoiceLineEvents(invoiceLine);

			if (invoiceLine != null)
			{
				invoiceLine.JI_CEIInfo.ValueChanged -= JI_CEI_ValueChanged;
			}
		}

		#endregion

		void JI_CEI_ValueChanged(object sender, EventArgs e)
		{
			CustomsInvoiceLinesBoundGrid.ListManager?.Refresh();
		}

		protected new JobComInvoiceLine CurrentInvoiceLine => base.CurrentInvoiceLine as JobComInvoiceLine;

		void ResetInstructionGroupBoxesCaption()
		{
			var declaration = JobDeclaration as JobDeclaration;
			ParentEntryInstructionGroupBox.Text = Business.CusEntryInstruction.GetEntryTypeDescription(declaration, false);
			ChildEntryInstructionGroupBox.Text = Business.CusEntryInstruction.GetEntryTypeDescription(declaration, true);
			ChildEntryInstructionGroupBox.Visible = declaration?.WillGenerateBothEntries ?? false;
		}

		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl()
		{
			return new InvoiceLineChargesUserControl();
		}

		void XC_GoodsSpecModelButton_Click(object sender, EventArgs e)
		{
			var invoiceLine = CurrentInvoiceLine;
			if (invoiceLine != null)
			{
				AdditionalInformationForm.ShowDialog(invoiceLine, invoiceLine.JI_NameOfGoodsInfo, invoiceLine.XC_GoodsSpecModelInfo, invoiceLine.IsEnteringOrExiting());
			}
		}

		void XC_GoodsSpecModel2Button_Click(object sender, EventArgs e)
		{
			var invoiceLine = CurrentInvoiceLine;
			if (invoiceLine != null)
			{
				AdditionalInformationForm.ShowDialog(invoiceLine, invoiceLine.JI_NameOfGoods2Info, invoiceLine.XC_GoodsSpecModel2Info, invoiceLine.IsEnteringOrExiting(true));
			}
		}
	}
}
