using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public sealed class EntryDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout EntryDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => EntryDetails;

		public EntryDetailsLayouts()
		{
			EntryDetails = CreateInvoiceLineDetailsLayout();
		}

		PanelLayout CreateInvoiceLineDetailsLayout()
		{
			var builder = new CommonEntryDetailsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;
			var esBag = EntryDetailsLayoutsControlBag.Instance;
			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(commonBag.TotalsLabel, ControlWidthClass.Auto);
			builder.Add(commonBag.NoPacksCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(esBag.InvoiceAmountCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.DutyCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.VatCalcEdit, ControlWidthClass.Auto);
			builder.Add(esBag.VATDeferredCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.EntryLinesCountCalcEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.CustomsLabel, ControlWidthClass.Auto);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.SubmittedDateDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.MRNTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.AcceptanceDateDateEdit, ControlWidthClass.Auto);
			builder.Add(esBag.CircuitTextBox, ControlWidthClass.Auto);
			builder.Add(esBag.CircuitCanTextBox, ControlWidthClass.Auto);
			builder.Add(esBag.CSVClearanceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ReleaseDateDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.EntryStatusDropEdit, ControlWidthClass.Auto);

			builder.SetVisibility(esBag.CircuitCanTextBox, i => i.DestinationStateIsCanaryIsland, i => i.ZG_DestinationStateInfo);

			return builder.Build();
		}
	}
}
