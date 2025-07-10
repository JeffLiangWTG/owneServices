using Enterprise.Customs.EU.GUI.CommercialInvoice;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.CommercialInvoice;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class ImportInvoiceDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		static PanelLayout CreateLayout()
		{
			var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
			var baseBag = InvoiceHeaderDetailsControlBag.Instance;
			var commonBag = builder.CommonBag;
			var euBag = EUInvoiceHeaderDetailsControlBag.Instance;

			builder.AddControlBag(baseBag);
			builder.AddControlBag(euBag);
			builder.AddControlBag(commonBag);

			builder.AddColumn();
			builder.Add(baseBag.InvoiceNumberBoundTextBox, ControlWidthClass.Auto);
			builder.Add(baseBag.InvoiceDateEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.InvoiceAmountCalcFindBox, ControlWidthClass.Auto);
			builder.Add(baseBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.IncotermAndIncotermPlaceUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
			builder.Add(baseBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.UCRTextBox, ControlWidthClass.Auto);

			builder.SetVisibility(commonBag.UCRTextBox, invoice => invoice.JobDeclaration is JobDeclaration declaration && (declaration.JE_ApplicationCode.EqualsIgnoringCase(ImportDeclarationApplicationCodeList.Codes.V1) || declaration.JE_ApplicationCode.EqualsIgnoringCase(ImportDeclarationApplicationCodeList.Codes.V2)), x => x.JobDeclaration?.JE_ApplicationCodeInfo);

			return builder.Build();
		}
	}
}
