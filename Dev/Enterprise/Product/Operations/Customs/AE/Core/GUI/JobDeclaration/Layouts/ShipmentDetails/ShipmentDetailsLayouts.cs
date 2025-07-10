using Enterprise.Customs.AE.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class ShipmentDetailsLayouts : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ShipmentDetailsLayouts()
	{
		Layout = CreateShipmentDetailsLayout();
	}

	PanelLayout CreateShipmentDetailsLayout()
	{
		var builder = new ShipmentDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var aeBag = ShipmentDetailsControlBag.Instance;
		builder.AddControlBag(aeBag);

		builder.AddColumn();
		builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(aeBag.TypeOfGoodsDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.MarksAndNumbersNotePopupEdit, ControlWidthClass.Auto);
		builder.Add(aeBag.OperationalStatusDropEdit, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.TotalNoOfPiecesCalcEdit, ControlWidthClass.LongNoCaption, commonBag.TotalNoOfPacksCalcDropEdit);

		builder.AddControlBehaviour<ZStmNotePopupEditWithBindableText>(commonBag.MarksAndNumbersNotePopupEdit, UpdateMarksAndNumbersNotePopupEditControlBehaviourAction);

		return builder.Build();
	}

	void UpdateMarksAndNumbersNotePopupEditControlBehaviourAction(ZStmNotePopupEditWithBindableText marksAndNumbersNotePopupEdit, JobDeclaration declaration)
	{
		marksAndNumbersNotePopupEdit.MaximumNoteLength = 350;
	}
}
