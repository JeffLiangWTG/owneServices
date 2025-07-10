using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class ImportSupplierHeaderUserControl : EUImportSupplierHeaderUserControl
	{
		public ImportSupplierHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceHeader.Schema.JZ_UCR,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
			});
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(false, JobComInvoiceHeader.Schema.SupplierOrgPK);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(false, JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress);
		}

		protected override bool IsCalculateFreightEnabled(Customs.Business.BaseJobDeclaration jobDeclaration) => jobDeclaration.IsAir;

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			var isImportV1orV2 = JobDeclaration is JobDeclaration declaration && declaration.IsImport && (declaration.JE_ApplicationCode.EqualsIgnoringCase(ImportDeclarationApplicationCodeList.Codes.V1) || declaration.JE_ApplicationCode.EqualsIgnoringCase(ImportDeclarationApplicationCodeList.Codes.V2));
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(isImportV1orV2, JobComInvoiceHeader.Schema.JZ_UCR);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(false, JobComInvoiceHeader.Schema.SupplierName);
		}

		protected override CalculateFreightForm GetCalculateFreightForm(IJobComInvChargeCollection<JobComInvCharge> charges) =>
			EU.Business.Declaration.CalculateFreightBizObj.New<CalculateFreightBizObj>(charges, CurrentDataItem) is CalculateFreightBizObj calculateFreightBizObj
				? new CalculateFreightForm(calculateFreightBizObj)
				: null;

		protected override ResourceStringData GetValueIndicatorsTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) =>
			declaration is JobDeclaration ieDeclaration && ieDeclaration.IsUCC5
			? Res.GetData("719F9EE2-E738-46D6-B116-5730277440B5", "[4/13] Valuation Indicators")
			: CaptionProvider.ValueIndicators;

		protected override ResourceStringData GetAdditionalInfoTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) =>
			declaration is JobDeclaration ieDeclaration && ieDeclaration.IsUCC5
			? Res.GetData("D9C04929-A9E9-4397-80E5-44366AD3F20D", "[2/2] Additional Information")
			: CaptionProvider.AdditionalDocuments;

		protected override ResourceStringData GetPreviousDocumentsTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) =>
			declaration is JobDeclaration ieDeclaration && ieDeclaration.IsUCC5
			? Res.GetData("A15A022D-E9EF-4769-9274-B377886ADD23", "[2/1] Previous Documents")
			: CaptionProvider.PreviousDocuments;

		protected override ResourceStringData GetSupportingDocumentsTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) =>
			declaration is JobDeclaration ieDeclaration && ieDeclaration.IsUCC5
			? Res.GetData("DB0800C4-B805-42F7-B503-FA5037CE6975", "[2/3] Supporting Documents")
			: CaptionProvider.SupportingDocuments;

		protected override Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);
		protected override Type GetAdditionalInfosUserControlType() => typeof(ImportAdditionalInfosUserControlWithGrid);
		protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLayoutSupportingDocumentsUserControl);
	}
}
