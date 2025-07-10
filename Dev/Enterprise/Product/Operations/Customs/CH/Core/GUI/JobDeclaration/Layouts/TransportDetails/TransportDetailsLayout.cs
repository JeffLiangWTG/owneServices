using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class TransportDetailsLayout : IPanelLayoutProvider
{
	PanelLayout TransportDetails { get; }

	public PanelLayout Layout => TransportDetails;

	public TransportDetailsLayout()
	{
		TransportDetails = CreateTransportDetailsLayout();
	}

	PanelLayout CreateTransportDetailsLayout()
	{
		var builder = new TransportDetailsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;

		var chBag = TransportDetailsControlBag.Instance;
		builder.AddControlBag(chBag);

		builder.AddColumn();
		builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Medium);
		builder.Add(commonBag.TransportMeansDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(chBag.VehicleTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
		builder.Add(chBag.DispatchCountryUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.GoodsDestinationCountryCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.UCRTextBox, ControlWidthClass.Auto);
		builder.Add(chBag.SpecificCircumstanceIndicatorDropEdit, ControlWidthClass.Auto);

		builder.SetVisibility(commonBag.TransportIDAndNationalityUserControl, h => !h.IsAir, h => h.JE_TransportModeInfo);
		builder.SetVisibility(commonBag.VoyageAndNationalityUserControl, h => h.IsAir, h => h.JE_TransportModeInfo);
		builder.SetVisibility(chBag.VehicleTypeDropEdit, h => h.IsRoad && h.IsImport, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo);
		builder.SetVisibility(chBag.DispatchCountryUserControl, h => h.IsImport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.GoodsDestinationCountryCodeFindBox, h => h.IsExportOrExportDeclarationActivation, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.UCRTextBox, h => h.IsExportOrExportDeclarationActivation, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(chBag.SpecificCircumstanceIndicatorDropEdit, h => h.IsExportOrExportDeclarationActivation, h => h.JE_MessageTypeInfo);

		builder.SetVisibility(commonBag.TransportMeansDropEdit, h => h.IsExportAndOwnPropulsion, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo);

		return builder.Build();
	}
}
