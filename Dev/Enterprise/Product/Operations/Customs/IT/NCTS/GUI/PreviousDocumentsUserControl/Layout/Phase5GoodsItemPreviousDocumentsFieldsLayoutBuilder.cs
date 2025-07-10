using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class Phase5GoodsItemPreviousDocumentsFieldsLayoutBuilder : Ucc6ExportPreviousDocumentsFieldsLayoutBuilder<NctsPreviousDocument>
{
	protected override void SetDefaultCaptions()
	{
		base.SetDefaultCaptions();

		SetCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.Reference2TextBox
			, _ => Res.GetData("25C0F36B-B58D-4275-8C89-0E35DB1C1DDA"
				, englishCaption: "Complement of Information"
				, englishFullDescription: "[12 01 002 000] Complement of Information"));
	}
}
