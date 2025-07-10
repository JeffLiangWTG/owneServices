using System.Windows.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class Ucc6ExportPreviousDocumentFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public Ucc6ExportPreviousDocumentFieldsLayout()
	{
		Layout = CreateLayout();
	}

	static PanelLayout CreateLayout()
	{
		var builder = new Ucc6ExportPreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();
		var euBag = builder.CommonBag;
		var itBag = PreviousDocumentsFieldsControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(euBag.CodeDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.PackageQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.QuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(itBag.ItemNumberCalcEdit, ControlWidthClass.Auto);

		builder.AddControlBehaviour<ZDropEdit>(euBag.CodeDropEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZTextBox>(euBag.ReferenceTextBox, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZCalcDropEdit>(euBag.PackageQuantityCalcDropEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZCalcDropEdit>(euBag.QuantityCalcDropEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZCalcEdit>(itBag.ItemNumberCalcEdit, UpdateControlBehaviourAction);

		const int previousDocumentsFieldsControlWidth = 980;
		void UpdateControlBehaviourAction(Control control, PreviousDocument previousDocument)
		{
			control.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.MarkAsScaled(previousDocumentsFieldsControlWidth);
		}

		return builder.Build();
	}
}
