using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUExportInvoiceLineUserControl : AUInvoiceLineUserControl
	{
		public AUExportInvoiceLineUserControl()
		{
			InitializeComponent();

			InitialiseTariffFindBox();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);

			if (!DesignMode && !UseUniversalTariff)
			{
				var tariffColumnStyle = new AHECCTariffColumnStyleInfo(() => false)
				{
					ColumnName = JobComInvoiceLineSchema.Constants.JI_Tariff,
					IsVisible = false
				};

				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(tariffColumnStyle);
			}

			this.LineDetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			new UNDGDataItemFormManager(CustomsInvoiceLinesBoundGrid).Initialize(dGLinkLabel, flashPointCalcEdit, flashPointDescLabel, uNDGContactGuidFindBox, dGGuidFindBox);
		}

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

		protected override ModuleIdentifier ClassificationModuleID => ModuleIDs.ExportClassification;
		protected override bool UseUniversalTariff => AUCAHECCWrapper.EnableCWRefForAHECC;
		public bool UseCMRTariffTestData => AUCClassWrapper.UseCMRTariffTestData;
		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.Export;

		protected override string GetCustomsCountryCode() => UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;
		protected override ZString GetDataGroupingForUniversalTariff() => UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			JI_Calc_DutyConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_GSTConvertToLocalCurrencyControl.Visible = false;
		}

		#endregion

		#region Implementation

		void JI_AUStateBoundButton_Click(object sender, EventArgs e)
		{
			var manager = this.BindingContext[this.DataSource, "FilteredInvoiceLines"] as CurrencyManager;
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
