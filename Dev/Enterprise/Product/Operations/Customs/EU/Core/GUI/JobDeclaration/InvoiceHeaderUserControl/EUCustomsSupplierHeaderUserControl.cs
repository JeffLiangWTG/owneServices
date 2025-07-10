using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EUCustomsSupplierHeaderUserControl : Customs.GUI.LayoutDeclarationInvoiceHeaderUserControl, ISupportMultipleResourceStringDataSupporter, ISupportMultipleResourceStringData
	{
		public EUCustomsSupplierHeaderUserControl()
		{
			InitializeComponent();
			InitializeComponentLostByDesignMode();
			InvoicePaymentTabPage.RunWhenBindingOrFirstShown((s, args) => InitInvoicePaymentUserControl());
			AdditionalInfoTabPage.RunWhenBindingOrFirstShown((s, args) => InitAdditionalInfosUserControl());
			PreviousDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitPreviousDocumentsUserControl());
			SupportingDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitSupportingDocumentsUserControl());
			ValueIndicatorsTabPage.RunWhenBindingOrFirstShown((s, args) => InitValueIndicatorsUserControl());
			InvoiceHeadersBoundGrid.AfterBind += InvoiceHeadersBoundGrid_AfterBind;
		}
		protected const string SupportingInfoColumnLayoutContext = "INV";

		public override BaseJobDeclaration JobDeclaration
		{
			get => base.JobDeclaration;
			set
			{
				var oldValue = JobDeclaration as JobDeclaration;
				base.JobDeclaration = value;
				var newValue = JobDeclaration as JobDeclaration;
				if (oldValue != newValue)
				{
					SetValueIndicatorsTabPageCaptionResourceString(newValue);
					SetAdditionalInfoTabPageCaptionResourceString(newValue);
					SetSupportingDocumentsTabPageCaptionResourceString(newValue);
					SetPreviousDocumentsTabPageCaptionResourceString(newValue);
				}
			}
		}

		void SetValueIndicatorsTabPageCaptionResourceString(JobDeclaration declaration)
		{
			ValueIndicatorsTabPage.CaptionResourceString = GetValueIndicatorsTabPageCaption(declaration);
		}

		protected virtual ResourceStringData GetValueIndicatorsTabPageCaption(JobDeclaration declaration) => Res.GetData("EUCustomsSupplierHeaderUserControl|D6873B3A-8042-4709-BF71-436C74AFF303", "[UCC 4/13] Valuation Indicators");

		void SetAdditionalInfoTabPageCaptionResourceString(JobDeclaration declaration)
		{
			AdditionalInfoTabPage.CaptionResourceString = GetAdditionalInfoTabPageCaption(declaration);
		}

		protected virtual ResourceStringData GetAdditionalInfoTabPageCaption(JobDeclaration declaration) => declaration?.IsUCC6 ?? false ? CaptionProvider.AdditionalDocuments : CaptionProvider.AdditionalInfo_44;

		void SetSupportingDocumentsTabPageCaptionResourceString(JobDeclaration declaration)
		{
			SupportingDocumentsTabPage.CaptionResourceString = GetSupportingDocumentsTabPageCaption(declaration);
		}

		protected virtual ResourceStringData GetSupportingDocumentsTabPageCaption(JobDeclaration declaration) => CaptionProvider.SupportingDocuments_44;

		void SetPreviousDocumentsTabPageCaptionResourceString(JobDeclaration declaration)
		{
			PreviousDocumentsTabPage.CaptionResourceString = GetPreviousDocumentsTabPageCaption(declaration);
		}

		protected virtual ResourceStringData GetPreviousDocumentsTabPageCaption(JobDeclaration declaration) => Res.GetData("EUCustomsSupplierHeaderUserControl|57b7ad6a-94a9-4354-84b6-2e16cb9ea9c4", "[40] Previous Docs");

		void InitializeComponentLostByDesignMode()
		{
			BindingSource.SetBindingMember(invoicePaymentUserControl1, nameof(JobDeclaration.Invoices));
		}

		protected void InitializeComponentLostByDesignModeCore() { }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				InitTabsVisibility();
			}
		}

		void InitTabsVisibility()
		{
			var currentDataItem = CurrentDataItem;
			var configuration = currentDataItem?.Configuration.InvoiceHeaderConfiguration;
			if (configuration != null)
			{
				SupportingDocumentsTabPage.TabVisible = configuration.SupportingDocumentsSupport(currentDataItem);
				InvoicePaymentTabPage.TabVisible = configuration.InvoicePaymentSupport(currentDataItem);
				AdditionalInfoTabPage.TabVisible = configuration.AdditionalInfosSupport(currentDataItem);
				PreviousDocumentsTabPage.TabVisible = configuration.PreviousDocumentsSupport(currentDataItem);
				SetValueIndicatorsVisibility();
			}

			void SetValueIndicatorsVisibility()
			{
				var valueIndicatorsEnabled = configuration.ValueIndicatorsSupport(CurrentDataItem);
				ValueIndicatorsTabPage.TabVisible = valueIndicatorsEnabled;
				var valueIndicatorsColumnNames = InvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.GroupName.Equals(ValueIndicatorsGroupName)).Select(x => x.ColumnName).ToArray();
				InvoiceHeadersBoundGrid.SetAvailability(valueIndicatorsEnabled, valueIndicatorsColumnNames);
			}
		}

		public new JobDeclaration CurrentDataItem => (JobDeclaration)base.CurrentDataItem;

		protected virtual bool ShouldShowVatGstColumnInChargesGrid => true;

		protected virtual bool ShouldShowCalculateDDPButton => false;

		protected override void UnHookDeclarationEventsCore(BaseJobDeclaration declaration)
		{
			base.UnHookDeclarationEventsCore(declaration);
			declaration.JE_TransportModeInfo.ValueChanged -= JE_TransportModeInfo_ValueChanged;
		}

		protected override void HookDeclarationEventsCore(BaseJobDeclaration declaration)
		{
			declaration.JE_TransportModeInfo.ValueChanged += JE_TransportModeInfo_ValueChanged;
			base.HookDeclarationEventsCore(declaration);
		}

		protected override void UpdateControlsVisiblityWhenJobDeclarationChanged()
		{
			base.UpdateControlsVisiblityWhenJobDeclarationChanged();
			UpdateCalculateChargeButtonPanelsVisibility();
		}

		void JE_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateCalculateChargeButtonPanelsVisibility();
		}

		void UpdateCalculateChargeButtonPanelsVisibility()
		{
			var calculateFreightEnabled = false;
			var calculateInsuranceEnabled = false;
			var calculateDDPEnabled = false;
			var jobDeclaration = JobDeclaration as JobDeclaration;
			if (jobDeclaration != null)
			{
				calculateInsuranceEnabled = jobDeclaration.SupportsCalculateInsurance;
				calculateFreightEnabled = IsCalculateFreightEnabled(jobDeclaration);
				calculateDDPEnabled = ShouldShowCalculateDDPButton;
			}
			InvoiceChargesButtonPanel.Visible = calculateFreightEnabled || calculateInsuranceEnabled || calculateDDPEnabled;
			GroupChargesButton.Visible = calculateFreightEnabled;
			UpdateInvoiceChargeButtonsVisibility(jobDeclaration);
		}

		protected virtual bool IsCalculateFreightEnabled(BaseJobDeclaration jobDeclaration)
		{
			return jobDeclaration.IsAir || (jobDeclaration.IsImport && CalculateFreightNonAirTransportModes.Contains(jobDeclaration.JE_TransportMode));
		}

		ImmutableHashSet<ZString> CalculateFreightNonAirTransportModes => calculateFreightNonAirTransportModes ?? (calculateFreightNonAirTransportModes = ImmutableHashSet.Create<ZString>(
			Core.Constants.TransportModes.Sea,
			Core.Constants.TransportModes.InlandWaterwayTransport,
			Core.Constants.TransportModes.Rail,
			Core.Constants.TransportModes.Road));
		ImmutableHashSet<ZString> calculateFreightNonAirTransportModes;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.OnApportionmentDirtyChanged += new BaseJobDeclaration.ApportionmentDirtyChangedEventHandler(CurrentDataItem_OnApportionmentDirtyChanged);
				CurrentDataItem_OnApportionmentDirtyChanged();
			}
		}

		void InvoiceHeadersBoundGrid_AfterBind(object sender, EventArgs e)
		{
			InvoiceHeadersBoundGrid.ListManager.PositionChanged += InvoiceHeadersBoundGridListManager_PositionChanged;
			InvoiceHeadersBoundGridListManager_PositionChanged(null, null);
		}

		void InvoiceHeadersBoundGridListManager_PositionChanged(object sender, EventArgs e)
		{
			UpdateCurrentInvoice();
		}

		protected virtual void UpdateCurrentInvoice()
		{
			JobComInvoiceHeader invoiceHeader = null;
			var listManager = InvoiceHeadersBoundGrid.ListManager;
			if (listManager != null)
			{
				invoiceHeader = (JobComInvoiceHeader)listManager.GetCurrent();
				if (invoiceHeader != null && invoiceHeader.IsDeleted)
				{
					invoiceHeader = null;
				}
			}

			if (currentInvoice != invoiceHeader)
			{
				UnHookInvoiceHeaderEvents(currentInvoice);
				currentInvoice = invoiceHeader;
				HookInvoiceHeaderEvents(invoiceHeader);
			}

			var declaration = currentInvoice?.JobDeclaration;
			UpdateInvoiceChargeButtonsVisibility(declaration);
		}
		JobComInvoiceHeader currentInvoice;

		void UpdateInvoiceChargeButtonsVisibility(JobDeclaration declaration)
		{
			var calculateFreightButtonEnabledAndVisible = declaration != null && IsCalculateFreightEnabled(declaration);
			InvoiceChargesCalculateFreightButton.Enabled = calculateFreightButtonEnabledAndVisible;
			InvoiceChargesCalculateFreightButton.Visible = calculateFreightButtonEnabledAndVisible;
			var insuranceButtonEnabledAndVisible = declaration?.SupportsCalculateInsurance ?? false;
			InvoiceChargesCalculateInsuranceButton.Enabled = insuranceButtonEnabledAndVisible;
			InvoiceChargesCalculateInsuranceButton.Visible = insuranceButtonEnabledAndVisible;
			InvoiceChargesCalculateDDPButton.Enabled = ShouldShowCalculateDDPButton;
			InvoiceChargesCalculateDDPButton.Visible = ShouldShowCalculateDDPButton;
		}

		protected virtual void HookInvoiceHeaderEvents(JobComInvoiceHeader invoiceHeader)
		{
		}

		protected virtual void UnHookInvoiceHeaderEvents(JobComInvoiceHeader invoiceHeader)
		{
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			AddColumns();
			JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns("JZ_OH_Buyer");
			HideGroupInvoiceColumn();
		}

		void AddColumns()
		{
			var isCostCalculationsTotalsUISupport = (JobDeclaration?.IsExport ?? false) && (Configuration?.ExportCostCalculationsTotalsUISupport ?? false);

			InvoiceChargesAddDeductTotal.Visible = isCostCalculationsTotalsUISupport;
			GroupChargesAddDeductTotal.Visible = isCostCalculationsTotalsUISupport;
			GroupInvoiceTotal.Visible = isCostCalculationsTotalsUISupport;

			ChargesGroupBox.Visible = !isCostCalculationsTotalsUISupport;
			InvoiceChargesGroupLabel.Visible = isCostCalculationsTotalsUISupport;
			BaseGroupChargesGroupBox.Visible = !isCostCalculationsTotalsUISupport;
			GroupChargesGroupLabel.Visible = isCostCalculationsTotalsUISupport;

			if (isCostCalculationsTotalsUISupport)
			{
				MoveControls(ChargesGroupBox, ChargesGroupsSplitterContainer.Panel1);
				ChargesTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
				MoveControls(BaseGroupChargesGroupBox, ChargesGroupsSplitterContainer.Panel2);
				BaseGroupChargesGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			}
			else
			{
				MoveControls(ChargesGroupsSplitterContainer.Panel1, ChargesGroupBox);
				ChargesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
				MoveControls(ChargesGroupsSplitterContainer.Panel2, BaseGroupChargesGroupBox);
				BaseGroupChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			}

			var zDropEditColumnStyleInfo21 = new ZDropEditColumnStyleInfo()
			{
				ColumnName = "JZ_ValuationCode",
				IsVisible = false,
				ToolTip = Res.GetString("f02b63f3-8dbb-4f46-91ae-3bab9f4b9553", "The Nature Of Transaction"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85),
			};

			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo21);

			var zDropEditColumnStyleInfo22 = new ZDateEditColumnStyleInfo()
			{
				ColumnName = "JZ_ValuationDateOverride",
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|B9E83581 - C2E5 - 4621 - AC9C - A2B68D8B52AB", "Valuation Date", "Date Of Valuation"),
				IsVisible = true,
				ToolTip = Res.GetString("efee218a-316c-49b9-93a1-df85e85bd4db", "Date of valuation"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85),
			};

			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo22);

			if (!isCostCalculationsTotalsUISupport)
			{
				using (ApportionedChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					ApportionedChargesGrid.ColumnStyles.Add(CreateIsStatisticalValueApplicableColumn());
					ApportionedChargesGrid.ColumnStyles.Add(CreateFixedRateColumn());
					ApportionedChargesGrid.ColumnStyles.Add(CreateExchangeRateColumn());
					ApportionedChargesGrid.ReOrderColumns(
						// If you add new columns, also change the index value below.  (The remove-by-name method doesn't work)
						[
							InvoiceCharge.Schema.J7_ChargeType,
							InvoiceCharge.Schema.ChargeCodeDescription,
							InvoiceCharge.Schema.J7_Amount,
							InvoiceCharge.Schema.J7_RX_NKCurrency,
							InvoiceCharge.Schema.J7_IsDutiable,
							InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
							InvoiceCharge.Schema.J7_IsGSTApplicable,
							InvoiceCharge.Schema.J7_IsIncludedInITOT,
							InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
							InvoiceCharge.Schema.J7_FullOrPartialApportionment,
							JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable,
							InvoiceCharge.Schema.J7_ExchangeRate
						]);
				}

				using (InvoiceChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					InvoiceChargesGrid.ColumnStyles.Add(CreateIsStatisticalValueApplicableColumn());
					InvoiceChargesGrid.ColumnStyles.Add(CreateFixedRateColumn());
					InvoiceChargesGrid.ColumnStyles.Add(CreateExchangeRateColumn());
					InvoiceChargesGrid.ReOrderColumns(
						// If you add new columns, also change the index value below.  (The remove-by-name method doesn't work)
						[
							InvoiceCharge.Schema.J7_ChargeType,
							InvoiceCharge.Schema.ChargeCodeDescription,
							InvoiceCharge.Schema.J7_Amount,
							InvoiceCharge.Schema.J7_RX_NKCurrency,
							InvoiceCharge.Schema.J7_IsDutiable,
							InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
							InvoiceCharge.Schema.J7_IsGSTApplicable,
							InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
							InvoiceCharge.Schema.J7_PrepaidCollect,
							InvoiceCharge.Schema.J7_Percentage,
							InvoiceCharge.Schema.J7_DistributeBy,
							JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable,
							InvoiceCharge.Schema.J7_ExchangeRate
						]);
				}

				using (BaseGroupChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					BaseGroupChargesGrid.ColumnStyles.Add(CreateIsStatisticalValueApplicableColumn());
					BaseGroupChargesGrid.ColumnStyles.Add(CreateFixedRateColumn());
					BaseGroupChargesGrid.ColumnStyles.Add(CreateExchangeRateColumn());
					BaseGroupChargesGrid.ReOrderColumns(
						// If you add new columns, also change the index value below.  (The remove-by-name method doesn't work)
						[
							BaseGroupInvoiceCharge.Schema.J7_ChargeType,
							BaseGroupInvoiceCharge.Schema.ChargeCodeDescription,
							BaseGroupInvoiceCharge.Schema.J7_Amount,
							BaseGroupInvoiceCharge.Schema.J7_RX_NKCurrency,
							BaseGroupInvoiceCharge.Schema.J7_IsDutiable,
							BaseGroupInvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
							BaseGroupInvoiceCharge.Schema.J7_IsGSTApplicable,
							BaseGroupInvoiceCharge.Schema.J7_Percentage,
							BaseGroupInvoiceCharge.Schema.J7_PrepaidCollect,
							BaseGroupInvoiceCharge.Schema.J7_DistributeBy,
							BaseGroupInvoiceCharge.Schema.J7_FullOrPartialApportionment,
							BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT,
							JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable,
							BaseGroupInvoiceCharge.Schema.J7_ExchangeRate
						]);
				}

				if (!ShouldShowVatGstColumnInChargesGrid)
				{
					// 6 is VAT/GST/IVA
					BaseGroupChargesGrid.ColumnStyles.RemoveAt(6);
					InvoiceChargesGrid.ColumnStyles.RemoveAt(6);
					ApportionedChargesGrid.ColumnStyles.RemoveAt(6);
				}
			}

			var zOrganisationFindBoxColumnStyleInfo3 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				ColumnName = "ManufacturerOrgPK",
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|4F1F0E64-2A24-4ABE-92D2-2BACD9995A11", "Manufacturer"),
				GroupName = Res.GetData("D8B3D460-CCDD-449D-8329-151C2ACC33C7", "Manufacturer"),
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			};

			var zGuidDropEditColumnStyleInfo3 = new ZGuidDropEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|6DD6ECEE-6653-4139-B846-EF85494EE40E", "Manufacturer Address"),
				GroupName = Res.GetData("D8B3D460-CCDD-449D-8329-151C2ACC33C7", "Manufacturer"),
				IsVisible = false,
				ColumnName = "JZ_OA_ManufacturerAddress",
				ToolTip = Res.GetString("3559a4a2-4c08-4ddc-8660-4769f10fab14", "Invoice\'s Manufacturer Address"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(129),
			};

			var zOrganisationFindBoxColumnStyleInfo4 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|69661d3d-8682-4af2-ad0d-69a8c97d7b5b", "Supplier"),
				GroupName = Res.GetData("D8B3D460-CCDD-449D-8329-151C2ACC33C6", "Supplier"),
				ColumnName = "SupplierOrgPK",
				IsMandatory = true,
				IsVisible = false,
			};

			var zGuidDropEditColumnStyleInfo4 = new ZGuidDropEditColumnStyleInfo()
			{
				GroupName = Res.GetData("D8B3D460-CCDD-449D-8329-151C2ACC33C6", "Supplier"),
				IsVisible = false,
				ColumnName = "JZ_OA_SupplierAddress",
				ToolTip = Res.GetString("6f2f709d-6a90-4457-8043-40c313d770c4", "Invoice\'s Supplier Address"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104),
			};

			var zOrganisationFindBoxColumnStyleInfo5 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				GroupName = Res.GetData("25B0B796-241C-487B-9FF5-BB6B3F720F26", "Buyer"),
				ColumnName = JobComInvoiceHeader.Schema.BuyerOrgPK,
				IsVisible = true,
			};

			var zGuidDropEditColumnStyleInfo5 = new ZGuidDropEditColumnStyleInfo()
			{
				GroupName = Res.GetData("25B0B796-241C-487B-9FF5-BB6B3F720F26", "Buyer"),
				IsVisible = true,
				ColumnName = JobComInvoiceHeader.Schema.JZ_OA_BuyerAddress,
				ToolTip = Res.GetString("49303431-3c99-4169-ba1f-22e7398a40c0", "Invoice\'s Buyer Address"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93),
			};

			var zOrganisationFindBoxColumnStyleInfo6 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|A42B1362-E83F-41C9-B75D-4E49203A92B2", "Sold To Party"),
				GroupName = Res.GetData("0036B049-55DD-43B1-BB32-A1E488300EBF", "Sold To Party"),
				ColumnName = "SoldToPartyOrgPK",
				IsVisible = false,
			};

			var zGuidDropEditColumnStyleInfo6 = new ZGuidDropEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|A872C0FA-775B-4C62-9D18-8E8D26A166C6", "Sold To Party Address"),
				GroupName = Res.GetData("0036B049-55DD-43B1-BB32-A1E488300EBF", "Sold To Party"),
				IsVisible = false,
				ColumnName = "JZ_OA_SoldToPartyAddress",
				ToolTip = Res.GetString("983da63f-4a83-4adf-9b44-d9596304e0c8", "Invoice\'s Sold To Party Address"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
			};

			var zOrganisationFindBoxColumnStyleInfo7 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|1760BC2C-1DAF-4260-B2E8-E32180FA7F00", "Ship To Party"),
				GroupName = Res.GetData("20735BD3-20CC-49C9-A4BA-AADA861D8A43", "Ship To Party"),
				ColumnName = "ShipToPartyOrgPK",
				IsVisible = false,
			};

			var zGuidDropEditColumnStyleInfo7 = new ZGuidDropEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|F8BC6834-E733-432F-8222-741786CD634A", "Ship To Party Address"),
				GroupName = Res.GetData("20735BD3-20CC-49C9-A4BA-AADA861D8A43", "Ship To Party"),
				IsVisible = false,
				ColumnName = "JZ_OA_ShipToPartyAddress",
				ToolTip = Res.GetString("5083b748-c91e-46fa-9135-afcf164a945c", "Invoice\'s ShipToParty Address"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
			};

			var zOrganisationFindBoxColumnStyleInfo8 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				GroupName = Res.GetData("5ED3022F-FD05-4059-88B1-F1F952CC2E0A", "Seller"),
				ColumnName = JobComInvoiceHeader.Schema.SellerOrgPK,
				IsVisible = false,
			};

			var zGuidDropEditColumnStyleInfo8 = new ZGuidDropEditColumnStyleInfo()
			{
				GroupName = Res.GetData("5ED3022F-FD05-4059-88B1-F1F952CC2E0A", "Seller"),
				IsVisible = false,
				ColumnName = JobComInvoiceHeader.Schema.JZ_OA_SellerAddress,
				ToolTip = Res.GetString("b585e687-b020-4400-9b22-254d5fe07d9e", "Invoice\'s Seller Address"),
			};

			var zOrganisationFindBoxColumnStyleInfo9 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|30D0259C-5934-4E3A-8DAA-17E3D7F65F61", "Invoicer"),
				GroupName = Res.GetData("7E4E1EAB-2048-4CDF-8FBB-ED07DD6A19CA", "Invoicer"),
				ColumnName = "InvoicerOrgPK",
				IsVisible = false,
			};

			var zGuidDropEditColumnStyleInfo9 = new ZGuidDropEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|DFE3E671-12B3-4893-A5E5-96607018465C", "Invoicer Address"),
				GroupName = Res.GetData("7E4E1EAB-2048-4CDF-8FBB-ED07DD6A19CA", "Invoicer"),
				IsVisible = false,
				ColumnName = "JZ_OA_InvoicerAddress",
				ToolTip = Res.GetString("c02cdc5f-2c2d-47b4-ac02-d676b47cea51", "Invoice\'s Invoicer Address"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104),
			};

			var zOrganisationFindBoxColumnStyleInfo10 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				GroupName = Res.GetData("6B743D83-3B47-47C9-919B-20FF883F974F", "Exporter"),
				ColumnName = JobComInvoiceHeader.Schema.ExporterOrgPK,
				IsVisible = false,
			};

			var zGuidDropEditColumnStyleInfo10 = new ZGuidDropEditColumnStyleInfo()
			{
				GroupName = Res.GetData("6B743D83-3B47-47C9-919B-20FF883F974F", "Exporter"),
				IsVisible = false,
				ColumnName = JobComInvoiceHeader.Schema.JZ_OA_ExporterAddress,
				ToolTip = Res.GetString("09e23ddb-8743-4564-8660-af5ef0eb37c9", "Invoice\'s Exporter Address"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106),
			};

			var zOrganisationFindBoxColumnStyleInfo11 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|E793274A-DAFE-416C-B8E4-A2F370E99CA6", "Consignee"),
				GroupName = Res.GetData("9FFF614B-3064-47DE-B2F6-7D99B1C5D95A", "Consignee"),
				ColumnName = "ConsigneeOrgPK",
				IsVisible = false,
			};

			var zGuidDropEditColumnStyleInfo11 = new ZGuidDropEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|EFDBBE97-503E-4FBC-80B7-CFD302CD70F5", "Consignee Address"),
				GroupName = Res.GetData("9FFF614B-3064-47DE-B2F6-7D99B1C5D95A", "Consignee"),
				IsVisible = false,
				ColumnName = "JZ_OA_ConsigneeAddress",
				ToolTip = Res.GetString("82700678-8a16-40ea-8d27-3ed06d40adea", "Invoice\'s Consignee Address"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115),
			};

			var zOrganisationFindBoxColumnStyleInfo12 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|C532288D-6A28-4F2D-B0D1-D1D9ED8758E8", "Intermediate Consignee"),
				GroupName = Res.GetData("F32E010B-7B59-4AEC-B3D0-04D05F737EAC", "Intermediate Consignee"),
				ColumnName = "IntermediateConsigneeOrgPK",
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(139),
			};

			var zGuidDropEditColumnStyleInfo12 = new ZGuidDropEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|17F60D4B-ECB1-4A98-BBB3-C89912849DAE", "Intermediate Consignee Address"),
				GroupName = Res.GetData("F32E010B-7B59-4AEC-B3D0-04D05F737EAC", "Intermediate Consignee"),
				IsVisible = false,
				ColumnName = "JZ_OA_IntermediateConsigneeAddress",
				ToolTip = Res.GetString("94cfb7af-c926-4790-806a-8d5fb2dd9869", "Invoice\'s Intermediate Consignee Address"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(181),
			};

			var zOrganisationFindBoxColumnStyleInfo13 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceModuleButtonGrid|B1314273-71E5-4114-B12A-B2725A810471", "Selling Agent"),
				ColumnName = "SellingAgentOrgPK",
				IsVisible = false,
			};

			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo4);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo4);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo5);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo5);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo6);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo6);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo7);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo7);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo8);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo8);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo9);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo9);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo10);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo10);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo11);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo11);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo12);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo12);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo13);

			AddValueIndicatorsColumns();

			SetInvoiceDateColumnVisibilityAndSetPosition();
		}

		void MoveControls(Control source, Control destination)
		{
			foreach (var control in source.Controls.Cast<Control>().Where(x => !Object.ReferenceEquals(x, destination)))
			{
				source.Controls.Remove(control);
				destination.Controls.Add(control);
			}
		}

		void HideGroupInvoiceColumn()
		{
			if (JobDeclaration != null && !JobDeclaration.IsImport)
			{
				var groupInvoiceColumnStyle = InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice);
				if (groupInvoiceColumnStyle != null)
				{
					groupInvoiceColumnStyle.IsUnavailable = true;
					groupInvoiceColumnStyle.IsVisible = false;
				}
			}
		}

		void AddValueIndicatorsColumns()
		{
			var valueIndicatorsGroupName = ValueIndicatorsGroupName;
			JobComInvoiceHeadersBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZCheckBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceHeader.Schema.RelatedIndicator,
					CaptionResourceString = Res.GetData("F103B1F7-0E02-460E-94B1-6A3A43092955", "[4/13] Party Relationship"),
					GroupName = valueIndicatorsGroupName,
					IsVisible = false
				},
				new ZCheckBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceHeader.Schema.ZG_RelatedIndicator2,
					CaptionResourceString = Res.GetData("26AF44A2-6A0D-4C9D-821D-0B0660468CA8", "Restrictions"),
					GroupName = valueIndicatorsGroupName,
					IsVisible = false
				},
				new ZCheckBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceHeader.Schema.ZG_RelatedIndicator3,
					CaptionResourceString = Res.GetData("65E5FCE8-730D-478B-A785-7FCAE4322166", "Sale Conditions"),
					GroupName = valueIndicatorsGroupName,
					IsVisible = false
				},
				new ZCheckBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceHeader.Schema.ZG_RelatedIndicator4,
					CaptionResourceString = Res.GetData("8502995B-47B3-40B4-82B1-F382DC60DE23", "Disposal Accrual"),
					GroupName = valueIndicatorsGroupName,
					IsVisible = false
				}
			});
		}

		ResourceStringData ValueIndicatorsGroupName => Res.GetData("C5270FD5-CFE4-4D97-B72C-3D996F11F6D9", "[4/13] Value Indicators");

		void SetInvoiceDateColumnVisibilityAndSetPosition()
		{
			var invoiceDateColumn = JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(x => x.ColumnName == Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_InvoiceDate);
			invoiceDateColumn.IsVisible = true;
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Remove(invoiceDateColumn);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Insert(1, invoiceDateColumn);
		}

		internal static ZCheckBoxColumnStyleInfo CreateIsStatisticalValueApplicableColumn()
		{
			return new ZCheckBoxColumnStyleInfo()
			{
				ColumnName = InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
				CaptionResourceString = Res.GetData("03c727f3-5ab6-44f4-9a8d-47bad639c7c1", "Statistical Value Applicable"),
				ToolTip = Res.GetString("309d34ea-ede9-454c-a41a-fe1a5ea91af7", "Statistical Value Applicable"),
				IsVisible = true,
				IsReadOnly = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(152)
			};
		}

		internal static ZCheckBoxColumnStyleInfo CreateFixedRateColumn()
		{
			return new ZCheckBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("5FB64938-6464-4EB2-9621-AAD0B1C51CC5", "Fixed Rate"),
				ColumnName = InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
				GroupName = Res.GetData("InvoiceLineCharges|D3C21701-9613-4E5B-A540-EE07BD705B22", "Exchange Rate"),
				IsMandatory = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74)
			};
		}

		internal static ZCalcEditColumnStyleInfo CreateExchangeRateColumn()
		{
			return new ZCalcEditColumnStyleInfo()
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("A1C0DD81-4753-457B-B3EE-EAB1EBBEB632", "Exchange Rate"),
				ColumnName = InvoiceCharge.Schema.J7_ExchangeRate,
				Decimals = 6,
				GroupName = Res.GetData("InvoiceLineCharges|D3C21701-9613-4E5B-A540-EE07BD705B22", "Exchange Rate"),
				IsMandatory = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102)
			};
		}

		void InitInvoicePaymentUserControl()
		{
			invoicePaymentUserControl1.UserControlType = typeof(InvoicePaymentUserControl);
		}

		void InitAdditionalInfosUserControl()
		{
			additionalInfosUserControl1.UserControlType = GetAdditionalInfosUserControlType();
			additionalInfosUserControl1.HostedControlCreated += (sender, args) =>
			{
				if (additionalInfosUserControl1.HostedControl is ISupportingInfoUserControls)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(additionalInfosUserControl1.HostedControl, (NoResString)"Invoices", SupportingInfoColumnLayoutContext);
				}
			};
		}

		protected virtual Type GetAdditionalInfosUserControlType()
		{
			return typeof(AdditionalInfosUserControl);
		}

		void InitPreviousDocumentsUserControl()
		{
			previousDocumentsUserControl1.UserControlType = GetPreviousDocumentsUserControlType();
			previousDocumentsUserControl1.HostedControlCreated += (sender, args) =>
			{
				if (previousDocumentsUserControl1.HostedControl is ISupportingInfoUserControls previousDocumentsUserControl && previousDocumentsUserControl is Control control)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, (NoResString)"Invoices", SupportingInfoColumnLayoutContext);
				}
			};
		}

		protected virtual Type GetPreviousDocumentsUserControlType()
		{
			return typeof(PreviousDocumentsUserControl);
		}

		void InitSupportingDocumentsUserControl()
		{
			SupportingDocumentsUserControl.UserControlType = GetSupportingDocumentsUserControlType();
			SupportingDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				if (SupportingDocumentsUserControl.HostedControl is ISupportingDocumentsUserControl supportingDocumentsUserControl && supportingDocumentsUserControl is Control control)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, (NoResString)"Invoices", SupportingInfoColumnLayoutContext);
				}
			};
		}

		protected virtual Type GetValueIndicatorsUserControlType()
		{
			return typeof(ValueIndicatorsUserControl);
		}

		void InitValueIndicatorsUserControl()
		{
			ValueIndicatorsUserControl.UserControlType = GetValueIndicatorsUserControlType();
		}

		protected virtual Type GetSupportingDocumentsUserControlType()
		{
			return typeof(SupportingDocumentsUserControl);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var invoiceHeader = (JobComInvoiceHeader)CurrentInvoiceHeader;
				if (invoiceHeader != null)
				{
					UnHookInvoiceHeaderEvents(invoiceHeader);
				}
			}
			base.Dispose(disposing);
		}

		void InvoiceChargesCalculateFreightButton_Click(object sender, EventArgs e)
		{
			if (currentInvoice != null)
			{
				ShowCalculateFreightForm(currentInvoice.Charges);
			}
		}

		void ShowCalculateFreightForm(IJobComInvChargeCollection<JobComInvCharge> charges)
		{
			var jobDeclaration = CurrentDataItem;
			if (jobDeclaration != null)
			{
				ZChildForm form;
				if (jobDeclaration.IsAir)
				{
					form = GetCalculateFreightForm(charges);
				}
				else
				{
					form = GetCalculateFreightNonAirForm(charges);
				}

				if (form != null)
				{
					ZFormModaliser.Show(form, GetParentForm(this));
				}
			}
		}

		protected virtual CalculateFreightForm GetCalculateFreightForm(IJobComInvChargeCollection<JobComInvCharge> charges)
		{
			var bizObj = CalculateFreightBizObj.New(charges, CurrentDataItem);
			if (bizObj != null)
			{
				return new CalculateFreightForm(bizObj);
			}
			return null;
		}

		void ShowCalculateInsuranceForm(IJobComInvChargeCollection<JobComInvCharge> charges)
		{
			var jobDeclaration = CurrentDataItem;
			ZChildForm form = null;
			if (jobDeclaration != null)
			{
				form = GetCalculateInsuranceForm(charges);
			}

			if (form != null)
			{
				ZFormModaliser.Show(form, GetParentForm(this));
			}
		}

		protected virtual CalculateInsuranceForm GetCalculateInsuranceForm(IJobComInvChargeCollection<JobComInvCharge> charges)
		{
			CalculateInsuranceForm form = null;
			if (currentInvoice is not null && CurrentDataItem?.IncoTermAndChargeFactory is EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory)
			{
				var bizObj = GetCalculateInsuranceBusinessObject(euIncoTermAndChargeFactory, currentInvoice);
				form = new CalculateInsuranceForm(bizObj);
			}

			return form;
		}

		protected virtual CalculateInsuranceBizObj GetCalculateInsuranceBusinessObject(EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory, JobComInvoiceHeader invoiceHeader) => new(euIncoTermAndChargeFactory, invoiceHeader);

		protected virtual CalculateFreightNonAirForm GetCalculateFreightNonAirForm(IJobComInvChargeCollection<JobComInvCharge> charges)
		{
			CalculateFreightNonAirForm form = null;
			var bizObj = CalculateFreightNonAirBizObj.New(charges, CurrentDataItem);
			if (bizObj != null)
			{
				form = new CalculateFreightNonAirForm(bizObj);
			}
			return form;
		}

		void GroupChargesCalculateFreightButton_Click(object sender, EventArgs e)
		{
			var topGroupInvoice = JobDeclaration?.TopGroupInvoice;
			if (topGroupInvoice != null)
			{
				ShowCalculateFreightForm(topGroupInvoice.Charges);
			}
		}

		static ZForm GetParentForm(Control control)
		{
			ZForm result = null;
			while (control != null)
			{
				control = control.Parent;
				result = control as ZForm;
				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		void InvoiceChargesCalculateInsuranceButton_Click(object sender, EventArgs e)
		{
			var topGroupInvoice = JobDeclaration?.TopGroupInvoice;
			if (topGroupInvoice != null)
			{
				ShowCalculateInsuranceForm(topGroupInvoice.Charges);
			}
		}

		void InvoiceChargesCalculateDDPButton_Click(object sender, EventArgs e)
		{
			GetInvoiceChargesDDPCalculationResults();
		}

		protected virtual void GetInvoiceChargesDDPCalculationResults()
		{
		}

		InvoiceHeaderConfiguration Configuration => (JobDeclaration as JobDeclaration)?.Configuration.InvoiceHeaderConfiguration;

		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => this;

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => (JobDeclaration?.IsExport ?? false) && (Configuration?.ExportCostCalculationsTotalsUISupport ?? false) ? [Business.Declaration.JobDeclaration.CaptionKeyChargesExport] : JobDeclaration?.MultipleKeysToUse ?? Array.Empty<string>();
	}
}
