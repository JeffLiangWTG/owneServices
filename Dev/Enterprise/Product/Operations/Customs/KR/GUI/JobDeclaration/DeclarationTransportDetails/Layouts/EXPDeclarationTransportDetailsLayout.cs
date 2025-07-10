using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class EXPDeclarationTransportDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public EXPDeclarationTransportDetailsLayout(JobDeclaration declaration)
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
			var columnWidthRuler = layout.CreateRightRuler(460);
			var ruler = layout.CreateRuler(390);
			layout.Include(0, captionRuler, commonBag.OverrideValuesCheckBox);

			if (declaration.IsMail)
			{
				var ruler1 = layout.CreateRuler(254);
				var ruler2 = layout.CreateRuler(370);

				layout.Include(0, captionRuler, krBag.CarrierKRCCodeFindBox);
				layout.Include(0, captionRuler, krBag.PortOfLoadingCodeFindBox, ruler1, krBag.IATALoadPortDropEdit, ruler2, krBag.ExportDateEdit);
			}
			else if (declaration.IsAir)
			{
				layout.Include(0, captionRuler, commonBag.MasterBillTextBox);
				layout.Include(0, captionRuler, krBag.VoyageFlightNumberTextBox, krBag.FolioNumberTextBox, ruler, krBag.CarrierKRCCodeFindBox);
				layout.Include(0, captionRuler, commonBag.PortOfLoadingUserControl);

				layout.SetCaption<JobDeclaration>(krBag.CarrierKRCCodeFindBox, h => DeclarationTransportDetailsControlBag.AirCarrierCaption, h => h.JE_TransportModeInfo);
			}
			else
			{
				layout.Include(0, captionRuler, commonBag.OceanBillTextBox, columnWidthRuler);
				layout.Include(0, captionRuler, commonBag.VesselCodeFindBox, columnWidthRuler);
				layout.Include(0, captionRuler, commonBag.VoyageNumberTextBox, ruler, krBag.CarrierKRCCodeFindBox);
				layout.Include(0, captionRuler, commonBag.PortOfLoadingUserControl);

				layout.SetCaption<JobDeclaration>(krBag.CarrierKRCCodeFindBox, h => DeclarationTransportDetailsControlBag.SeaCarrierCaption, h => h.JE_TransportModeInfo);
			}

			layout.Include(0, captionRuler, commonBag.PortOfDischargeUserControl);
			layout.SetCaptions<JobDeclaration>(commonBag.PortOfLoadingUserControl
												, h => new Dictionary<string, ResourceStringData>
												{
													{ TransportDetailsPortOfLoadingUserControl.ControlNames.PortOfLoadingFindBox, Res.GetData("80E6F927-931B-4634-B3FB-A2CC5431B678", "Port of Loading") },
												}
												, h => h.JE_MessageTypeInfo);
			layout.SetVisibility<JobDeclaration>(commonBag.OverrideValuesCheckBox, h => !h.IsStandAlone);

			return layout;
		}
	}
}
