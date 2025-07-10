using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class Phase5GoodsItemPreviousDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
{
	public Phase5GoodsItemPreviousDocumentLayoutWithGrid()
	{
		Layout = CreateGoodsItemPreviousDocumentLayout();
	}

	Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(GoodsItemPreviousDocumentsGridUserControl);

	public PanelLayout Layout { get; }

	PanelLayout CreateGoodsItemPreviousDocumentLayout()
	{
		var builder = new Phase5GoodsItemPreviousDocumentsFieldsLayoutBuilder();
		var euBag = builder.CommonBag;
		var itBag = PreviousDocumentsFieldsControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(euBag.CodeDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.PackageQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.QuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(itBag.ItemNumberCalcEdit, ControlWidthClass.Auto);
		builder.Add(euBag.Reference2TextBox, ControlWidthClass.Auto);

		builder.AddControlBehaviour<ZDropEdit>(euBag.CodeDropEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZTextBox>(euBag.ReferenceTextBox, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZCalcDropEdit>(euBag.PackageQuantityCalcDropEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZCalcDropEdit>(euBag.QuantityCalcDropEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZCalcEdit>(itBag.ItemNumberCalcEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZTextBox>(euBag.Reference2TextBox, UpdateControlBehaviourAction);

		return builder.Build();
	}

	void UpdateControlBehaviourAction(Control control, NctsPreviousDocument previousDocument)
	{
		const int previousDocumentsFieldsControlWidth = 980;
		control.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.MarkAsScaled(previousDocumentsFieldsControlWidth);
	}
}
