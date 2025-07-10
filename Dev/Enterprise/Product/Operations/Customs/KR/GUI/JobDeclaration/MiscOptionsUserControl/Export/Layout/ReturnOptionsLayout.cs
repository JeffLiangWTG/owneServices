using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ReturnOptionsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MiscOptionsLayoutBuilder();

			var krBag = MiscOptionsControlBag.Instance;
			builder.AddControlBag(krBag);

			builder.AddColumn();
			builder.Add(krBag.ReturnReasonDropEdit, ControlWidthClass.Auto);
			builder.Add(krBag.ReturnTypeDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
