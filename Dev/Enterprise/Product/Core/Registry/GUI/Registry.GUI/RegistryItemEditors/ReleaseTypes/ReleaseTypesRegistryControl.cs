using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	partial class ReleaseTypesRegistryControl : RegistryBusinessObjectTemplateZUserControl
	{
		public ReleaseTypesRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ReleaseTypesGrid.ReadOnly = readOnly;
		}

#if DEBUG
		internal ZGrid ReleaseTypesGridForTest
		{
			get { return ReleaseTypesGrid; }
		}
#endif

	}
}
