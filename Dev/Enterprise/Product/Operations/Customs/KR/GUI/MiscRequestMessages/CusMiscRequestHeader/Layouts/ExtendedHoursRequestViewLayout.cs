using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ExtendedHoursRequestViewLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new CusMiscRequestHeaderViewLayoutBuilder();
			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(controlBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.CustomsDivisionCodeFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.BranchGuidFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.RequestDetailsTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(controlBag.ApplicationNumberTextBox, ControlWidthClass.Auto);
			builder.Add(controlBag.StatusDropEdit, ControlWidthClass.Long);
			builder.Add(controlBag.RequestDateEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.EntryCountCalcEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
