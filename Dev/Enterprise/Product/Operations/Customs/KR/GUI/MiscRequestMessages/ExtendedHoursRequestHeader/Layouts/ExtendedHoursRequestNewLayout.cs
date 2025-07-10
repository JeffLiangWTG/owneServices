using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ExtendedHoursRequestNewLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new ExtendedHoursRequestNewLayoutsBuilder();
			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(controlBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.CustomsDivisionCodeFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.RequestPeriodStartDateEdit, ControlWidthClass.Long);
			builder.Add(controlBag.RequestPeriodEndDateEdit, ControlWidthClass.Long);
			builder.Add(controlBag.BranchGuidFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(controlBag.RequestReasonTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
