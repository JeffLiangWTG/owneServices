using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class LocalExportMiscOptionsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MiscOptionsLayoutBuilder();

			var krBag = MiscOptionsGroupBoxControlBag.Instance;
			builder.AddControlBag(krBag);

			builder.AddColumn();
			builder.Add(krBag.MiscellaneousGroupBox, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}
	}
}
