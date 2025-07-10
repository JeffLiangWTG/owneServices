using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class IMPDeclarationTransportDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public IMPDeclarationTransportDetailsLayout(JobDeclaration declaration)
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
			var ruler2 = layout.CreateRuler(390);
			var ruler3 = layout.CreateRuler(370);
			var columnWidthRuler = layout.CreateRightRuler(460);
			layout.Include(0, captionRuler, commonBag.OverrideValuesCheckBox);

			if (declaration.IsAir)
			{
				layout.Include(0, captionRuler, commonBag.MasterBillTextBox);
				layout.Include(0, captionRuler, krBag.VoyageFlightNumberTextBox, krBag.FolioNumberTextBox, ruler2, krBag.CarrierKRCCodeFindBox);
				layout.Include(0, captionRuler, krBag.VesselCountryCodeFindBox, columnWidthRuler);

				layout.SetCaption<JobDeclaration>(krBag.CarrierKRCCodeFindBox, h => DeclarationTransportDetailsControlBag.AirCarrierCaption, h => h.JE_TransportModeInfo);
				layout.SetCaption<JobDeclaration>(krBag.VesselCountryCodeFindBox, h => Res.GetData("5D64CE49-EB17-498C-959D-C28F5BD4236A", "Aircraft Country"), h => h.JE_TransportModeInfo);
			}
			else
			{
				layout.Include(0, captionRuler, commonBag.OceanBillTextBox, columnWidthRuler);
				layout.Include(0, captionRuler, commonBag.VesselCodeFindBox, columnWidthRuler);
				layout.Include(0, captionRuler, krBag.VesselCountryCodeFindBox, columnWidthRuler);
				layout.Include(0, captionRuler, commonBag.VoyageNumberTextBox, ruler2, krBag.CarrierKRCCodeFindBox);

				layout.SetCaption<JobDeclaration>(krBag.CarrierKRCCodeFindBox, h => DeclarationTransportDetailsControlBag.SeaCarrierCaption, h => h.JE_TransportModeInfo);
			}

			layout.Include(0, captionRuler, commonBag.PortOfLoadingUserControl);
			layout.Include(0, captionRuler, commonBag.PortOfDischargeUserControl);
			layout.Include(0, captionRuler, krBag.TransshipmentPortCodeFindBox, ruler3, krBag.TransshipmentDateEdit);

			layout.SetCaptions<JobDeclaration>(commonBag.PortOfDischargeUserControl
												, h => new Dictionary<string, ResourceStringData>
													{
														{ TransportDetailsPortOfDischargeUserControl.ControlNames.PortOfDischargeFindBox, DeclarationTransportDetailsControlBag.PortOfDischargeCaption },
													}
												, h => h.JE_MessageTypeInfo);
			layout.SetCaptions<JobDeclaration>(commonBag.PortOfLoadingUserControl
												, h => new Dictionary<string, ResourceStringData>
												{
													{ TransportDetailsPortOfLoadingUserControl.ControlNames.PortOfLoadingFindBox, Res.GetData("12CDE8B3-6462-4626-876C-F34DD0AA1675", "Port of Loading") },
												}
												, h => h.JE_MessageTypeInfo);
			layout.SetVisibility<JobDeclaration>(commonBag.OverrideValuesCheckBox, h => !h.IsStandAlone);

			return layout;
		}
	}
}
