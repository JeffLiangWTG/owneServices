using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ExportSteelExportLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new InvoiceLineOtherDetailsLayoutBuilder();
			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.ApprovalNoTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.SteelExportEffectiveDateUserControl, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
