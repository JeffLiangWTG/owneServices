using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class ShipmentTypeControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ShipmentTypeUserControl();

		public static ShipmentTypeControlBag Instance => instance ?? (instance = new ShipmentTypeControlBag());

		[ThreadStatic]
		static ShipmentTypeControlBag instance;

		ShipmentTypeControlBag()
		{
			EntryStyleDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.EntryStyleDropEdit));
			CTStatusIDDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.CTStatusIDDropEdit));
			SpecificCircumstanceDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.SpecificCircumstanceDropEdit));
			IsHighValueOvrdCheckBox = RegisterControl(nameof(ShipmentTypeUserControl.IsHighValueOvrdCheckBox));
			BorderTransportMeansDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.BorderTransportMeansDropEdit));
			IsSecurityDeclarationCheckBox = RegisterControl(nameof(ShipmentTypeUserControl.IsSecurityDeclarationCheckBox));
			SecurityDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.SecurityDropEdit));
		}

		public ControlReference EntryStyleDropEdit;
		public ControlReference CTStatusIDDropEdit;
		public ControlReference SpecificCircumstanceDropEdit;
		public ControlReference IsHighValueOvrdCheckBox;
		public ControlReference BorderTransportMeansDropEdit;
		public ControlReference IsSecurityDeclarationCheckBox;
		public ControlReference SecurityDropEdit;
	}
}
