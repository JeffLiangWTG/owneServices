using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
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
			var builder = new EU.GUI.PlugIn.PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.CodeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
