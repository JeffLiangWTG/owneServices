using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAImportSupplierHeaderUserControl : CACustomsSupplierHeaderUserControl
	{
		public CAImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InitializeNonInheritedColumns();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			InvoiceHeadersBoundGrid.RemoveFromAvailableColumns(JobComInvoiceHeaderSchema.JZ_OH_Buyer.Name);
			var supplierColumnStyle = InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeaderSchema.JZ_OH_Supplier.Name);
			supplierColumnStyle.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|90E86B46-76ED-4d4c-B5BD-577F9573BD2C", "Vendor");
			supplierColumnStyle.ColumnName = "SupplierDocumentaryAddress+OrganisationPK";
			supplierColumnStyle.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|fd1fd0cf-c308-467f-bd5b-3aa0387a358f", "Vendor");
			this.JZ_CIFAmountBoundCurrencyControl.Visible = false;
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutTr/Cac4tViR0a1W3XKS7zQ==";

			ApportionmentPendingLabel.AllowOutsideOfParent();
			RightBottomPanel.AllowOutsideOfParent();
		}

		protected override void InitLayout()
		{
			base.InitLayout();
			InitializeGridLayout();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			declarationValueChangedAnnouncer_OnValueChanged(this, null);
			declarationValueChangedAnnouncer = (CurrentDataItem as Customs.Business.IInvoicesProvider)?.GetValueChangedAnnouncer();
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged += new EventHandler(declarationValueChangedAnnouncer_OnValueChanged);
			}
		}

		Customs.Business.IInvoicesProviderValueChangedAnnouncer declarationValueChangedAnnouncer;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged -= declarationValueChangedAnnouncer_OnValueChanged;
				declarationValueChangedAnnouncer.Dispose();
			}
		}

		void declarationValueChangedAnnouncer_OnValueChanged(object sender, EventArgs e)
		{
			var declaration = CurrentDataItem as JobDeclaration;
			this.CargoControlNumbersTabPage.TabVisible = declaration != null && declaration.IsIID;
			this.PackagesPivotTabPage.TabVisible = declaration != null && declaration.IsIID;
		}

		#region InitializeGridLayout

		void InitializeGridLayout()
		{
			using (InvoiceHeadersBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				InvoiceHeadersBoundGrid.SetAllColumnsVisible(false);
				InvoiceHeadersBoundGrid.SetColumnVisible(true, InvoiceDefaultColumnsSequence);
				InvoiceHeadersBoundGrid.ReOrderColumns(InvoiceDefaultColumnsSequence);
				InvoiceHeadersBoundGrid.AfterBind += new EventHandler(InvoiceHeadersBoundGrid_AfterBind);
			}
		}

		string[] InvoiceDefaultColumnsSequence
		{
			get
			{
				if (invoiceDefaultColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						JobComInvoiceHeader.Schema.JZ_InvoiceNumber,
						"SupplierDocumentaryAddress+OrganisationPK",
						"SupplierDocumentaryAddress+E2_OA_Address",
						JobComInvoiceHeader.Schema.JZ_IncoTerm,
						JobComInvoiceHeader.Schema.JZ_IncoTermPlace,
						JobComInvoiceHeader.Schema.JZ_InvoiceAmount,
						JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency,
						JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate,
						JobComInvoiceHeader.Schema.InvoiceLineTotal,
						JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
						JobComInvoiceHeader.Schema.CA_RL_NKLastPort,
						JobComInvoiceHeader.Schema.JZ_ValuationDateOverride,
						JobComInvoiceHeader.Schema.CA_TreatmentCode,
						JobComInvoiceHeader.Schema.CA_ValueForDutyCode,
						JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin,
						JobComInvoiceHeader.Schema.JZ_RW_NKOriginState,
						JobComInvoiceHeader.Schema.CA_RN_NKExport,
						JobComInvoiceHeader.Schema.CA_USStateOfExport,
						JobComInvoiceHeader.Schema.CA_TradeZone,
						JobComInvoiceHeader.Schema.CA_USPortOfExit,
						JobComInvoiceHeader.Schema.JZ_PaymentDate,
						JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill,
						JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice,
					};
					invoiceDefaultColumnsSequence = columnList.ToArray();
				}
				return invoiceDefaultColumnsSequence;
			}
		}

		string[] invoiceDefaultColumnsSequence;

		void InitializeNonInheritedColumns()
		{
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|4c2f18d6-8bf4-4849-92ef-bfe321e40f9c", "Dir. Ship. Date", "Direct Ship. Date", "Direct Shipment Date", "The Date of Direct Shipment of this invoice. Defaults to the Date of Export (ATD) on the declaration tab.");
			zDateEditColumnStyleInfo1.ColumnName = "JZ_ValuationDateOverride";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo1, 70, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);

			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|CA7C7493-AA51-4BC1-AC8F-3FF10D71CD31", "Dir. Ship. Place", "Direct Ship. Place", "Place of Direct Shipment", "The Port code for the place of direct shipment.");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CA_RL_NKLastPort";
			ControlDpiScalingHelper.SetWidth(ref zCodeFindBoxColumnStyleInfo1, 50, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);

			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo1.ColumnName = "CA_TreatmentCode";
			ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo1, 30, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);

			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo2.ColumnName = "CA_ValueForDutyCode";
			ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo2, 30, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);

			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|6FA8C845-0167-4DFC-B8A0-BB68D9BB959A", "ORG", "Ctry/Rgn. Of Origin", "Default Country/Region of Origin", "The country/region of origin of the goods.");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JZ_RN_NKDefaultOrigin";
			ControlDpiScalingHelper.SetWidth(ref zCodeFindBoxColumnStyleInfo2, 30, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);

			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo3.ColumnName = "JZ_RW_NKOriginState";
			ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo3, 50, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);

			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			zCodeFindBoxColumnStyleInfo3.ColumnName = "CA_RN_NKExport";
			ControlDpiScalingHelper.SetWidth(ref zCodeFindBoxColumnStyleInfo3, 30, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);

			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo4.ColumnName = "CA_USStateOfExport";
			ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo4, 50, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);

			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			zCodeFindBoxColumnStyleInfo4.ColumnName = "CA_TradeZone";
			ControlDpiScalingHelper.SetWidth(ref zCodeFindBoxColumnStyleInfo4, 40, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);

			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new ZCodeFindBoxColumnStyleInfo();
			zCodeFindBoxColumnStyleInfo5.ColumnName = "CA_USPortOfExit";
			ControlDpiScalingHelper.SetWidth(ref zCodeFindBoxColumnStyleInfo5, 40, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);

			ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			zAddressDropEditColumnStyleInfo1.ColumnName = "SupplierDocumentaryAddress.E2_OA_Address";
			zAddressDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|3c6c7d2f-2eec-4594-ab10-d9916ce91485", "Vendor Address");
			zAddressDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|fd1fd0cf-c308-467f-bd5b-3aa0387a358f", "Vendor");
			ControlDpiScalingHelper.SetWidth(ref zAddressDropEditColumnStyleInfo1, 80, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);

			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|54998F13-1FB2-4D7E-BF34-23F3057E0C6E", "Consignee");
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.ImporterList";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "FinalConsigneeAddress+OrganisationPK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo1.ToolTip = "Invoice\'s Consignee";
			zGuidFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|AADECF8F-6F59-473F-B0C1-6FA2BB7F2CCA", "Consignee");
			ControlDpiScalingHelper.SetWidth(ref zGuidFindBoxColumnStyleInfo1, 80, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);

			ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			zAddressDropEditColumnStyleInfo2.ColumnName = "FinalConsigneeAddress.E2_OA_Address";
			zAddressDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|313FF06F-0ADD-4BC1-B554-79719918D1DE", "Consignee Address");
			zAddressDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("InvoiceModuleButtonGrid|AADECF8F-6F59-473F-B0C1-6FA2BB7F2CCA", "Consignee");
			ControlDpiScalingHelper.SetWidth(ref zAddressDropEditColumnStyleInfo2, 80, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);

			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new ZCodeFindBoxColumnStyleInfo();
			zCodeFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportSupplierHeaderUserControl|2df4cb06-c83f-4ea1-a7e7-d5c906a5d092", "Ctry/Rgn.", "Tranship. Ctry/Rgn.", "Transhipment Country/Region", "Country/Region through which the goods were shipped in transit to Canada under CBSA control.");
			zCodeFindBoxColumnStyleInfo6.ColumnName = "CA_RN_NKTranshipment";
			ControlDpiScalingHelper.SetWidth(ref zCodeFindBoxColumnStyleInfo6, 110, true);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);

			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportSupplierHeaderUserControl|713c9c6e-bae5-4ac7-b1dd-0baa11fe483b", "Region", "Region of Origin");
			zTextBoxColumnStyleInfo1.ColumnName = "CA_IIDRegion";
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		}

		#endregion
		#region Change DDPDeductDutyOnlyDropEdit visibility
		void InvoiceHeadersBoundGrid_AfterBind(object sender, EventArgs e)
		{
			InvoiceHeadersBoundGrid.ListManager.CurrentChanged += InvoiceHeadersBoundGrid_CurrentChanged;
			ChangeDDPDeductDutyOnlyVisibility();
		}

		void JZ_IncoTermInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeDDPDeductDutyOnlyVisibilityCore();
		}

		void InvoiceHeadersBoundGrid_CurrentChanged(object sender, EventArgs e)
		{
			ChangeDDPDeductDutyOnlyVisibility();
		}

		void ChangeDDPDeductDutyOnlyVisibility()
		{
			UnhookInvoiceHeaderEvents();
			currentHeader = null;
			var listManager = InvoiceHeadersBoundGrid.ListManager;
			if (listManager != null && listManager.Count > 0)
			{
				currentHeader = (JobComInvoiceHeader)listManager.GetCurrent();
				ChangeDDPDeductDutyOnlyVisibilityCore();
			}
			HookInvoiceHeaderEvents();
		}

		void HookInvoiceHeaderEvents()
		{
			if (currentHeader != null)
			{
				currentHeader.JZ_IncoTermInfo.ValueChanged += JZ_IncoTermInfo_ValueChanged;
			}
		}

		void UnhookInvoiceHeaderEvents()
		{
			if (currentHeader != null)
			{
				currentHeader.JZ_IncoTermInfo.ValueChanged -= JZ_IncoTermInfo_ValueChanged;
			}
		}

		JobComInvoiceHeader currentHeader;

		void ChangeDDPDeductDutyOnlyVisibilityCore()
		{
			if (currentHeader != null)
			{
				DDPDeductDutyOnlyCheckBox.Visible = currentHeader.IsDDPDeductDutyOnlyRequired;
			}
		}
		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (currentHeader != null)
				{
					currentHeader.JZ_IncoTermInfo.ValueChanged -= JZ_IncoTermInfo_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}
	}
}
