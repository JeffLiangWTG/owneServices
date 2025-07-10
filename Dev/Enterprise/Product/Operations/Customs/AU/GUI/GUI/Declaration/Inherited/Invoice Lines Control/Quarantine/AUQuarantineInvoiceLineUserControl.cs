using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUQuarantineInvoiceLineUserControl : DeclarationInvoiceLineUserControl
	{
		public AUQuarantineInvoiceLineUserControl()
		{
			InitializeComponent();

			InitialiseTariffFindBox();
			RFPDetailsUserControl.InitializeEUTariffFindBox(GetEffectiveAssessmentDateForUniversalTariff);

			if (!DesignMode)
			{
				if (!UseUniversalTariff)
				{
					var tariffColumnStyle = new AHECCTariffColumnStyleInfo(() => false)
					{
						ColumnName = JobComInvoiceLineSchema.Constants.JI_Tariff,
						IsVisible = false
					};

					CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(tariffColumnStyle);
				}

				CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(DeclarationType.Quarantine);
				new UNDGDataItemFormManager(CustomsInvoiceLinesBoundGrid).Initialize(DGLinkLabel, FlashPointCalcEdit, FlashPointDescLabel, UNDGContactGuidFindBox, DGGuidFindBox);
			}
			LineDetailTabControl.Dock = DockStyle.Fill;
		}

		QuarantineExDocHeader quarantineExDocHeader;

		void InitialiseTariffFindBox()
		{
			if (UseUniversalTariff)
			{
				JI_TariffFindBox.GetEffectiveDate = GetEffectiveAssessmentDateForUniversalTariff;
				JI_TariffFindBox.ReadOnlyChanged += new EventHandler(this.JI_Tariff_ReadOnlyChanged);
				JI_TariffFindBox.Visible = true;
				JI_TariffFindBox.GetCountryCode = GetCustomsCountryCode;
				JI_TariffFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			}
			else
			{
				JI_TariffFindBoxAHECC.ReadOnlyChanged += new EventHandler(this.JI_Tariff_ReadOnlyChanged);
				JI_TariffFindBoxAHECC.Visible = true;
			}
		}

		#region Overrides

		protected new JobDeclaration CurrentDataItem => (JobDeclaration)base.CurrentDataItem;

		protected override bool UseUniversalTariff => AUCAHECCWrapper.EnableCWRefForAHECC;
		public bool UseCMRTariffTestData => AUCAHECCWrapper.UseCMRTariffTestData;
		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.Export;

		protected override ModuleIdentifier ClassificationModuleID => ModuleIDs.ExportClassification;
		protected override string GetCustomsCountryCode() => UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;
		protected override ZString GetDataGroupingForUniversalTariff() => UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			JI_Calc_DutyConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_GSTConvertToLocalCurrencyControl.Visible = false;
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (quarantineExDocHeader != null)
				{
					quarantineExDocHeader.QH_ProduceTypeInfo.ValueChanged -= QH_ProduceType_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		protected override void CustomsInvoiceLinesBoundGrid_AfterBind(object sender, EventArgs e)
		{
			base.CustomsInvoiceLinesBoundGrid_AfterBind(sender, e);

			quarantineExDocHeader = ((JobDeclaration)JobDeclaration).Invoices[0].QuarantineExDocHeader;
			quarantineExDocHeader.QH_ProduceTypeInfo.ValueChanged += QH_ProduceType_ValueChanged;
			QH_ProduceType_ValueChanged(null, null);
			RFPAnalysisUserControl.UpdateDairyFieldsIfRequired(quarantineExDocHeader);
			CusContainerInvoiceLineGrid.ReadOnly = !quarantineExDocHeader.Declaration.IsPersistent && quarantineExDocHeader.IsNEXDOCSActive;
		}

		void QH_ProduceType_ValueChanged(object sender, EventArgs e)
		{
			if (quarantineExDocHeader != null)
			{
				var isNEXDOCSActive = quarantineExDocHeader.IsNEXDOCSActive;
				RFPProcessUserControl.SetProcessingAndTreatmentGridsVisibility(isNEXDOCSActive);
				RFPDetailsUserControl.SetupVisibility(isNEXDOCSActive);
				RFPAnalysisUserControl.UpdateEggFieldsIfRequired(quarantineExDocHeader);

				SetupInvoiceLineTabs(isNEXDOCSActive);
				SetupInvoiceLineControls(isNEXDOCSActive);
			}
		}

		void SetupInvoiceLineTabs(bool isNEXDOCSActive)
		{
			RFPNumbersTabPage.TabVisible = !isNEXDOCSActive;
			REXProductAttachmentsTabPage.TabVisible = isNEXDOCSActive;

			if (isNEXDOCSActive)
			{
				RFPPackagesTabPage.Text = Res.GetString("E7ECF189-F5C0-45F6-9D94-F2C0C7311D58", "REX Packages");
				RFPDetailsTabPage.Text = Res.GetString("7E170C8D-95A5-402E-9A07-77A2C78AFD6D", "REX Details");
				RFPProcessTabPage.Text = Res.GetString("1DF4C8F2-55D5-4E89-BD30-0BC03F36AFC5", "REX Process");
				RFPCertificatesTabPage.Text = Res.GetString("DCED133B-0252-4885-9896-F93ECAFBEAFE", "REX Certificates");
				RFPAnalysisTabPage.Text = Res.GetString("BB7AF8CB-90C4-4D3B-B410-25FBF16BA626", "REX Analysis");
				RFPMeatTabPage.Text = Res.GetString("0167E598-9487-40C5-8FBB-5CE136AA13B1", "REX Meat");
				RFPStatementsTabPage.Text = Res.GetString("3A77D1F7-DCC6-468B-AB76-2457078410D7", "REX Statements");
			}
			else
			{
				RFPPackagesTabPage.Text = Res.GetString("596C192B-F227-4248-890F-6498372C41C9", "RFP Packages");
				RFPDetailsTabPage.Text = Res.GetString("B5B43B9C-D469-4015-AC98-8C20E09E650A", "RFP Details");
				RFPProcessTabPage.Text = Res.GetString("C8D4D731-E2EB-40F2-A27F-C160DF91960B", "RFP Process");
				RFPCertificatesTabPage.Text = Res.GetString("C4C75316-56A4-4FE8-97B0-46C735A6570A", "RFP Certificates");
				RFPAnalysisTabPage.Text = Res.GetString("D800FDEE-80F1-4F28-92BA-B0D183CFDC6B", "RFP Analysis");
				RFPMeatTabPage.Text = Res.GetString("9711B432-B682-464E-A78F-A2FCA9F91DC9", "RFP Meat");
				RFPStatementsTabPage.Text = Res.GetString("2C699028-F5FF-4D39-937B-39AF9E180B72", "RFP Statements");
				RFPNumbersTabPage.Text = Res.GetString("E4A7B13F-C7D3-4963-B974-DCC183006BBE", "RFP Numbers");
			}
		}

		void SetupInvoiceLineControls(ZBool isNEXDOCSActive)
		{
			// ClassificationDetailsGroupBox
			NoPermitRequiredCheckBox.Visible = isNEXDOCSActive;

			// RFPCertificatesTabPage
			RFPCertificatesUserControl.SetupVisibility(isNEXDOCSActive);

			// RFPAnalysisTabPage
			RFPAnalysisUserControl.SetupVisibility(isNEXDOCSActive);
		}
		#region Implementation

		void JI_AUStateBoundButton_Click(object sender, EventArgs e)
		{
			var manager = BindingContext[DataSource, "FilteredInvoiceLines"] as CurrencyManager;
			if (manager != null && manager.Position != -1)
			{
				var asAccountForLine = (JobComInvoiceLine)manager.GetCurrent();

				ZFormModaliser.ShowDialogAndDispose(new AUStatesForm(asAccountForLine));
			}
		}

		void AssayCodeBoundButton_Click(object sender, EventArgs e)
		{
			var manager = CustomsInvoiceLinesBoundGrid.ListManager;
			if (manager != null && manager.Position != -1)
			{
				var invoiceLine = (JobComInvoiceLine)manager.GetCurrent();
				ZFormModaliser.ShowDialogAndDispose(new ZEditAssayCodeForm(invoiceLine));
			}
		}

		void JI_Tariff_ReadOnlyChanged(object sender, EventArgs e)
		{
			if (sender is ZTextBox textBox)
			{
				AssayCodeBoundButton.Enabled = !textBox.ReadOnly;
			}
		}

		#endregion
	}
}
