using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class TransportDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => CreateLayout();

	static PanelLayout CreateLayout()
	{
		var builder = new TransportDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.TransportDetailsControlBag.Instance;
		builder.AddControlBag(euBag);
		var itBag = TransportDetailsControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(commonBag.OverrideValuesCheckBox, ControlWidthClass.Auto);
		builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OceanBillTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.VesselCodeFindBox, ControlWidthClass.Auto);
		builder.Add(euBag.FlightAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.TransportNationalityCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportDetailsPortOfLoadingWithIATAUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfFirstArrivalUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandSeparatorUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.InlandModeOfTransportDropEdit, ControlWidthClass.Auto);
		builder.Add(itBag.InlandTransportModeAndMeansUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.InlandTransportDetailsUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandIDAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.TransportInlandAirUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportInlandRailUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.TransportInlandRoadUserControl, ControlWidthClass.Auto);

		var transportModeDependency = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_TransportModeInfo };
		var transportModeAndMessageTypeDependency = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_TransportModeInfo, x => x.JE_MessageTypeInfo };
		var messageTypeAndVersionDependency = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_MessageTypeInfo, x => x.MessageVersionInfo };
		var messageTypeVersionAndTransportModeInlandDependency = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_MessageTypeInfo, x => x.MessageVersionInfo, x => x.JE_TransportModeInlandInfo };

		bool IsUcc6ExportAndTransportModeInlandAirOrRailOrRoad(JobDeclaration declaration) => declaration.IsUCC6AndIsExport && IsTransportModeInlandAirOrRailOrRoad(declaration);
		bool IsTransportModeInlandAirOrRailOrRoad(JobDeclaration declaration) => declaration.IsAirInland || declaration.IsRailInland || declaration.IsRoadInland;

		builder.SetVisibility(commonBag.OverrideValuesCheckBox, x => !x.IsStandAlone, x => x.JE_JSInfo);
		builder.SetVisibility(commonBag.MasterBillTextBox, x => x.IsAir, transportModeDependency);
		builder.SetVisibility(commonBag.OceanBillTextBox, x => x.IsSea, transportModeDependency);
		builder.SetVisibility(commonBag.VesselCodeFindBox, x => !x.IsImport && x.IsSea, transportModeAndMessageTypeDependency);
		builder.SetVisibility(euBag.FlightAndNationalityUserControl, x => !x.IsImport && x.IsAir, transportModeAndMessageTypeDependency);
		builder.SetVisibility(commonBag.VoyageAndNationalityUserControl, x => !x.IsImport && x.IsSea, transportModeAndMessageTypeDependency);
		builder.SetVisibility(commonBag.TransportIDAndNationalityUserControl, x => !x.IsImport && !(x.IsAir || x.IsSea), transportModeAndMessageTypeDependency);
		builder.SetVisibility(commonBag.PortOfLoadingUserControl, x => !(x.IsImport && x.IsAir), transportModeAndMessageTypeDependency);
		builder.SetVisibility(commonBag.TransportDetailsPortOfLoadingWithIATAUserControl, x => x.IsImport && x.IsAir, transportModeAndMessageTypeDependency);
		builder.SetVisibility(commonBag.InlandModeOfTransportDropEdit, x => !x.IsUCC6 || IsUcc6ExportAndTransportModeInlandAirOrRailOrRoad(x), messageTypeVersionAndTransportModeInlandDependency);
		builder.SetVisibility(itBag.InlandTransportModeAndMeansUserControl, x => (x.IsUCC6AndIsExport && !IsTransportModeInlandAirOrRailOrRoad(x)) || x.IsUCC6AndIsImport, messageTypeVersionAndTransportModeInlandDependency);
		builder.SetVisibility(euBag.InlandTransportDetailsUserControl, x => !x.IsUCC6AndIsExport, messageTypeVersionAndTransportModeInlandDependency);
		builder.SetVisibility(commonBag.TransportInlandIDAndNationalityUserControl, x => x.IsUCC6AndIsExport && !IsTransportModeInlandAirOrRailOrRoad(x), messageTypeVersionAndTransportModeInlandDependency);
		builder.SetVisibility(itBag.TransportInlandAirUserControl, x => x.IsUCC6AndIsExport && x.IsAirInland, messageTypeVersionAndTransportModeInlandDependency);
		builder.SetVisibility(commonBag.TransportInlandRailUserControl, x => x.IsUCC6AndIsExport && x.IsRailInland, messageTypeVersionAndTransportModeInlandDependency);
		builder.SetVisibility(itBag.TransportInlandRoadUserControl, x => x.IsUCC6AndIsExport && x.IsRoadInland, messageTypeVersionAndTransportModeInlandDependency);
		builder.SetVisibility(euBag.TransportNationalityCodeFindBox, x => x.IsImport, transportModeAndMessageTypeDependency);

		return builder.Build();
	}
}
