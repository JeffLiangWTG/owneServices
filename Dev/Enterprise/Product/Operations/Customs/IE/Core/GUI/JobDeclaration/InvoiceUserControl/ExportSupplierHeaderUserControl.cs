using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using InvoiceCharge = Enterprise.Customs.EU.Business.Declaration.InvoiceCharge;

namespace Enterprise.Customs.IE.GUI
{
	public partial class ExportSupplierHeaderUserControl : EUExportSupplierHeaderUserControl
	{
		public ExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			ReorderTabPages();
		}

		protected override ResourceStringData GetAdditionalInfoTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) => EU.GUI.CaptionProvider.AdditionalDocuments;

		protected override ResourceStringData GetSupportingDocumentsTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) => CaptionProvider.SupportingDocuments;

		protected override ResourceStringData GetPreviousDocumentsTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) => Res.GetData("7DF1D631-5B8F-4E35-A4D8-3594F8A57B2F", "Previous Docs");

		protected override Type GetPreviousDocumentsUserControlType() => typeof(InvoiceHeaderExportPreviousDocumentsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControlWithGrid);

		protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLayoutSupportingDocumentsUserControl);

		void ReorderTabPages()
		{
			InvoiceTabControl.TabPages.Remove(AdditionalInfoTabPage);
			InvoiceTabControl.TabPages.Insert(AdditionalInfoTabPage, 6);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			RemoveColumns();
			RenameColumns();
			AddColumns();
			ChargesGridsColumnVisibility();
		}

		void RemoveColumns()
		{
			var columnsToRemove = new[]
			{
				JobComInvoiceHeader.Schema.JZ_OH_Supplier,
				JobComInvoiceHeader.Schema.ConsigneeOrgPK,
				JobComInvoiceHeader.Schema.ExporterOrgPK,
				JobComInvoiceHeader.Schema.JZ_OH_Buyer,
				JobComInvoiceHeader.Schema.IntermediateConsigneeOrgPK,
				JobComInvoiceHeader.Schema.InvoicerOrgPK,
				JobComInvoiceHeader.Schema.SellerOrgPK,
				JobComInvoiceHeader.Schema.SellingAgentOrgPK,
				JobComInvoiceHeader.Schema.ShipToPartyOrgPK,
				JobComInvoiceHeader.Schema.SoldToPartyOrgPK,
				JobComInvoiceHeader.Schema.SupplierName,
				JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount,
				JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency
			};

			var columnStyles = JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>();
			var columnsIncludingGroupMembers = new List<ZGridColumnInfo>();

			foreach (var columnName in columnsToRemove)
			{
				var columnStyle = columnStyles.SingleOrDefault(x => x.ColumnName == columnName);
				if (columnStyle != null)
				{
					if (!columnStyle.GroupName.IsEmpty())
					{
						columnsIncludingGroupMembers.AddRange(columnStyles.Where(x => x.GroupName.ToString() == columnStyle.GroupName.ToString()));
					}
					else
					{
						columnsIncludingGroupMembers.Add(columnStyle);
					}
				}
			}

			foreach (var columnToRemove in columnsIncludingGroupMembers)
			{
				JobComInvoiceHeadersBoundGrid.ColumnStyles.Remove(columnToRemove);
			}
		}

		void RenameColumns()
		{
			var columnStyles = JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>();
			var columnRenamingInfos = new (string columnName, ResourceStringData newColumnName, ResourceStringData newGroupName, int width)[]
			{
				(JobComInvoiceHeader.Schema.BuyerOrgPK, Res.GetData("CFB18086-F6B5-4E50-A1D1-8D57398A666A", "Buyer/Consignee"), Res.GetData("CFB18086-F6B5-4E50-A1D1-8D57398A666A", "Buyer/Consignee"), ControlDpiScalingHelper.ScaleToCurrentDpiX(110)),
				(JobComInvoiceHeader.Schema.JZ_OA_BuyerAddress, Res.GetData("423A4FF5-EFD0-4C60-A6A1-042DD398102F", "Buyer/Consignee Address"), Res.GetData("CFB18086-F6B5-4E50-A1D1-8D57398A666A", "Buyer/Consignee"), ControlDpiScalingHelper.ScaleToCurrentDpiX(150)),
				(JobComInvoiceHeader.Schema.SupplierOrgPK, Res.GetData("D4A4B8FB-0237-428C-ADFB-98F3CE7A108F", "Supplier/Consignor"), Res.GetData("D4A4B8FB-0237-428C-ADFB-98F3CE7A108F", "Supplier/Consignor"), ControlDpiScalingHelper.ScaleToCurrentDpiX(110)),
				(JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress, Res.GetData("7CC10B34-6B12-4BF7-B980-319330DBC533", "Supplier/Consignor Address"), Res.GetData("D4A4B8FB-0237-428C-ADFB-98F3CE7A108F", "Supplier/Consignor"), ControlDpiScalingHelper.ScaleToCurrentDpiX(150)),
				(JobComInvoiceHeader.Schema.JZ_IncoTermPlace, Res.GetData("D47C0837-000D-4D33-AB0B-1541B39F9B6C", "Incoterm Location"), Res.GetData("EA4D0BAF-3096-4B9A-BC49-AD33CD21096F", "Incoterm Location"), ControlDpiScalingHelper.ScaleToCurrentDpiX(110)),
			};

			foreach (var renamingInfo in columnRenamingInfos)
			{
				var columnStyle = columnStyles.SingleOrDefault(x => x.ColumnName == renamingInfo.columnName);
				if (columnStyle != null)
				{
					columnStyle.CaptionResourceString = renamingInfo.newColumnName;
					columnStyle.GroupName = renamingInfo.newGroupName;
					columnStyle.Width = renamingInfo.width;
				}
			}
		}

		void AddColumns()
		{
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceHeader.Schema.JZ_UCR,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});
		}

		void ChargesGridsColumnVisibility()
		{
			base.ChangeControlsVisibility();
			InvoiceChargesGrid.SetAvailability(false, InvoiceCharge.Schema.J7_IsDutiable);
			BaseGroupChargesGrid.SetAvailability(false, InvoiceCharge.Schema.J7_IsDutiable);
			ApportionedChargesGrid.SetAvailability(false, InvoiceCharge.Schema.J7_IsDutiable);
		}
	}
}
