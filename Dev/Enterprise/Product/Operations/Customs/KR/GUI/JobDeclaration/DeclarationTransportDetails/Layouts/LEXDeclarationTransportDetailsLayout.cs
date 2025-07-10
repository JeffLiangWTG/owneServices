using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class LEXDeclarationTransportDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public LEXDeclarationTransportDetailsLayout(JobDeclaration declaration)
		{
			Layout = CreateTransportDetailsLayout(declaration);
		}

		static PanelLayout CreateTransportDetailsLayout(JobDeclaration declaration)
		{
			var commonBag = Customs.GUI.TransportDetailsControlBag.Instance;
			var krBag = DeclarationTransportDetailsControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(commonBag);
			layout.RegisterControlBag(krBag);

			var captionRuler = layout.CreateRuler(120);
			var ruler = layout.CreateRuler(376);
			var columnWidthRuler = layout.CreateRightRuler(460);
			layout.Include(0, captionRuler, commonBag.OverrideValuesCheckBox);

			if (declaration.IsLocalExportToAirplane)
			{
				layout.Include(captionRuler, commonBag.FlightUserControl);
			}
			else if (declaration.IsLocalExportToSeaVessel)
			{
				layout.Include(captionRuler, commonBag.VesselCodeFindBox, columnWidthRuler);
				layout.Include(captionRuler, krBag.RadioCallSignTextBox, ruler, krBag.VoyageDurationCalcEdit);
				layout.Include(captionRuler, krBag.MRNTypeDropEdit, ruler, krBag.MRNNumberTextBox);
			}
			layout.SetVisibility<JobDeclaration>(commonBag.OverrideValuesCheckBox, h => !h.IsStandAlone);

			return layout;
		}
	}
}
