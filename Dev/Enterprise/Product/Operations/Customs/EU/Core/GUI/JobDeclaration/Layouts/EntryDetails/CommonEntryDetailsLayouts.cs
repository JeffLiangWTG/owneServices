using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class CommonEntryDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout EntryDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => EntryDetails;

		public CommonEntryDetailsLayouts()
		{
			EntryDetails = CreateInvoiceLineDetailsLayout();
		}

		PanelLayout CreateInvoiceLineDetailsLayout()
		{
			var builder = new CommonEntryDetailsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.NoPacksCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.DutyCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.VatCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.EntryLinesCountCalcEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.SubmittedDateDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.MRNTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ReleaseDateDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.EntryStatusDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
