using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class ShipmentTypeLayout : IPanelLayoutProvider
	{
		PanelLayout ShipmentType { get; }

		PanelLayout IPanelLayoutProvider.Layout => ShipmentType;

		public ShipmentTypeLayout()
		{
			ShipmentType = CreateShipmentTypeLayout();
		}

		PanelLayout CreateShipmentTypeLayout()
		{
			var builder = new ShipmentTypeLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;
			var brBag = ShipmentTypeControlBag.Instance;
			builder.AddControlBag(brBag);

			builder.AddColumn();
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.MessageSubTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DeclarantTypeDropEdit, ControlWidthClass.Long);
			builder.Add(brBag.DeclarantTypeDropEdit, ControlWidthClass.Long);
			builder.Add(brBag.OperationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(brBag.BRTransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(brBag.IsMultimodalCheckBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);
			builder.Add(brBag.SpecialTransportDropEdit, ControlWidthClass.Long);
			builder.Add(brBag.DispatchModalityDropEdit, ControlWidthClass.Long);

			builder.SetVisibility(brBag.DeclarantTypeDropEdit, h => h.IsImportSiscomex, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.DispatchModalityDropEdit, h => h.RequiresDispatchModality, h => h.JE_MessageTypeInfo, h => h.JE_MessageSubTypeInfo);
			builder.SetVisibility(brBag.OperationTypeDropEdit, h => h.IsOperationTypeApplicable, h => h.JE_MessageTypeInfo, h => h.DeclarantTypeInfo);
			builder.SetVisibility(brBag.IsMultimodalCheckBox, h => h.IsMultimodalAvailable, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo, h => h.JE_MessageSubTypeInfo);
			builder.SetVisibility(brBag.SpecialTransportDropEdit, h => h.IsExport, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.MessageSubTypeDropEdit, h => h.IsImportSiscomex, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.DeclarantTypeDropEdit, h => h.IsExport, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.ContainerModeDropEdit, h => h.ContainerModeVisible, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo);

			builder.SetCaption(brBag.DispatchModalityDropEdit, h => h.DispatchModalityCaption, h => h.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
