using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using InvoiceCharge = Enterprise.Customs.EU.Business.Declaration.InvoiceCharge;

namespace Enterprise.Customs.IE.GUI
{
	public partial class ExportInvoiceLineUserControl : EU.GUI.EUExportInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();
			ReorderTabPages();
		}

		protected override ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.AdditionalDocuments;

		protected override ResourceStringData GetPreviousDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.PreviousDocuments;

		protected override ResourceStringData GetSupportingDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.SupportingDocuments;

		protected override ZBool DynamicLayoutApplied => ZBool.True;

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();

		protected override Type GetPreviousDocumentsUserControlType() => typeof(LayoutPreviousDocumentsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControlWithGrid);

		protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLineLayoutSupportingDocumentsUserControl);

		protected override Type GetOrganizationsUserControlType() => typeof(InvoiceLineOrganizationsUserControl);

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			ChargesGridsColumnVisibility();
			ChargesGridsColumn_Description();
			ContainersGridColumnVisibility();
		}

		void ReorderTabPages()
		{
			LineDetailTabControl.TabPages.Remove(NewLineDetailsTabPage);
			LineDetailTabControl.TabPages.Insert(NewLineDetailsTabPage, 0);

			LineDetailTabControl.TabPages.Remove(OrganizationsTabPage);
			LineDetailTabControl.TabPages.Insert(OrganizationsTabPage, 1);

			LineDetailTabControl.TabPages.Remove(AdditionalInfosTabPage);
			LineDetailTabControl.TabPages.Insert(AdditionalInfosTabPage, 11);
		}

		protected override void AddColumnsToGrid()
		{
			base.AddColumnsToGrid();
			var countryOfExportColumnStyle = new ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
				IsVisible = true,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(108),
			};
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(countryOfExportColumnStyle);

			var countryOfOriginColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CountryOfOrigin);
			int countryOfOriginIndex = CustomsInvoiceLinesBoundGrid.ColumnStyles.IndexOf(countryOfOriginColumnStyle);
			var newCountryOfOriginColumnStyle = new ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_CountryOfOrigin,
				IsVisible = true,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(76)
			};
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(countryOfOriginColumnStyle);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Insert(countryOfOriginIndex, newCountryOfOriginColumnStyle);
			var isMainPackColumnStyle = new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.ZG_IsMainPack,
				IsVisible = true,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(108),
			};
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(isMainPackColumnStyle);

			var netWeightInKGCaption = Res.GetData("94F93DA9-B6C4-4233-B3E2-13CAC7493564", "Net Weight in KG");
			var customsQtyColumn = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity);
			if (customsQtyColumn != null)
			{
				customsQtyColumn.GroupName = netWeightInKGCaption;
			}
			var customsUnitQtyColumn = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsUnitQty);
			if (customsUnitQtyColumn != null)
			{
				customsUnitQtyColumn.GroupName = netWeightInKGCaption;
			}
		}

		void ChargesGridsColumnVisibility()
		{
			var columnsToHide = new[] { InvoiceCharge.Schema.J7_IsDutiable, InvoiceCharge.Schema.J7_IsGSTApplicable };
			InvoiceLineCharges.ChargesGrid.SetAvailability(false, columnsToHide);
			InvoiceLineCharges.ApportionedChargesGrid.SetAvailability(false, columnsToHide);
		}

		void ChargesGridsColumn_Description()
		{
			var descriptonColumnName = InvoiceCharge.Schema.ChargeCodeDescription;
			var columnStyles = new[] { InvoiceLineCharges.ChargesGrid.GetColumnStyle(descriptonColumnName), InvoiceLineCharges.ApportionedChargesGrid.GetColumnStyle(descriptonColumnName) };

			foreach (var columnStyle in columnStyles)
			{
				columnStyle.Caption = Res.GetString("69C08D26-CC71-4B0A-B0E6-070A469C2AEF", "Description");
				columnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(138);
			}
		}

		void ContainersGridColumnVisibility()
		{
			var columnToHide = NonPersistentCusContainer.Schema.Mode;
			CusContainerInvoiceLineGrid.SetAvailability(false, columnToHide);
		}

		protected override string[] GetDefaultColumnsForGrid()
		{
			if (defaultColumnsForGrid == null)
			{
				var result = new List<string>(base.GetDefaultColumnsForGrid());
				var countryOfOriginIndex = result.IndexOf(JobComInvoiceLine.Schema.JI_CountryOfOrigin);
				result.Insert(countryOfOriginIndex + 1, JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport);
				result.Add(JobComInvoiceLine.Schema.ZG_IsMainPack);
				defaultColumnsForGrid = result.ToArray();
			}
			return defaultColumnsForGrid;
		}
		string[] defaultColumnsForGrid;
	}
}
