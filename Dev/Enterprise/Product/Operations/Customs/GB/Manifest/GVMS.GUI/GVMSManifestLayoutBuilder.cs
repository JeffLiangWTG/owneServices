using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GVMS.GUI
{
	public class GVMSManifestLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonManifestControlBag> where T : AsycudaManifestHeader
	{
		public override CommonManifestControlBag CommonBag => CommonManifestControlBag.Instance;

		protected override int MaxColumns => 3;

		protected override void SetDefaultVisibilities()
		{
			SetVisibility(CommonBag.ContainerModeDropEdit, h => !h.IsAir, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.VesselCodeFindBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.MastersNameTextBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.ConveyanceCountryCodeFindBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.VoyageFlightTextBox, h => !h.IsRoad, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.RadioCallSignTextBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.LloydsNumberTextBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.BuyersConsolidationCheckBox, h => h.BuyersConsolidationVisible, h => h.AMA_ContainerModeInfo);
			SetVisibility(CommonBag.VehicleRegistrationTextBox, h => true);
			SetVisibility(CommonBag.Trailer1RegNoTextBox, h => true);
			SetVisibility(CommonBag.Trailer2RegNoTextBox, h => true);
			SetVisibility(CommonBag.Trailer1RegCountryCodeFindBox, h => true);
			SetVisibility(CommonBag.Trailer2RegCountryCodeFindBox, h => true);
		}
	}
}
