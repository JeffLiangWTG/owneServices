using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public partial class TransitReferenceMappingControl : RegistryZUserControl
	{
		public TransitReferenceMappingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TransitReferenceMappingGrid.ReadOnly = readOnly;
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Registry.GUI
{
	public partial class TransitReferenceMappingControl
	{
		public ZGrid TransitReferenceMappingGrid_ForTestOnly
		{
			get { return TransitReferenceMappingGrid; }
			set { TransitReferenceMappingGrid = value; }
		}
	}
}

#endif
#endregion
