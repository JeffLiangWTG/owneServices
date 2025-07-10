using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class MiscOptionsLayout : IPanelLayoutProvider
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
			builder.Add(krBag.ReturnGroupBox, ControlWidthClass.LongNoCaption);
			builder.Add(krBag.SouthNorthTradeGroupBox, ControlWidthClass.LongNoCaption);
			builder.Add(krBag.AdditionalCargoGroupBox, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}
	}
}
