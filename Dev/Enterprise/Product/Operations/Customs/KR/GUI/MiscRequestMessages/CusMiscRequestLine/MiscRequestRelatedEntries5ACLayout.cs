using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class MiscRequestRelatedEntries5ACLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MiscRequestRelatedEntriesLayoutBuilder();
			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.EntryNumberTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.EntryDetailsTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
