using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class ManifestLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonManifestControlBag> where T : AsycudaManifestHeader
	{
		public override CommonManifestControlBag CommonBag { get; } = CommonManifestControlBag.Instance;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void SetDefaultVisibilities()
		{
			SetVisibility(CommonBag.MessageStatusTextBox, h => h.AMA_MessageStatusInfo.ReadOnly, h => h.AMA_MessageStatusInfo);
			SetVisibility(CommonBag.MessageStatusDropEdit, h => !h.AMA_MessageStatusInfo.ReadOnly, h => h.AMA_MessageStatusInfo);
			SetVisibility(CommonBag.JobReferenceTextBox, h => !h.IsStandAlone, h => h.AMA_ParentIdInfo);
			SetVisibility(CommonBag.ContainerModeDropEdit, h => !h.IsAir, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.VesselCodeFindBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.MastersNameTextBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.ConveyanceCountryCodeFindBox, h => h.IsSea || (h.IsAir && h.IsTrueAsycudaCountry), h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.VehicleRegistrationTextBox, h => h.IsRoad, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.VoyageFlightTextBox, h => !h.IsRoad, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.RadioCallSignTextBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.LloydsNumberTextBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.Trailer1RegNoTextBox, h => h.IsRoad, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.Trailer2RegNoTextBox, h => h.IsRoad, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.Trailer1RegCountryCodeFindBox, h => h.IsRoad, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.Trailer2RegCountryCodeFindBox, h => h.IsRoad, h => h.AMA_TransportModeInfo);
			SetVisibility(CommonBag.MasterBOLTextBox, h => h.MasterBOLVisible, h => h.AMA_AgentTypeInfo, h => h.AMA_RN_NKCountryInfo, h => h.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.BuyersConsolidationCheckBox, h => h.BuyersConsolidationVisible, h => h.AMA_ContainerModeInfo);
		}

		protected override void SetDefaultCaptions()
		{
			// todo: allow null caption to reset to default?
			SetCaption(
				CommonBag.MasterBOLTextBox,
				h => h.AMA_AgentType == Core.Constants.AgentType.CoLoad
					? Res.GetData("724679C7-08B9-463B-BB65-407A494050D4", "Parent Bill")
					: Res.GetData("4399B66A-FE72-48F8-99E1-319DA04BA4C0", "Master BOL"),
				h => h.AMA_AgentTypeInfo
			);
			SetCaption(CommonBag.VoyageFlightTextBox, h => h.VoyageFlightNoLabel, h => h.AMA_TransportModeInfo);
			SetCaption(CommonBag.ManifestNumberFromMasterBillTextBox, h => h.MasterBillLabel, h => h.AMA_TransportModeInfo);
		}
	}
}
