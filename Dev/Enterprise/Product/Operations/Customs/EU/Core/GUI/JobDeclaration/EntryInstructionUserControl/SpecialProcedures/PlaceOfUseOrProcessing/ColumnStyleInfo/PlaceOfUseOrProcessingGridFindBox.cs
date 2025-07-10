using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.GUI
{
	sealed class PlaceOfUseOrProcessingGridFindBox : ZGridFindBox
	{
		public PlaceOfUseOrProcessingGridFindBox()
		{
			CodeBox.ReadOnly = true;
			PopupButtonReadonlyCanBeDifferent = true;
			PopupButton.ReadOnly = false;
		}

		protected override IFindBoxPopup GetNewPopupForm() => new PlaceOfUseOrProcessingForm(PlaceOfUseOrProcessing);

		public PlaceOfUseOrProcessing PlaceOfUseOrProcessing { get; internal set; }

		protected override void OnPopupFormClosed(IFindBoxPopup popupForm)
		{
			base.OnPopupFormClosed(popupForm);
			if (!PlaceOfUseOrProcessing.DisplayText.IsEmpty)
			{
				((PlaceOfUseOrProcessingColumnStyle)ColumnStyle).CommitEditingRow();
			}
		}
	}
}
