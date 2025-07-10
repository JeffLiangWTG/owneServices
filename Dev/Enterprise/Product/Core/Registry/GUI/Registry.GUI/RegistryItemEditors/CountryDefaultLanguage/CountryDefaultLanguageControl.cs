namespace Enterprise.Registry.GUI
{
	public partial class CountryDefaultLanguageControl : RegistryZUserControl
	{
		public CountryDefaultLanguageControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CountryDefaultLanguageGrid.ReadOnly = readOnly;
		}
	}
}
