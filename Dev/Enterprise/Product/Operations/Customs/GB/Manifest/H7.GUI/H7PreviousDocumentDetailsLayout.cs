using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	public sealed class H7PreviousDocumentDetailsLayout : IPanelLayoutProvider
	{
		static PanelLayout CreateLayout()
		{
			var builder = new EU.GUI.PlugIn.PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.CodeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);

			return builder.Build();
		}

		public PanelLayout Layout => CreateLayout();
	}
}
