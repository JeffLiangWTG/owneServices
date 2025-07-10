using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class ImportAdditionalInfoLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new AdditionalInfoLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.ContentInfoTypeSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.ContentInfoTypeGrid, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(commonBag.AdditionalTariffsSeparatorUserControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.AdditionalTariffsGrid, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
