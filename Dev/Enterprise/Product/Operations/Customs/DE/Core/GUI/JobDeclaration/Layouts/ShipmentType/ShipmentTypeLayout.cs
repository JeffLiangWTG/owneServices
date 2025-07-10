using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class ShipmentTypeLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout => ShipmentType;

		public ShipmentTypeLayout()
		{
			ShipmentType = CreateShipmentTypeLayout();
		}

		static PanelLayout CreateShipmentTypeLayout()
		{
			var builder = new Customs.GUI.ShipmentTypeLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			var deBag = ShipmentTypeControlBag.Instance;
			builder.AddControlBag(deBag);

			var euBag = EU.GUI.ShipmentTypeControlBag.Instance;
			builder.AddControlBag(euBag);

			var miscBag = Customs.GUI.CommonMiscOptionsControlBag.Instance;
			builder.AddControlBag(miscBag);

			builder.AddColumn();
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.EntryStyleDropEdit, ControlWidthClass.Long);
			builder.Add(miscBag.RepresentationDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.MethodOfPaymentDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.BorderTransportMeansDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.CTStatusIDDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.IsHighValueOvrdCheckBox, ControlWidthClass.Auto);

			builder.SetVisibility(miscBag.RepresentationDropEdit, h => !h.IsExport, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(deBag.MethodOfPaymentDropEdit, h => h.IsImport);
			builder.SetVisibility(euBag.SpecificCircumstanceDropEdit, h => !h.IsImport);
			builder.SetVisibility(euBag.IsHighValueOvrdCheckBox, h => h.IsImport);
			builder.SetCaption(deBag.BorderTransportMeansDropEdit, j => j.IsImport
				? Res.GetData("97046809-4097-452F-BDF7-7B3CD269B7DD", "Border M.O.T.", "Border Means of Transport")
				: Res.GetData("D811A0CD-4D03-48F5-AF10-1E56C12AC5EE", "Border T.O.ID.", "Border Type of ID"),
				d => d.JE_MessageTypeInfo);

			return builder.Build();
		}

		PanelLayout ShipmentType { get; }
	}
}
