using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	sealed class UCC6PreviousDocumentFieldsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public UCC6PreviousDocumentFieldsLayout()
		{
			Layout = CreateInvoiceDetailsLayout();
		}
		static PanelLayout CreateInvoiceDetailsLayout()
		{
			var builder = new PreviousDocumentsFieldsLayoutBuilder<Business.Declaration.MultiLineAddInfos.PreviousDocument>();

			var euBag = PreviousDocumentsFieldsControlBag.Instance;

			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(euBag.CodeDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.PackageQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.QuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.ItemNumberCalcEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
