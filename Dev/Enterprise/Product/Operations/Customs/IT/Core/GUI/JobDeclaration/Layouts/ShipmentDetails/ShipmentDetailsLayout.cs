using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class ShipmentDetailsLayout : IPanelLayoutProvider
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
		var itBag = ShipmentDetailsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddControlBag(itBag);
		builder.AddColumn();

		builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.GoodsLocationDropEdit, ControlWidthClass.Auto);
		builder.Add(itBag.LocationQualifierDropEdit, ControlWidthClass.Auto);
		builder.Add(itBag.GoodsLocationDUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.GoodsLocationFUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.GoodsLocationFCUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.GoodsLocationLBLCUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.LocationOfGoodsUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.SubLocationTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentDetailsQuantitiesUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentDetailsIncoTermsPlaceUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.ShipmentDetailsUnlocoIncoTermsPlaceUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.AdditionalDeliveryTermsTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.AgentsReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.UCRTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);

		SetIncoTermsPlaceVisibility(builder, euBag, itBag);
		SetGoodsLocationVisibility(builder, euBag, itBag);

		return builder.Build();
	}

	static void SetIncoTermsPlaceVisibility(EU.GUI.ShipmentDetailsLayoutBuilder<JobDeclaration> builder, EU.GUI.ShipmentDetailsControlBag euBag, ShipmentDetailsControlBag itBag)
	{
		var incoTermsPlaceDependencies = new Func<JobDeclaration, ZPropertyInfo>[]
		{
			j => j.JE_MessageTypeInfo,
			j => j.MessageVersionInfo,
			j => j.JE_ShipmentIncoTermInfo
		};

		builder.SetVisibility(euBag.ShipmentDetailsIncoTermsPlaceUserControl, j => !j.AgreedPlaceCodeSupportAndVisible && !j.IsUCC6AndIsExport, incoTermsPlaceDependencies);
		builder.SetVisibility(euBag.ShipmentDetailsUnlocoIncoTermsPlaceUserControl, j => j.AgreedPlaceCodeSupportAndVisible, incoTermsPlaceDependencies);
		builder.SetVisibility(itBag.AdditionalDeliveryTermsTextBox, j => j.IsUcc6ExportAndIsShipmentIncoTermOther, incoTermsPlaceDependencies);
	}

	static void SetGoodsLocationVisibility(EU.GUI.ShipmentDetailsLayoutBuilder<JobDeclaration> builder, EU.GUI.ShipmentDetailsControlBag euBag, ShipmentDetailsControlBag itBag)
	{
		var locationQualifierDependencies = new Func<JobDeclaration, ZPropertyInfo>[]
		{
			j => j.JE_MessageTypeInfo,
			j => j.JE_LocationQualifierInfo,
			j => j.ZG_AuthorisationNumberInfo,
		};

		builder.SetVisibility(euBag.GoodsLocationDropEdit, j => !j.IsImport && !j.IsUCC6, locationQualifierDependencies);
		builder.SetVisibility(itBag.SubLocationTextBox, j => !j.IsUCC6, locationQualifierDependencies);

		builder.SetVisibility(itBag.LocationOfGoodsUserControl, j => j.IsUCC6AndIsExport, locationQualifierDependencies);

		builder.SetVisibility(itBag.LocationQualifierDropEdit, j => j.IsImport && !j.IsLocationQualifierValid, locationQualifierDependencies);
		builder.SetVisibility(itBag.GoodsLocationDUserControl, j => j.IsImport && j.IsLocationQualifierValidAndD, locationQualifierDependencies);
		builder.SetVisibility(itBag.GoodsLocationFUserControl, j => j.IsImport && j.IsLocationQualifierValidAndF, locationQualifierDependencies);
		builder.SetVisibility(itBag.GoodsLocationFCUserControl, j => j.IsImport && j.IsLocationQualifierValidAndFC, locationQualifierDependencies);
		builder.SetVisibility(itBag.GoodsLocationLBLCUserControl, j => j.IsImport && j.IsLocationQualifierValidAndLBorLC, locationQualifierDependencies);
	}
}
