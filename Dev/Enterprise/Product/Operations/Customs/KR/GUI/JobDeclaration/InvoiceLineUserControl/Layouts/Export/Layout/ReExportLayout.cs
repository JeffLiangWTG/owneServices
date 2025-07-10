using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ReExportLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var builder = new InvoiceLineDetailsLongCaptionLayoutBuilder();
			var common = builder.CommonBag;

			builder.AddColumn();
			builder.Add(common.PreviousEntryNumberTextBox, ControlWidthClass.Medium);
			builder.Add(common.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Medium);

			return builder.Build();
		}
	}
}
