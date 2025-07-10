using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public sealed class TransactionLinesGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

		static IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = TransactionLinesGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.DescriptionTextBoxColumn);
			builder.AddColumn(euGridColumnBag.TariffColumn);
			builder.AddColumn(euGridColumnBag.InvoiceValueCalcEditColumn);
			builder.AddColumn(euGridColumnBag.StatisticalValueCalcEditColumn);
			builder.AddColumn(euGridColumnBag.CurrencyDropEditColumn);
			builder.AddColumn(euGridColumnBag.MassInKilogramsCalcEditColumn);
			builder.AddColumn(euGridColumnBag.MassInKilogramsUnitDropEditColumn);
			builder.AddColumn(euGridColumnBag.SupplementaryQuantityCalcEditColumn);
			builder.AddColumn(euGridColumnBag.SupplementaryQuantityUnit);
			builder.AddColumn(euGridColumnBag.CountryOfOriginDropEditColumn);
			builder.AddColumn(euGridColumnBag.RegionDropEditColumn);

			return builder.Build();
		}

		IGridColumnLayout layout;
	}
}
