using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MiscellaneousOptionsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MiscOptionsLayoutBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
