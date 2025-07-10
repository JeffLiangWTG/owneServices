using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportSupplierHeaderUserControl : LayoutDeclarationInvoiceHeaderUserControl
	{
		public ImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			CustomsDetailsPanel.UpdateLayout(new CustomsDetailsLayout());

			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			InvoiceHeadersBoundGrid.GridId = (NoResString)"GridLayoutT+oYGAheR1633dT3oeP+lw==";

			JZ_CIFAmountBoundCurrencyControl.Visible = false;
			JZ_Calc_TNIBoundInvoiceCurrencyControl.Visible = false;

			ApportionmentPendingLabel.AllowOverlap(CustomsValueKRW);
			ApportionmentPendingLabel.AllowOverlap(CustomsValueUSD);

			QuestionGroupBoxUserControl.ChangeBindingTo934();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			EditColumns();
			AddChargeColumns();
			ReOrderColumns();
			ReOrderTabPage();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateMailItemsVisibility();
			UpdateValuationDeclarationVisibility();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			declaration = dataSource as JobDeclaration;
			if (declaration != null)
			{
				declaration.JE_ProcedureTypeInfo.ValueChanged -= ProcedureTypeInfo_ValueChanged;
				declaration.JE_ProcedureTypeInfo.ValueChanged += ProcedureTypeInfo_ValueChanged;
			}

			HookToValuationCodeValueChanged();

			var listManager = InvoiceHeadersBoundGrid.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged -= InvoiceHeaderBoundGrid_CurrentFocusedRowChanged;
				listManager.CurrentChanged += InvoiceHeaderBoundGrid_CurrentFocusedRowChanged;
			}
		}
		JobDeclaration declaration;

		protected new JobComInvoiceHeader CurrentInvoiceHeader => (JobComInvoiceHeader)base.CurrentInvoiceHeader;

		void ProcedureTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateMailItemsVisibility();
		}

		void UpdateMailItemsVisibility()
		{
			var procedureType = declaration?.JE_ProcedureType ?? ZString.Empty;
			MailItemsTabPage.TabVisible = procedureType == DeclarationProcedureTypeCodeList.Codes._26;
		}

		void ValuationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateValuationDeclarationVisibility();
		}

		void InvoiceHeaderBoundGrid_CurrentFocusedRowChanged(object sender, EventArgs e)
		{
			HookToValuationCodeValueChanged();
			UpdateValuationDeclarationVisibility();
		}

		void HookToValuationCodeValueChanged()
		{
			if (CurrentInvoiceHeader != null)
			{
				CurrentInvoiceHeader.JZ_ValuationCodeInfo.ValueChanged -= ValuationCodeInfo_ValueChanged;
				CurrentInvoiceHeader.JZ_ValuationCodeInfo.ValueChanged += ValuationCodeInfo_ValueChanged;
			}
		}

		void UpdateValuationDeclarationVisibility()
		{
			if (CurrentInvoiceHeader != null)
			{
				var isValuationMethodDefaultOrA = CurrentInvoiceHeader.JZ_ValuationCode.IsEmpty || CurrentInvoiceHeader.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne;
				QuestionTabPage.TabVisible = isValuationMethodDefaultOrA;
				PriceTabPage.TabVisible = isValuationMethodDefaultOrA;

				MethodTwoToSixTabPage.TabVisible = !isValuationMethodDefaultOrA;
				MethodTwoToThreeTabPage.TabVisible = ValuationCodeList.IsValuationMethodTwoToThree(CurrentInvoiceHeader.JZ_ValuationCode);
				MethodFourTabPage.TabVisible = ValuationCodeList.IsValuationMethodFour(CurrentInvoiceHeader.JZ_ValuationCode);
				MethodFiveToSixTabPage.TabVisible = ValuationCodeList.IsValuationMethodFiveToSix(CurrentInvoiceHeader.JZ_ValuationCode);
			}
		}

		void EditColumns()
		{
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_OH_Buyer)));
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_GroupInvoice)));
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_CIFAmount)));
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_CIFCurrency)));

			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Remarks)).CaptionResourceString = null;
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Remarks)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_IncoTerm)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceAmount)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.InvoiceLineTotal)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_IncoTerm)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceAmount)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.InvoiceLineTotal)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.SupplierName)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.SupplierName)).CaptionResourceString = Res.GetData("F926CFAB-9257-4E22-995F-EA911EFD7F8D", "Supplier Name");
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBAmount)).CaptionResourceString = FobCaptionAndGroup;
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBAmount)).GroupName = FobCaptionAndGroup;
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBCurrency)).CaptionResourceString = FobUnitCaption;
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBCurrency)).GroupName = FobCaptionAndGroup;

			InvoiceHeadersBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				 new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.JZ_ValuationCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZOrganisationFindBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.ManufacturerOrgPK),
					GroupName = ManufacturerGroup,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZAddressDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Normal,
					ColumnName = nameof(JobComInvoiceHeader.JZ_OA_ManufacturerAddress),
					GroupName = ManufacturerGroup,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZOrganisationFindBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.ShipperOrgPK),
					GroupName = ShipperGroup,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZAddressDropEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.JZ_OA_ShipperAddress),
					GroupName = ShipperGroup,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_ImportCargoManagementNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170)
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_BlanketValuationDeclarationNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_PaymentTerms),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_COOStatus),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_ValuationDecAttachCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_RN_NKDefaultOrigin),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.CriteriaForDeterminingCountryOfOrigin),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_COOLabelLocation),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_COOLabelType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_COOExemptionReason),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_OnlineTradeType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZOrganisationFindBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.DistributorOrgPK),
					GroupName = DistributorGroup,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
				},
				new ZAddressDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_OA_DistributorAddress),
					GroupName = DistributorGroup,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZOrganisationFindBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.SellerOrgPK),
					GroupName = SellerGroup,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
				},
				new ZAddressDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_OA_SellerAddress),
					GroupName = SellerGroup,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZOrganisationFindBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_OH_SellingAgent),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.CertificateOfOriginNo),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.CertificateOfOriginIssuingCountry),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.CertificateOfOriginIssueDate),
					DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.CertificateOfOriginCriteriaCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.CertificateOfOriginStatus),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.CertificateOfOriginAgencyName),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.CertificateOfOriginAreaName),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.CertificateOfOriginPersonName),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.PurchaseOrderNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260)
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.PurchaseOrderDate),
					DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.ContractNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260)
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.ContractDate),
					DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.JZ_ProvPricingYN),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.JZ_ProvAdditionalRate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.JZ_ImpContractExpiryDate),
					DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.JZ_ProvAdditionalAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.JZ_EstimatedDateOfFinalPrice),
					DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
			});

			InvoiceChargesGrid.ColumnStyles.Remove(InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
			ApportionedChargesGrid.ColumnStyles.Remove(ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
			BaseGroupChargesGrid.ColumnStyles.Remove(BaseGroupChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
		}

		void AddChargeColumns()
		{
			InvoiceChargesGrid.AddExchangeRateColumn();
			ApportionedChargesGrid.AddExchangeRateColumn();
			BaseGroupChargesGrid.AddExchangeRateColumn();
		}

		void ReOrderColumns()
		{
			InvoiceHeadersBoundGrid.ReOrderColumnsAndChangeVisibility(headerColumnsInOrder);
			InvoiceChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.HeaderChargeColumnsInOrder);
			ApportionedChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.HeaderChargeColumnsInOrder);
			BaseGroupChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.GroupChargeColumnsInOrder);
		}

		void ReOrderTabPage()
		{
			CustomsDetailsTabPage.TabIndex = 1;
			COAndFTATabPage.TabIndex = 2;
			MailItemsTabPage.TabIndex = 3;
			CustomFieldsTabPage.TabIndex = 4;
		}

		readonly string[] headerColumnsInOrder =
		{
			nameof(JobComInvoiceHeader.JZ_InvoiceNumber),
			nameof(JobComInvoiceHeader.JZ_OH_Supplier),
			nameof(JobComInvoiceHeader.ShipperOrgPK),
			nameof(JobComInvoiceHeader.JZ_OA_ShipperAddress),
			nameof(JobComInvoiceHeader.JZ_IncoTerm),
			nameof(JobComInvoiceHeader.JZ_InvoiceAmount),
			nameof(JobComInvoiceHeader.JZ_RX_NKInvoice_Currency),
			nameof(JobComInvoiceHeader.JZ_InvoiceCurrExRate),
			nameof(JobComInvoiceHeader.JZ_Calc_BalanceString),
			nameof(JobComInvoiceHeader.InvoiceLineTotal),
			nameof(JobComInvoiceHeader.JZ_CU_RelatedHouseBill),
			nameof(JobComInvoiceHeader.JZ_ImportCargoManagementNumber),
			nameof(JobComInvoiceHeader.JZ_ValuationCode),
			nameof(JobComInvoiceHeader.JZ_BlanketValuationDeclarationNumber),
			nameof(JobComInvoiceHeader.JZ_Weight),
			nameof(JobComInvoiceHeader.JZ_WeightUQ),
			nameof(JobComInvoiceHeader.JZ_NetWeight),
			nameof(JobComInvoiceHeader.JZ_NetWeightUQ),
			nameof(JobComInvoiceHeader.JZ_PaymentTerms),
			nameof(JobComInvoiceHeader.JZ_COOStatus),
			nameof(JobComInvoiceHeader.JZ_ValuationDecAttachCode),
			nameof(JobComInvoiceHeader.JZ_RN_NKDefaultOrigin),
			nameof(JobComInvoiceHeader.CriteriaForDeterminingCountryOfOrigin),
			nameof(JobComInvoiceHeader.JZ_COOLabelLocation),
			nameof(JobComInvoiceHeader.JZ_COOLabelType),
			nameof(JobComInvoiceHeader.JZ_COOExemptionReason),
			nameof(JobComInvoiceHeader.JZ_Remarks),
		};

		static ResourceStringData ManufacturerGroup => Res.GetData("ACF03B22-9008-46A9-BA88-2B0B8461D873", "Manufacturer");
		static ResourceStringData ShipperGroup => Res.GetData("F534E27C-023F-428E-8113-2CF04CBA81D7", "Shipper");
		static ResourceStringData DistributorGroup => Res.GetData("7A54310C-C5B2-46CB-8411-56A092EE787C", "Online Trade Distributor");
		static ResourceStringData SellerGroup => Res.GetData("597478FE-4447-4D3E-BB02-C0D44EBCE6C4", "Online Trade Seller");
		static ResourceStringData FobCaptionAndGroup => Res.GetData("81E892E9-AD57-4F67-84CC-C4F7BBAE4B30", "Customs Value");
		static ResourceStringData FobUnitCaption => Res.GetData("E25CA814-F4AD-48D8-A487-C929BE77494A", "Curr.", "Currency", "");
	}
}
