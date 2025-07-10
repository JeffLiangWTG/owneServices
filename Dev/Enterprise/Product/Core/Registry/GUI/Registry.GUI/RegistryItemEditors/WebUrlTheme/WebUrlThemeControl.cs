using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class WebUrlThemeControl : RegistryZUserControl
	{
		public WebUrlThemeControl()
		{
			InitializeComponent();
			DataSourceType = typeof(WebThemeUrlCollection);
		}

		public WebThemeUrlCollection BusinessEntity
		{
			get { return (WebThemeUrlCollection)BindingSource.Current; }
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			UrlThemeGrid.ReadOnly = readOnly;
		}
	}
}
