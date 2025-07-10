using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ShipmentDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ShipmentDetailsLayout()
		{
			Layout = CreateShipmentDetailsLayout();
		}

		static PanelLayout CreateShipmentDetailsLayout()
		{
			var commonBag = Customs.GUI.ShipmentDetailsControlBag.Instance;
			var krBag = ShipmentDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(commonBag);
			layout.RegisterControlBag(krBag);

			var captionRuler = layout.CreateRuler(120);
			var ruler1 = layout.CreateRuler(404);
			var ruler2 = layout.CreateRuler(365);
			var columnWidthRuler = layout.CreateRightRuler(485);

			layout.Include(0, captionRuler, commonBag.HouseBillParcelPostTextBox, columnWidthRuler);
			layout.Include(0, captionRuler, krBag.OriginCodeFindBox, ruler1, krBag.EstimatedDepartureDateEdit);
			layout.Include(0, captionRuler, krBag.FinalDestinationCodeFindBox, ruler1, krBag.EstimatedArrivalDateEdit);
			layout.Include(0, captionRuler, commonBag.GoodsDescriptionTextBox, columnWidthRuler);
			layout.Include(0, captionRuler, commonBag.OwnersReferenceTextBox, columnWidthRuler);
			layout.Include(0, captionRuler, krBag.WeightCalcDropEdit, ruler2, krBag.VolumeCalcDropEdit);
			layout.Include(0, captionRuler, commonBag.TotalNoOfPacksCalcDropEdit, ruler1, commonBag.ContainerCountCalcEdit);
			layout.Include(0, captionRuler, commonBag.ShipmentDetailsIncoTermsUserControl);
			layout.Include(0, captionRuler, krBag.ShipmentDetailsScreeningUserControl);

			layout.SetVisibility<JobDeclaration>(commonBag.ContainerCountCalcEdit, h => (bool)h.IsExport && (bool)h.IsSea, h => h.JE_TransportModeInfo);

			layout.SetCaption<JobDeclaration>(commonBag.OwnersReferenceTextBox, h => h.IsImport ? Res.GetData("E89B951D-4531-4BDE-ADF9-2A20C619230F", "Owners Reference") : ShipmentDetailsControlBag.OwnersReferenceCaption, h => h.JE_TransportModeInfo);

			return layout;
		}
	}
}
