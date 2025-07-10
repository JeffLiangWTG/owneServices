using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class Ucc6ExportPreviousDocumentsFieldsLayoutBuilder<T> : EU.GUI.PlugIn.PreviousDocumentsFieldsLayoutBuilder<T>
	where T : PreviousDocument
{
	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override void SetDefaultCaptions()
	{
		base.SetDefaultCaptions();
		SetCodeDropEditCaption();
		SetReferenceTextBoxCaption();
		SetPackageQuantityCalcDropEditCaption();
		SetItemNumberCalcEditCaption();
	}

	void SetCodeDropEditCaption()
	{
		SetCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit
			, _ => Res.GetData("61C37362-5AB8-4146-8C61-91D362E75568"
				, englishCaption: "Type"
				, englishFullDescription: "[12 01 002 000] Type"));
	}

	void SetReferenceTextBoxCaption()
	{
		SetCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.ReferenceTextBox
			, _ => Res.GetData("D4133C0E-A350-4F95-A4D2-83FE8855A0F1"
				, englishCaption: "Reference Number"
				, englishFullDescription: "[12 01 001 000] Reference Number"));
	}

	void SetPackageQuantityCalcDropEditCaption()
	{
		SetCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.PackageQuantityCalcDropEdit
			, _ => Res.GetData("A7C30071-B498-4939-B8A1-E92697962C6F"
				, englishCaption: "Number of Packages"
				, englishFullDescription: "[12 01 004 000] Number of Packages"
				, englishShortCaption: "Package No."));
	}

	void SetItemNumberCalcEditCaption()
	{
		SetCaption(PreviousDocumentsFieldsControlBag.Instance.ItemNumberCalcEdit
			, _ => Res.GetData("B50E1EDF-75FD-4201-8528-D436E9575327"
				, englishCaption: "Goods Item Identifier"
				, englishFullDescription: "[12 01 007 000] Goods Item Identifier"
				, englishShortCaption: "Item No."));
	}
}
