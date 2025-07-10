using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class ShipmentDetailsLayout : IPanelLayoutProvider
{
	PanelLayout ShipmentDetails { get; }

	public PanelLayout Layout => ShipmentDetails;

	public ShipmentDetailsLayout()
	{
		ShipmentDetails = CreateShipmentDetailsLayout();
	}

	PanelLayout CreateShipmentDetailsLayout()
	{
		var builder = new ShipmentDetailsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;

		var chBag = ShipmentDetailsControlBag.Instance;
		builder.AddControlBag(chBag);

		builder.AddColumn();
		builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(chBag.PaymentMethodUserControl, ControlWidthClass.Auto);
		builder.Add(chBag.VatPaidByUserControl, ControlWidthClass.Auto);
		builder.Add(chBag.ClearanceLocationDropEdit, ControlWidthClass.Auto);
		builder.Add(chBag.LocationOfGoodsDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.DeclarationLanguageDropEdit, ControlWidthClass.Auto);
		builder.Add(chBag.AdditionalDecisionInfoCheckBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);

		builder.SetVisibility(chBag.PaymentMethodUserControl, h => h.IsImport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(chBag.VatPaidByUserControl, h => h.IsImport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(chBag.AdditionalDecisionInfoCheckBox, h => h.IsImport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(chBag.ClearanceLocationDropEdit, h => h.IsImport, h => h.JE_MessageTypeInfo);

		return builder.Build();
	}
}
