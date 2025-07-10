using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public sealed class ShipmentDetailsLayouts : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ShipmentDetailsLayouts()
	{
		Layout = CreateShipmentDetailsLayout();
	}

	static PanelLayout CreateShipmentDetailsLayout()
	{
		var builder = new EU.GUI.ShipmentDetailsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.ShipmentDetailsControlBag.Instance;
		var beBag = ShipmentDetailsControlBag.Instance;
		builder.AddControlBag(euBag);
		builder.AddControlBag(beBag);

		builder.AddColumn();
		builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
		builder.Add(beBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
		builder.Add(beBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.RegionOfDestinationDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(beBag.LocationOfGoodsCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentDetailsQuantitiesUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentDetailsIncoTermsPlaceUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentIncoTermPlaceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.AgentsReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.UCRTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.DeclarationLanguageDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
		builder.Add(beBag.PresentationStartDateEdit, ControlWidthClass.Auto);

		builder.SetVisibility(beBag.LocationOfGoodsCodeFindBox, x => !(x.IsUCC6 && x.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin), x => x.JE_ApplicationCodeInfo);
		builder.SetVisibility(euBag.ShipmentIncoTermPlaceTextBox, x => x.IsUCC6, x => x.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.AgreedPlaceCodeFindBox, x => x.IsUCC6, x => x.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.ShipmentDetailsIncoTermsPlaceUserControl, x => !x.IsUCC6, x => x.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.RegionOfDestinationDropEdit, x => x.IsUCC6 && x.IsImport, x => x.JE_MessageTypeInfo);

		return builder.Build();
	}
}
