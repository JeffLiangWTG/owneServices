using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class LEXShipmentDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public LEXShipmentDetailsLayout()
		{
			Layout = CreateLEXShipmentDetailsLayout();
		}

		static PanelLayout CreateLEXShipmentDetailsLayout()
		{
			var commonBag = Customs.GUI.ShipmentDetailsControlBag.Instance;
			var krBag = ShipmentDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(commonBag);
			layout.RegisterControlBag(krBag);

			var captionRuler = layout.CreateRuler(120);
			var ruler = layout.CreateRuler(365);
			var columnWidthRuler = layout.CreateRightRuler(485);

			layout.Include(0, captionRuler, commonBag.GoodsDescriptionTextBox, columnWidthRuler);
			layout.Include(0, captionRuler, commonBag.OwnersReferenceTextBox, columnWidthRuler);
			layout.Include(0, captionRuler, krBag.WeightCalcDropEdit, ruler, krBag.VolumeCalcDropEdit);
			layout.Include(0, captionRuler, commonBag.TotalNoOfPacksCalcDropEdit);
			layout.Include(0, captionRuler, commonBag.ShipmentDetailsIncoTermsUserControl);
			layout.Include(0, captionRuler, krBag.ShipmentDetailsScreeningUserControl);

			layout.SetCaption<JobDeclaration>(commonBag.OwnersReferenceTextBox, h => ShipmentDetailsControlBag.OwnersReferenceCaption, h => h.JE_TransportModeInfo);

			return layout;
		}
	}
}
