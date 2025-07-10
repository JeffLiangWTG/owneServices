namespace Enterprise.Customs.ES.GUI
{
	public partial class CustomsOfficesUserControl : EU.GUI.CustomsOfficesUserControl
	{
		public CustomsOfficesUserControl()
		{
			InitializeComponent();
		}

		protected override bool CustomsOfficesGridVisible => true;
	}
}
