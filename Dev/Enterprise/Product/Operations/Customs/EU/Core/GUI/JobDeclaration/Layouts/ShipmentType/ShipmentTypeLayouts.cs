using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class ShipmentTypeLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ShipmentTypeLayouts()
		{
			Layout = CreateShipmentTypeLayout();
		}

		static PanelLayout CreateShipmentTypeLayout()
		{
			var builder = new ShipmentTypeLayoutBuilder<JobDeclaration>();

			var commonBag = builder.CommonBag;
			var euBag = ShipmentTypeControlBag.Instance;
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.EntryStyleDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.InlandModeOfTransportDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.CTStatusIDDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.IsHighValueOvrdCheckBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
