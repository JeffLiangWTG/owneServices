using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUDrawbackInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		public AUDrawbackInvoiceLineUserControl()
		{
			InitializeComponent();
			InitialiseTariffFindBox();

			JI_CountryOfOriginBoundFindBox.Visible = false;
			VolumeCalcDropEdit.Visible = false;
			JI_WeightCalcDropEdit.Visible = false;
			InvoiceLinesSummaryGroupBox.Visible = false;
			ClassificationDetailsGroupBox.Text = "Drawback Details";

			LineChargesTabPage.Controls.Remove(InvoiceLineCharges.ChargesGrid.Parent);
			InvoiceLineCharges.ApportionedChargesGroupBox.Dock = DockStyle.Fill;

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Drawback);
		}

		protected override bool UseUniversalTariff => AUCClassWrapper.UseCustomsReferenceData;
		public bool UseCMRTariffTestData => AUCClassWrapper.UseCMRTariffTestData;
		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.Import;
		protected override ModuleIdentifier ClassificationModuleID => ModuleIDs.ImportClassification;
		protected override string GetCustomsCountryCode() => UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;
		protected override ZString GetDataGroupingForUniversalTariff() => UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;

		const string AddInfoPrefix = JobComInvoiceLine.Schema.AddInfo + "+";

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;
		protected new JobComInvoiceLine CurrentInvoiceLine => base.CurrentInvoiceLine as JobComInvoiceLine;

		protected override bool SupportsBOMExpander => true;

		string[] CustomsInvoiceLinesBoundGridColumnNamesInSortOrder
		{
			get
			{
				if (customsInvoiceLinesBoundGridColumnNamesInSortOrder == null)
				{
					List<string> orderList = new List<string>();

					orderList.Add(JobComInvoiceLine.Schema.JI_Calc_Invoice);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_LineNo);
					orderList.Add("IsBOMParentLine");
					orderList.Add("BOMParentLineNumber");
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_PartNo);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_LinePrice);
					orderList.Add(AddInfoPrefix + AUAddInfo.Schema.ZA_DDN_Hidden);
					orderList.Add(AddInfoPrefix + AUAddInfo.Schema.ZA_DDL_Hidden);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_Description);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_CC);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_Tariff);
					orderList.Add(AddInfoPrefix + AUAddInfo.Schema.ZA_DAM_Hidden);
					orderList.Add(AddInfoPrefix + AUAddInfo.Schema.ZA_DCV_Hidden);
					orderList.Add(AddInfoPrefix + AUAddInfo.Schema.ZA_DTR_Hidden);
					orderList.Add(AddInfoPrefix + AUAddInfo.Schema.ZA_DDT_Hidden);
					orderList.Add(AddInfoPrefix + AUAddInfo.Schema.ZA_EDN_Hidden);
					orderList.Add(AddInfoPrefix + AUAddInfo.Schema.ZA_DARC_Hidden);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib1);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib2);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib3);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_SerialNumber);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib1);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib2);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib3);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib4);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib5);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib6);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_CustomTextBlob1);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_Weight);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_WeightUQ);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_Volume);
					orderList.Add(JobComInvoiceLineSchema.Constants.JI_VolumeUQ);
					orderList.Add(JobComInvoiceLine.Schema.MergedLineNumber);

					customsInvoiceLinesBoundGridColumnNamesInSortOrder = orderList.ToArray();
				}
				return customsInvoiceLinesBoundGridColumnNamesInSortOrder;
			}
		}
		string[] customsInvoiceLinesBoundGridColumnNamesInSortOrder;

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			ReorderAndChangeInvoiceLinesGridVisibility();
		}

		void ReorderAndChangeInvoiceLinesGridVisibility()
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(CustomsInvoiceLinesBoundGridColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetAllAvailability(false);
				CustomsInvoiceLinesBoundGrid.SetAvailability(true, CustomsInvoiceLinesBoundGridColumnNamesInSortOrder);
			}
		}

		void SelectEntryLinesButton_Click(object sender, EventArgs e)
		{
			var currentInvoiceLine = CurrentInvoiceLine;
			if (currentInvoiceLine != null && currentInvoiceLine.DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment)
			{
				ZFormModaliser.ShowDialogAndDispose(new DrawbackRefEntryLinesForm(currentInvoiceLine));
				currentInvoiceLine.CalculateClaimAmount();
			}
			else
			{
				Globals.Message.ShowError(SelectEntryLinesButtonErrorText, "Only for method B");
			}
		}

		public const string SelectEntryLinesButtonErrorText = "This button may only be used for lines with Drawback Method B.";

		void InitialiseTariffFindBox()
		{
			if (UseUniversalTariff)
			{
				tariffFindBox.GetEffectiveDate = GetEffectiveAssessmentDateForUniversalTariff;
				tariffFindBox.Visible = true;
				tariffFindBox.GetCountryCode = GetCustomsCountryCode;
				tariffFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			}
			else
			{
				tariffFindBoxAUCClass.Visible = true;

				if (!DesignMode)
				{
					var tariffColumnStyle = new AUCClassColumnStyleInfo()
					{
						ColumnName = JobComInvoiceLineSchema.Constants.JI_Tariff,
						CharacterCasing = CharacterCasing.Upper,
						Caption = "Tariff",
						ToolTip = "Tariff",
					};

					CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(tariffColumnStyle);
				}
			}
		}
	}
}
