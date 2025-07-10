using System.Windows.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class Ucc6ExportEntryInstructionPreviousDocumentFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public Ucc6ExportEntryInstructionPreviousDocumentFieldsLayout()
	{
		Layout = CreatePreviousDocumentFieldsLayout();
	}

	static PanelLayout CreatePreviousDocumentFieldsLayout()
	{
		var builder = new EU.GUI.PlugIn.PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();

		var euBag = EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance;
		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.CodeDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Auto);

		builder.SetCaption(euBag.CodeDropEdit, _ => Res.GetData("B975FDA2-AFC2-46FE-9310-EE907FCC2F70", "Type", "[12 01 002 000] Type"));

		builder.AddControlBehaviour<ZDropEdit>(euBag.CodeDropEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZTextBox>(euBag.ReferenceTextBox, UpdateControlBehaviourAction);

		const int previousDocumentsFieldsControlWidth = 980;
		void UpdateControlBehaviourAction(Control control, PreviousDocument previousDocument)
		{
			control.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.MarkAsScaled(previousDocumentsFieldsControlWidth);
		}

		return builder.Build();
	}
}
