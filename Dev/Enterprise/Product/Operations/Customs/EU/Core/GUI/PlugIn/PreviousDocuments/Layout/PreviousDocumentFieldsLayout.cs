using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class PreviousDocumentFieldsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public PreviousDocumentFieldsLayout()
		{
			Layout = CreatePreviousDocumentFieldsLayout();
		}

		static PanelLayout CreatePreviousDocumentFieldsLayout()
		{
			var builder = new PreviousDocumentsFieldsLayoutBuilder<Business.Declaration.MultiLineAddInfos.PreviousDocument>();

			var euBag = PreviousDocumentsFieldsControlBag.Instance;

			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(euBag.CodeDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
