using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
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
			var builder = new PreviousDocumentsFieldsLayoutBuilder();

			var ilBag = PreviousDocumentsFieldsControlBag.Instance;

			builder.AddControlBag(ilBag);

			builder.AddColumn();
			builder.Add(ilBag.CodeCodeFindBox, ControlWidthClass.Auto);
			builder.Add(ilBag.ReferenceTextBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
