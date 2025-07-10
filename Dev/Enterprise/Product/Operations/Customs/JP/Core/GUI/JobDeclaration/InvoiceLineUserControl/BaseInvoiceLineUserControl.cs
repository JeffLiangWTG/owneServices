using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class BaseInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl, ISupportMultipleResourceStringDataSupporter
	{
		public BaseInvoiceLineUserControl()
		{
			InitializeComponent();
			ResetCustomsInvoiceLinesBoundGridColumns();
		}

		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Japan;

		protected override ZString UniversalTariffType => CurrentInvoiceLine?.UniversalTariffType ?? ZString.Empty;

		protected override ZString TariffColumnNameCore => JobComInvoiceLine.Schema.JI_FormattedTariff;

		public ISupportMultipleResourceStringData SupportMultipleResourceStringData => CurrentInvoiceLine as JobComInvoiceLine;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var declaration = dataSource as JobDeclaration;
			var showEntryInstruction = declaration?.AreMultipleEntryInstructionsAllowed ?? false;
			CustomsInvoiceLinesBoundGrid.SetAvailability(showEntryInstruction, [JobComInvoiceLine.Schema.JI_CEI, JobComInvoiceLine.Schema.EntryInstructionDescription]);
		}

		protected virtual void ResetCustomsInvoiceLinesBoundGridColumns()
		{
			var tariffColumn = CustomsInvoiceLinesBoundGrid.GetColumnStyle(TariffColumnName) as TariffColumnStyleInfo;
			tariffColumn.NeedLoadParentDataGroup = false;
			tariffColumn.GetTariffType = () => UniversalTariffType;
			tariffColumn.GetCountryCode = GetCustomsCountryCode;
			tariffColumn.GetDataGrouping = GetDataGroupingForUniversalTariff;
			tariffColumn.GetEffectiveDate = () => GetEffectiveAssessmentDateForUniversalTariff();
			tariffColumn.NeedLoadNomenclatureWhenTariffNotFound = true;

			var tariffColumnIndex = CustomsInvoiceLinesBoundGrid.ColumnStyles.IndexOf(tariffColumn);

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Insert(tariffColumnIndex + 1, new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = nameof(JobComInvoiceLine.TariffDescription),
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(85),
				IsReadOnly = true,
				CaptionResourceString = Res.GetData("D8097ED5-1CD3-4892-8B99-544529F90625", "Tariff Desc.", "Tariff Description", "")
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Insert(tariffColumnIndex + 2, new ZDropEditColumnStyleInfo
			{
				ColumnName = nameof(JobComInvoiceLine.JI_NACCSCode),
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(85),
				Caption = Res.GetString("B7C93317-B3A5-4087-945A-DAB3982D933D", "NACCS Code")
			});

			var entryInstructionDescriptionInfo = new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("BED86E36-B56A-4634-871A-721ECC59C177", "Entry Inst. Desc.", "Entry Instruction Desc.", "Entry Instruction Description"),
				ColumnName = JobComInvoiceLine.Schema.EntryInstructionDescription,
				GroupName = Res.GetData("45454DB4-3BE6-4CCA-901C-0A6E32AC3B26", "Entry Instruction"),
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(106),
			};

			var zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("6B2D11DB-4ED0-4B41-AC1F-BB7E2F5921C5", "Entry Ins.", "Entry Instruction"),
				ColumnName = "JI_CEI",
				GroupName = Res.GetData("45454DB4-3BE6-4CCA-901C-0A6E32AC3B26", "Entry Instruction"),
				ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
			};

			CustomsInvoiceLinesBoundGrid.ColumnStyles.InsertRange(2, new object[] { zGuidDropEditColumnStyleInfo1, entryInstructionDescriptionInfo });

			var invoiceQtyStyleInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle("JI_InvoiceQuantity");
			invoiceQtyStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			var customsQtyStyleInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle("JI_CustomsQuantity");
			customsQtyStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
		}
	}
}
