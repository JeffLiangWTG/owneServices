using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class UnitMeasurementTextOverrideControl : RegistryZUserControl
	{
		public UnitMeasurementTextOverrideControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			UnitMeasurementTextOverrideGrid.ReadOnly = readOnly;
		}

		ZArchitecture.ZGrid UnitMeasurementTextOverrideGrid;

		public ZArchitecture.ZGrid UnitMeasurementTextOverrideGrid_ForTestOnly
		{
			get { return UnitMeasurementTextOverrideGrid; }
			set { UnitMeasurementTextOverrideGrid = value; }
		}
	}
}
