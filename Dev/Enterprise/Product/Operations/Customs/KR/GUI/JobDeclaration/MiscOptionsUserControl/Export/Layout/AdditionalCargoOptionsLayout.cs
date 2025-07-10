using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class AdditionalCargoOptionsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MiscOptionsLayoutBuilder();

			var krBag = MiscOptionsControlBag.Instance;
			builder.AddControlBag(krBag);

			builder.AddColumn();
			builder.Add(krBag.BondedTransportationPeriodUserControl, ControlWidthClass.Auto);
			builder.Add(krBag.UCRTextBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
