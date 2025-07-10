using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI
{
	public class TransportDetailsLayoutBuilder : EU.GUI.Declaration.TransportDetailsLayoutBuilder<JobDeclaration>
	{
		protected override bool InlandModeOfTransportDropEditVisibility(JobDeclaration declaration) => !declaration.IsUCC6AndIsExport;

		protected override bool InlandTransportDetailsUserControlVisible(JobDeclaration declaration) => base.InlandTransportDetailsUserControlVisible(declaration) || !(declaration.IsRailInland || declaration.IsRoadInland);

		public TransportDetailsControlBag ESBag => TransportDetailsControlBag.Instance;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(ESBag.MasterBillAndIATAUserControl, x => x.IsAir, x => x.JE_TransportModeInfo);
			SetVisibility(EUBag.VesselUserControl, x => x.IsSea, x => x.JE_TransportModeInfo);
			SetVisibility(CommonBag.TransportInlandModeAndTypeOfIdUserControl, x => x.IsUCC6AndIsExport, x => x.JE_MessageTypeInfo);
			SetVisibility(ESBag.TransportInlandRailUserControl, x => x.IsUCC6 && x.IsRailInland, dependsOnTransportModeInlandAndMessageType);
			SetVisibility(CommonBag.PortOfLoadingUserControl, x => true);
			SetVisibility(EUBag.AdditionalWagonNumbersUserControl, x => x.IsUCC6 && x.IsRailInland, dependsOnTransportModeInlandAndMessageType);
		}
	}
}
