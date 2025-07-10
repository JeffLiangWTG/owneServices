using Enterprise.Customs.GUI;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public sealed class ShipmentDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout ShipmentDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ShipmentDetails;

		public ShipmentDetailsLayout()
		{
			ShipmentDetails = CreateShipmentDetailsLayout();
		}

		static PanelLayout CreateShipmentDetailsLayout()
		{
			var builder = new ShipmentDetailsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;
			var mxBag = ShipmentDetailsControlBag.Instance;
			builder.AddControlBag(mxBag);

			builder.AddColumn();
			builder.Add(commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			builder.Add(mxBag.GoodsDestinationDropEdit, ControlWidthClass.Auto);
			builder.Add(mxBag.GoodsOriginDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalNoOfPiecesCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ContainerCountCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);

			builder.SetVisibility(mxBag.GoodsDestinationDropEdit, h => h.IsImport, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(mxBag.GoodsOriginDropEdit, h => h.IsExport, h => h.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
