using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class ShipmentDetailsLayout : IPanelLayoutProvider
{
	public ShipmentDetailsLayout()
	{
		Layout = CreateShipmentDetailsLayout();
	}

	public PanelLayout Layout { get; }

	static PanelLayout CreateShipmentDetailsLayout()
	{
		var builder = new EU.GUI.ShipmentDetailsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.ShipmentDetailsControlBag.Instance;
		var esBag = ShipmentDetailsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddControlBag(esBag);
		builder.AddColumn();

		builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
		builder.Add(esBag.PartialWriteoffCheckBox, ControlWidthClass.Auto);
		builder.Add(esBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
		builder.Add(esBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
		builder.Add(esBag.DestinationStateDropEdit, ControlWidthClass.Auto);
		builder.Add(esBag.RegionOrTerritoryOfDestinationDropEdit, ControlWidthClass.Auto);
		builder.Add(esBag.RegionOrTerritoryOfDestinationCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(esBag.GoodsLocationCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentDetailsQuantitiesUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentDetailsIncoTermsPlaceUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentIncoTermPlaceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.AgentsReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.UCRTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);

		builder.SetVisibility(euBag.ShipmentIncoTermPlaceTextBox, x => x.IsUCC6, x => x.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.AgreedPlaceCodeFindBox, x => x.AgreedPlaceCodeSupportAndVisible, x => x.JE_MessageTypeInfo, x => x.JE_ShipmentIncoTermInfo);
		builder.SetVisibility(euBag.ShipmentDetailsIncoTermsPlaceUserControl, x => !x.IsUCC6, x => x.JE_MessageTypeInfo);

		builder.SetVisibility(esBag.RegionOrTerritoryOfDestinationCodeFindBox, x => x.IsUCC6AndIsImport && !x.IsGoodsDestinationESOrXCOrXLOrEmpty, x => x.JE_MessageTypeInfo, x => x.JE_GoodsDestinationInfo);
		builder.SetVisibility(esBag.RegionOrTerritoryOfDestinationDropEdit, x => x.IsUCC6AndIsImport && x.IsGoodsDestinationESOrXCOrXLOrEmpty, x => x.JE_MessageTypeInfo, x => x.JE_GoodsDestinationInfo);
		builder.SetVisibility(esBag.DestinationStateDropEdit, x => x.IsImport && !x.IsUCC6, x => x.JE_MessageTypeInfo);
		builder.SetVisibility(esBag.PartialWriteoffCheckBox, x => x.IsImport, x => x.JE_MessageTypeInfo);

		builder.AddControlBehaviour<ShipmentDetailsScreeningUserControl>(commonBag.ShipmentDetailsScreeningUserControl, ModifyShipmentDetailsScreeningUserControl);

		void ModifyShipmentDetailsScreeningUserControl(ShipmentDetailsScreeningUserControl control, JobDeclaration dec)
		{
			control.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
		}

		return builder.Build();
	}
}
