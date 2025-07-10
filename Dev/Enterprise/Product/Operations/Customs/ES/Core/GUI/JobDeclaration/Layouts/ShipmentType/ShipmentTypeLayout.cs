using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	sealed class ShipmentTypeLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ShipmentTypeLayout()
		{
			Layout = CreateShipmentTypeLayout();
		}

		static PanelLayout CreateShipmentTypeLayout()
		{
			var builder = new EU.GUI.ShipmentTypeLayoutBuilder<EU.Business.Declaration.JobDeclaration>();
			var commonBag = builder.CommonBag;
			var euBag = EU.GUI.ShipmentTypeControlBag.Instance;
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.EntryStyleDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.BorderTransportMeansDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.CTStatusIDDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.IsSecurityDeclarationCheckBox, ControlWidthClass.Auto);

			builder.SetVisibility(euBag.BorderTransportMeansDropEdit, d => d.IsUCC6);
			builder.SetVisibility(euBag.SpecificCircumstanceDropEdit, d => d.IsExport, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.IsSecurityDeclarationCheckBox, d => d.IsSecurityAllowed(), d => d.JE_MessageTypeInfo, d => d.JE_EntryStyleInfo);

			builder.SetCaption(euBag.BorderTransportMeansDropEdit, r => r != null && r.IsImport ? Res.GetData("D35525E9-0DC9-4403-B13C-488FAA38397F", "Arr. MOT", "Arrival M.O.T.", "Arrival M.O.T.", "Arrival Method of Transport") : Res.GetData("5BF14E5B-961F-4148-9883-4137ECF16375", "Border M.O.T."));

			return builder.Build();
		}
	}
}
