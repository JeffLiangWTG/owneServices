using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	sealed class ManifestLayouts : IPanelLayoutProvider
	{
		public ManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		PanelLayout ManifestDetails { get; }

		PanelLayout CreateManifestDetailsLayout()
		{
			var manifestLayoutBuilder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
			var commonBag = manifestLayoutBuilder.CommonBag;

			manifestLayoutBuilder.AddColumn();
			manifestLayoutBuilder.Add(commonBag.CountryTextBox, ControlWidthClass.Medium);
			manifestLayoutBuilder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.NatureDropEdit, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.ManifestTypeDropEdit, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.ManifestNumberTextBox, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.DeclarantAddressControl, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.ActArrivalDateEdit, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.CarrierAddressControl, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.ShippingAgentAddressControl, ControlWidthClass.Long);

			manifestLayoutBuilder.AddColumn();
			manifestLayoutBuilder.Add(commonBag.JobReferenceTextBox, ControlWidthClass.Medium);
			manifestLayoutBuilder.Add(commonBag.MessageStatusTextBox, ControlWidthClass.Medium);
			manifestLayoutBuilder.Add(commonBag.MessageStatusDropEdit, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.RegistrationNumberTextBox, ControlWidthClass.Long);
			manifestLayoutBuilder.Add(commonBag.RegistrationDateEdit, ControlWidthClass.Medium);

			manifestLayoutBuilder.SetVisibility(commonBag.ManifestNumberFromMasterBillTextBox, h => h.IsAir || h.IsSea, h => h.AMA_TransportModeInfo);

			return manifestLayoutBuilder.Build();
		}
	}
}
