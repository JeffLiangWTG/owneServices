namespace Enterprise.Registry.GUI
{
	public partial class ChargeableFactorRegistryControl : RegistryBusinessObjectTemplateZUserControl
	{
		public ChargeableFactorRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			zMetricFactorDropEdit.ReadOnly = zImperialFactorDropEdit.ReadOnly = readOnly;
		}
	}
}
