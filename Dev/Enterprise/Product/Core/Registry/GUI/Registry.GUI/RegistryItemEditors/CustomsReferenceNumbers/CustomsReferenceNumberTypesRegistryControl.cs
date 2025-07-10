namespace Enterprise.Registry.GUI
{
	public partial class CustomsReferenceNumberTypesRegistryControl : RegistryZUserControl
	{
		public CustomsReferenceNumberTypesRegistryControl()
		{
			InitializeComponent();
		}

		public object Data
		{
			get { return DataSource; }
			set { SetDataBinding(value, null); }
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			customsReferenceNumbersGrid.ReadOnly = readOnly;
		}
	}
}
