using System.Windows.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public class TransportDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public TransportDetailsLayout()
		{
			Layout = CreateLayout();
		}

		static PanelLayout CreateLayout()
		{
			var builder = new TransportDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;
			var euBag = builder.EUBag;
			builder.AddControlBag(euBag);
			var esBag = builder.ESBag;
			builder.AddControlBag(esBag);

			builder.AddColumn();

			builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
			builder.Add(esBag.MasterBillAndIATAUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.VesselUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.FlightAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfFirstArrivalUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandSeparatorUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.InlandModeOfTransportDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandModeAndTypeOfIdUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.InlandTransportDetailsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportInlandRoadUserControl, ControlWidthClass.Auto);
			builder.Add(esBag.TransportInlandRailUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.AdditionalWagonNumbersUserControl, ControlWidthClass.Auto);

			builder.AddControlBehaviour<VesselUserControl>(euBag.VesselUserControl, UpdateVesselControlCaptionBehaviourAction);
			builder.AddControlBehaviour<Customs.GUI.TransportIDAndNationalityUserControl>(commonBag.TransportIDAndNationalityUserControl, UpdateTransportIDControlCaptionBehaviourAction);
			builder.AddControlBehaviour<AdditionalWagonNumbersUserControl>(euBag.AdditionalWagonNumbersUserControl, UpdateAdditionalWagonBehaviourAction);

			void UpdateVesselControlCaptionBehaviourAction(Control control, JobDeclaration declaration)
			{
				control.FindSingle<ZCodeFindBox>("VesselCodeFindBox").CaptionResourceString = Res.GetData("{809934FB-A3AC-4EDA-B76B-4D1E39F17191}", "[21] Vessel");
			}

			void UpdateTransportIDControlCaptionBehaviourAction(Control control, JobDeclaration declaration)
			{
				control.FindSingle<ZTextBox>("TransportIDTextBox").CaptionResourceString = Res.GetData("{5E813BCF-3488-4E5A-A35A-996C112FEAFE}", "[21] Transport ID");
			}

			void UpdateAdditionalWagonBehaviourAction(Control control, JobDeclaration declaration)
			{
				control.FindSingle<ZButton>("AdditionalWagonNumbersButton").Dock = DockStyle.Right;
				control.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			}

			return builder.Build();
		}
	}
}
