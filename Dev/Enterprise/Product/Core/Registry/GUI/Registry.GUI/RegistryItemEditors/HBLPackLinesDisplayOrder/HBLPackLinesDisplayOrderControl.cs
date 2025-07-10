namespace Enterprise.Registry.GUI
{
	public partial class HBLPackLinesDisplayOrderControl : RegistryZUserControl
	{
		public HBLPackLinesDisplayOrderControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PresetPackLinesOrderDropEdit.ReadOnly = readOnly;
		}
	}
}
