using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class WarehouseAdjustmentInvoiceLineDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
			var commonBag = builder.CommonBag;
			var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(euBag);
			var deBag = InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(deBag);

			builder.AddColumn();
			builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.InvoiceNumberDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
			builder.Add(deBag.OutwardMRNTextBox, ControlWidthClass.Long);
			builder.Add(deBag.OutwardDecisiveDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PreviousEntryNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Auto);

			builder.SetCaption(commonBag.BondedWhsQuantityCalcDropEdit, i => i?.BondedWhsQuantityAdjustmentResourceDataString);

			return builder.Build();
		}
	}
}
