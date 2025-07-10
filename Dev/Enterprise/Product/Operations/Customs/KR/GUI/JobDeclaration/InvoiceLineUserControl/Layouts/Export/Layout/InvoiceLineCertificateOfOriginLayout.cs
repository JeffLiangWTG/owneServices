using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class InvoiceLineCertificateOfOriginLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var builder = new InvoiceLineDetailsLongCaptionLayoutBuilder();
			var common = builder.CommonBag;

			var krBag = InvoiceLineCertificateOfOriginControlBag.Instance;
			builder.AddControlBag(krBag);

			builder.AddColumn();
			builder.Add(common.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(krBag.COOIndicatorDropEdit, ControlWidthClass.Long);
			builder.Add(krBag.COODeterminationRuleDropEdit, ControlWidthClass.Long);
			builder.Add(krBag.COOLabelLocationDropEdit, ControlWidthClass.Long);
			builder.Add(krBag.FTATypeDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
