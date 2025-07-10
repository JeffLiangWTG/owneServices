using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class ShipmentDetailsLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ShipmentDetailsLayouts()
		{
			Layout = CreateShipmentDetailsLayout();
		}

		static PanelLayout CreateShipmentDetailsLayout()
		{
			var builder = new ShipmentDetailsLayoutBuilder<JobDeclaration>();

			var commonBag = builder.CommonBag;
			var euBag = ShipmentDetailsControlBag.Instance;
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.GoodsLocationDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.ShipmentDetailsQuantitiesUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.ShipmentDetailsCountUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.ShipmentDetailsIncoTermsPlaceUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
			builder.Add(euBag.ShipmentIncoTermPlaceTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.AgentsReferenceTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.UCRTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);

			builder.SetVisibility(euBag.ShipmentIncoTermPlaceTextBox, x => x.IsUCC6, x => x.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.AgreedPlaceCodeFindBox, x => x.IsUCC6, x => x.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.ShipmentDetailsIncoTermsPlaceUserControl, x => !x.IsUCC6, x => x.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
