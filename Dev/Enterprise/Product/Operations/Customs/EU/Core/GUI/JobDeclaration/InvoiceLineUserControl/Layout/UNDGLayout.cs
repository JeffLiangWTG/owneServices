using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class UNDGLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new UNDGLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.DGGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.FlashpointUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.DGLinkLabel, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
