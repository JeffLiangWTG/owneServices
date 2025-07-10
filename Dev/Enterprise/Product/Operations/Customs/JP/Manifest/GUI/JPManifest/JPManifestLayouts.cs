using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using AsycudaManifestHeader = Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public sealed class JPManifestLayouts : IPanelLayoutProvider
	{
		PanelLayout ManifestDetails { get; }

		public PanelLayout Layout => ManifestDetails;

		public JPManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		PanelLayout CreateManifestDetailsLayout()
		{
			AddControls();
			SetVisibilities();
			return Builder.Build();
		}

		void AddControls()
		{
			var common = Builder.CommonBag;
			var jpManifestControlBag = JPManifestControlBag.Instance;
			Builder.AddControlBag(jpManifestControlBag);

			Builder.AddColumn();
			builder.Add(common.ManifestTypeDropEdit, ControlWidthClass.Long);
			Builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			Builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
			Builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			Builder.Add(jpManifestControlBag.IsCoLoadedCheckBox, ControlWidthClass.Auto);
			Builder.Add(jpManifestControlBag.IsSubConsolidationCheckBox, ControlWidthClass.Auto);
			Builder.Add(common.CustomsOfficeDropEdit, ControlWidthClass.Long);
			Builder.Add(common.GoodsLocationCodeFindBox, ControlWidthClass.Long);

			Builder.AddColumn();
			builder.Add(jpManifestControlBag.CustomsAgentCodeFindBox, ControlWidthClass.Long);
			builder.Add(jpManifestControlBag.CustomsAgentCredentialGuidDropEdit, ControlWidthClass.Long);
			builder.Add(jpManifestControlBag.ViaLocationCodeFindBox, ControlWidthClass.Long);
			Builder.Add(jpManifestControlBag.PortOfLoadingUserControl, ControlWidthClass.Long);
			Builder.Add(jpManifestControlBag.PortOfDischargeUserControl, ControlWidthClass.Long);
			Builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			Builder.Add(common.RadioCallSignTextBox, ControlWidthClass.Medium);
			Builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);

			Builder.AddColumn();
			builder.Add(jpManifestControlBag.InputReferenceTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);
			Builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			Builder.Add(common.CarrierCodeTextBox, ControlWidthClass.Medium);
			Builder.Add(jpManifestControlBag.ConsolidatorUserControl, ControlWidthClass.LongNoCaption);
			Builder.Add(jpManifestControlBag.BookingNumberTextBox, ControlWidthClass.Medium);
			Builder.Add(jpManifestControlBag.MoveInDestinationCodeFindBox, ControlWidthClass.Long);
			Builder.Add(jpManifestControlBag.MasterBillMessageStatusDropEdit, ControlWidthClass.Long);
			Builder.Add(jpManifestControlBag.MasterBillCustomsStatusDropEdit, ControlWidthClass.Long);
		}

		void SetVisibilities()
		{
			var common = Builder.CommonBag;
			var jpManifestControlBag = JPManifestControlBag.Instance;
			Builder.SetVisibility(jpManifestControlBag.IsCoLoadedCheckBox, h => !h.IsNVC, h => h.AMA_ManifestTypeInfo);
			Builder.SetVisibility(jpManifestControlBag.IsSubConsolidationCheckBox, h => h.IsHCH, h => h.AMA_ManifestTypeInfo);
			Builder.SetVisibility(common.CustomsOfficeDropEdit, h => h.IsNVC || h.IsHCH, h => h.AMA_ManifestTypeInfo);
			Builder.SetVisibility(common.GoodsLocationCodeFindBox, h => !h.IsHDF && !h.IsHCH, h => h.AMA_ManifestTypeInfo);

			Builder.SetVisibility(jpManifestControlBag.ViaLocationCodeFindBox, h => h.IsSea && h.IsExport, h => h.AMA_ManifestTypeInfo);
			Builder.SetVisibility(jpManifestControlBag.PortOfLoadingUserControl, h => !h.IsNVC, h => h.AMA_ManifestTypeInfo);
			Builder.SetVisibility(jpManifestControlBag.PortOfDischargeUserControl, h => !h.IsNVC, h => h.AMA_ManifestTypeInfo);
			Builder.SetVisibility(common.VoyageFlightTextBox, h => h.IsHCH, h => h.AMA_ManifestTypeInfo);
			Builder.SetVisibility(common.RadioCallSignTextBox, h => false);
			Builder.SetVisibility(common.EstDepartureDateEdit, h => h.IsHCH, h => h.AMA_ManifestTypeInfo);

			Builder.SetVisibility(common.MessageStatusDropEdit, h => true);
			Builder.SetVisibility(common.CarrierAddressControl, h => false);
			Builder.SetVisibility(common.CarrierCodeTextBox, h => false);
			Builder.SetVisibility(jpManifestControlBag.ConsolidatorUserControl, h => h.IsHCH, h => h.AMA_ManifestTypeInfo);
			Builder.SetVisibility(jpManifestControlBag.BookingNumberTextBox, h => h.IsSea && h.IsExport, h => h.AMA_ManifestTypeInfo);
			Builder.SetVisibility(jpManifestControlBag.MoveInDestinationCodeFindBox, h => false);
			Builder.SetVisibility(jpManifestControlBag.MasterBillCustomsStatusDropEdit, h => h.IsHDF || h.IsHCH, h => h.AMA_ManifestTypeInfo);
			Builder.SetVisibility(jpManifestControlBag.MasterBillMessageStatusDropEdit, h => h.IsHDF, h => h.AMA_ManifestTypeInfo);
		}

		ManifestLayoutBuilder<AsycudaManifestHeader> Builder => builder ??= new JPManifestLayoutBuilder();
		ManifestLayoutBuilder<AsycudaManifestHeader> builder;
	}
}
