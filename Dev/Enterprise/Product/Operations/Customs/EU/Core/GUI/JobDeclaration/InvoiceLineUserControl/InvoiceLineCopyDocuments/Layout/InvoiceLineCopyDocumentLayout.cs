using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class InvoiceLineCopyDocumentLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new InvoiceLineCopyDocumentLayoutBuilder();
			var commonBag = builder.CommonBag;
			builder.AddColumn();
			builder.Add(commonBag.InvoiceNumberDropEditGroupBox, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}
	}
}
